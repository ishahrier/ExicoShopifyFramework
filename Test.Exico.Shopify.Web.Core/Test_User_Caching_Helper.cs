using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Web.Core.Extensions;
using Exico.Shopify.Web.Core.Helpers;
using Exico.Shopify.Web.Core.Modules;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Test.Exico.Shopify.Web.Core
{
    [Collection("Sequential")]
    public class Test_User_Caching_Helper
    {
        private ServiceProvider serviceProvider;

        public Test_User_Caching_Helper()
        {
            //UsrMgr = BuildUserManager<AspNetUser>();
            var services = new ServiceCollection();
            var accessor = new HttpContextAccessor()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = null /*no user is authticated yet*/
                }
            };
            services.AddSingleton<IHttpContextAccessor>(accessor);
            services.AddMemoryCache();
            services.AddLogging();
            services.AddDbContext<ExicoIdentityDbContext>(
             options => options.UseInMemoryDatabase("caching_test_db"));
            services.AddTransient<UserInContextHelper>();
            serviceProvider = services.BuildServiceProvider();

            accessor.HttpContext.RequestServices = serviceProvider;

            ExicoIdentityDbContext Dbcontext;
            Dbcontext = serviceProvider.GetService<ExicoIdentityDbContext>();
            Dbcontext.Database.EnsureDeleted();
            Dbcontext.Database.EnsureCreated();


            //var dbOptions = new DbContextOptionsBuilder<ExicoIdentityDbContext>().UseInMemoryDatabase("user_store_db").Options;
            //Dbcontext = new ExicoIdentityDbContext(dbOptions);



        }

        [Fact]
        public async Task User_Cache_Should_Return_Null_If_User_Not_Found_In_Storage()
        {

            var loggedUserId = Guid.NewGuid().ToString();
            var Logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<UserCaching>();
            var UsrMgr = BuildUserManager<AspNetUser>();
            var cache = serviceProvider.GetService<IMemoryCache>();
            var httpAccessor = serviceProvider.GetService<IHttpContextAccessor>();

            //that use was once logged in but then got deleted somehow
            httpAccessor.HttpContext.User = new ClaimsPrincipal(new
                        ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, "a-deleted-user"),
                            new Claim(ClaimTypes.NameIdentifier,"not-t-be-found")
                        }, "SomeAuthTYpe"));

            var signInManager = new FakeSignInManager(httpAccessor);
            UserCaching r = new UserCaching(signInManager, httpAccessor, cache, UsrMgr, Logger);
            Assert.Null(await r.SetLoggedOnUserInCache());
        }

        [Fact]
        public void User_Cache_Should_Return_Data_If_User_In_Storage_And_Logged_In()
        {
            var Logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<UserCaching>();
            var loggeUserId = Guid.NewGuid().ToString();
            var UsrMgr = BuildUserManager<AspNetUser>();
            var cache = serviceProvider.GetService<IMemoryCache>();
            var httpAccessor = serviceProvider.GetService<IHttpContextAccessor>();

            var user = new AspNetUser()
            {
                UserName = "test_User",
                Email = "test@gmail.com",
                Id = loggeUserId
            };
            UsrMgr.CreateAsync(user);//saving in db

            httpAccessor.HttpContext.User = new ClaimsPrincipal(new
                        ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim(ClaimTypes.NameIdentifier,user.Id)
                        }, "SomeAuthTYpe"));

            var signInManager = new FakeSignInManager(httpAccessor);
            UserCaching r = new UserCaching(signInManager, httpAccessor, cache, UsrMgr, Logger);

            var cachedUsr = r.SetLoggedOnUserInCache().Result;
            Assert.NotNull(cachedUsr);
            Assert.Equal(user.Id, cachedUsr.Id);
        }

        [Fact]
        public void User_Cache_Should_Return_Null_If_User_NOT_Logged_In()
        {
            var Logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<UserCaching>();
            var loggeUserId = Guid.NewGuid().ToString();
            var UsrMgr = BuildUserManager<AspNetUser>();
            var cache = serviceProvider.GetService<IMemoryCache>();
            var httpAccessor = serviceProvider.GetService<IHttpContextAccessor>();

            var user = new AspNetUser()
            {
                UserName = "test_User",
                Email = "test@gmail.com",
                Id = loggeUserId
            };
            UsrMgr.CreateAsync(user);//saving in db
            httpAccessor.HttpContext.User = null;
            var signInManager = new FakeSignInManager(httpAccessor);
            UserCaching r = new UserCaching(signInManager, httpAccessor, cache, UsrMgr, Logger);

            var cachedUsr = r.SetLoggedOnUserInCache().Result;
            Assert.Null(cachedUsr);
        }

        [Fact]
        public void Getting_From_User_Cache_should_cache_on_call_If_SetLoggedOnUserInCache_is_not_already_called_on_logged_user()
        {
            var Logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<UserCaching>();
            var loggeUserId = Guid.NewGuid().ToString();
            var UsrMgr = BuildUserManager<AspNetUser>();
            var cache = serviceProvider.GetService<IMemoryCache>();
            var httpAccessor = serviceProvider.GetService<IHttpContextAccessor>();

            var user = new AspNetUser()
            {
                UserName = "test_User",
                Email = "test@gmail.com",
                Id = loggeUserId
            };
            UsrMgr.CreateAsync(user);//saving in db

            httpAccessor.HttpContext.User = new ClaimsPrincipal(new
                        ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim(ClaimTypes.NameIdentifier,user.Id)
                        }, "SomeAuthTYpe"));

            var signInManager = new FakeSignInManager(httpAccessor);
            UserCaching r = new UserCaching(signInManager, httpAccessor, cache, UsrMgr, Logger);
            var cached = r.GetLoggedOnUser().Result;
            Assert.NotNull(cached);
            Assert.Equal(user.Id, cached.Id);
        }

        [Fact]
        public async Task Should_remove_key_and_data_on_clear_logged_on_user()
        {
            var Logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<UserCaching>();
            var loggeUserId = Guid.NewGuid().ToString();
            var UsrMgr = BuildUserManager<AspNetUser>();
            var cache = serviceProvider.GetService<IMemoryCache>();
            var httpAccessor = serviceProvider.GetService<IHttpContextAccessor>();

            var user = new AspNetUser()
            {
                UserName = "test_User",
                Email = "test@gmail.com",
                Id = loggeUserId
            };
            await UsrMgr.CreateAsync(user);//saving in db

            httpAccessor.HttpContext.User = new ClaimsPrincipal(new
                        ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim(ClaimTypes.NameIdentifier,user.Id)
                        }, "SomeAuthTYpe"));

            var signInManager = new FakeSignInManager(httpAccessor);
            UserCaching r = new UserCaching(signInManager, httpAccessor, cache, UsrMgr, Logger);
            await r.SetLoggedOnUserInCache();
            var cachedUsr = await r.GetLoggedOnUser();
            Assert.NotNull(cachedUsr);
            Assert.Equal(user.Id, cachedUsr.Id);
            r.ClearLoggedOnUser();
            //cachedUsr = await r.GetLoggedOnUser();
            //Assert.Null(cachedUsr);
            var keyIsNUll = cache.Get(r.GetUserCacheKey(cachedUsr.Id));
            Assert.Null(keyIsNUll);
        }

        [Fact]
        public void Test_User_Caching_keys_Are_Correctly_Generating()
        {
            var Logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<UserCaching>();
            var loggeUserId = Guid.NewGuid().ToString();
            var UsrMgr = BuildUserManager<AspNetUser>();
            var cache = serviceProvider.GetService<IMemoryCache>();
            var httpAccessor = serviceProvider.GetService<IHttpContextAccessor>();

            var user = new AspNetUser()
            {
                UserName = "test_User",
                Email = "test@gmail.com",
                Id = loggeUserId
            };
            UsrMgr.CreateAsync(user);//saving in db

            httpAccessor.HttpContext.User = new ClaimsPrincipal(new
                        ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim(ClaimTypes.NameIdentifier,user.Id)
                        }, "SomeAuthTYpe"));

            var signInManager = new FakeSignInManager(httpAccessor);
            UserCaching r = new UserCaching(signInManager, httpAccessor, cache, UsrMgr, Logger);

            var cachedUsr = r.SetLoggedOnUserInCache().Result;

            Assert.NotNull(cachedUsr);

            Assert.Equal(r.GetUserCacheKey(user.Id), r.GetLoggedOnUserCacheKey());
        }


        private void SetUserInContext(IHttpContextAccessor httpAccessor, string uname, string uid)
        {
            httpAccessor.HttpContext.User = new ClaimsPrincipal(new
            ClaimsIdentity(new Claim[]
            {
                            new Claim(ClaimTypes.Name, "uname"),
                            new Claim(ClaimTypes.NameIdentifier,uname)
            }, "SomeAuthTYpe"));
        }
        private UserManager<TUser> BuildUserManager<TUser>(IUserStore<TUser> store = null) where TUser : AspNetUser, new()
        {


            ExicoIdentityDbContext Dbcontext = serviceProvider.GetService<ExicoIdentityDbContext>();
            var us = new UserStore<TUser>(Dbcontext);

            store = store ?? new Mock<IUserStore<TUser>>().Object;

            var options = new Mock<IOptions<IdentityOptions>>();
            var idOptions = new IdentityOptions();
            idOptions.Lockout.AllowedForNewUsers = false;
            options.Setup(o => o.Value).Returns(idOptions);

            var userValidators = new List<IUserValidator<TUser>>();
            var validator = new Mock<IUserValidator<TUser>>();
            userValidators.Add(validator.Object);

            var pwdValidators = new List<PasswordValidator<TUser>>();
            pwdValidators.Add(new PasswordValidator<TUser>());

            var userManager = new UserManager<TUser>
                (
                 us,
                 options.Object,
                 new PasswordHasher<TUser>(),
                 userValidators,
                 pwdValidators,
                 new UpperInvariantLookupNormalizer(),
                 new IdentityErrorDescriber(),
                 null,
                 new Mock<ILogger<UserManager<TUser>>>().Object);

            validator.Setup(v => v.ValidateAsync(userManager, It.IsAny<TUser>()))
                .Returns(Task.FromResult(IdentityResult.Success)).Verifiable();

            return userManager;
        }
    }
}
