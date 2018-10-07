
using Exico.Shopify.Data;
using Exico.Shopify.Data.Domain.AppModels;
using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Domain.ShopifyApiModels;
using Exico.Shopify.Data.Domain.ViewModels;
using Exico.Shopify.Data.Framework;
using Exico.Shopify.Web.Core.Controllers;
using Exico.Shopify.Web.Core.Controllers.BaseControllers;
using Exico.Shopify.Web.Core.Controllers.BaseControllers.Interfaces;
using Exico.Shopify.Web.Core.Helpers;
using Exico.Shopify.Web.Core.Modules;
using Exico.Shopify.Web.Core.Plugins.WebMsg;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Test.Exico.Shopify.Web.Core
{
    [Collection("Sequential")]
    public class Test_Uninstall_Controller
    {

        public Test_Uninstall_Controller()
        {
            settings = GetSettings();
        }


        [Fact]
        public void app_uninstalled_should_be_able_to_detect_un_aythentic_request()
        {
            //arrange
            InitAllMocks();
            SetupIsAuthenticWebhook(false);
            var c = InitController();

            //act
            var result = c.AppUninstalled("1").Result;

            //assert
            Assert.NotNull(result);
            var cResult = (ContentResult)result;
            Assert.Contains("Webhook is not authentic.", cResult.Content);
            shopifyApi.VerifyAll();
        }

        [Fact]
        public void app_uninstalled_should_return_valid_content_even_when_user_is_not_found()
        {
            InitAllMocks();
            SetupIsAuthenticWebhook(true);
            SetupFindSingleWhere(true);//that means,  user = null 
            SetupEmailer();
            var c = InitController();

            //act
            var result = c.AppUninstalled("1").Result;

            //assert
            shopifyApi.VerifyAll();
            userService.VerifyAll();
            emailer.VerifyAll();
            Assert.NotNull(result);
            var cResult = (OkResult)result;
            Assert.Equal(cResult.StatusCode, (int)HttpStatusCode.OK);
            

        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void app_uninstalled_should_return_valid_content_when_user_is_found_and_regardless_user_deletion_is_successfull_or_not(bool userDeleteSuccess)
        {
            InitAllMocks();
            SetupIsAuthenticWebhook(true);
            SetupDbComand(true);
            SetupFindSingleWhere(false);//that means,  user = null             
            SetupEmailer();
            SetupCacheClearUser(userDeleteSuccess);//it wont matter if it fails or passes
            SetupRemoveUser(!userDeleteSuccess);
            var c = InitController();

            //act
            var result = c.AppUninstalled("1").Result;

            //assert
            shopifyApi.VerifyAll();
            userService.VerifyAll();
            emailer.VerifyAll();
            repo.VerifyAll();
            userCaching.VerifyAll();
            Assert.NotNull(result);
            var cResult = (OkResult)result;
            Assert.Equal(cResult.StatusCode, (int)HttpStatusCode.OK);
            if (userDeleteSuccess)
            {
                Assert.True(c.UserIsDeleted_is_called);
                Assert.False(c.CouldNotDeleteUser_is_called);
            }
            else
            {
                Assert.False(c.UserIsDeleted_is_called);
                Assert.True(c.CouldNotDeleteUser_is_called);
            }

        }



        #region class variables                
        Mock<IUserCaching> userCaching;
        Mock<IShopifyEventsEmailer> emailer;
        Mock<IShopifyApi> shopifyApi;
        Mock<IConfiguration> config;
        IDbSettingsReader settings;
        Mock<IDbRepository<AspNetUser>> repo;
        Mock<IDbService<AspNetUser>> userService;
        #endregion

        #region Helper Methods

        private void SetupIsAuthenticWebhook(bool returns, bool throwsError = false)
        {
            if (throwsError == false)
            {
                shopifyApi.Setup(x => x.IsAuthenticWebhook(It.IsAny<HttpRequest>())).ReturnsAsync(returns);
            }
            else
            {
                shopifyApi.Setup(x => x.IsAuthenticWebhook(It.IsAny<HttpRequest>())).ThrowsAsync(new Exception("Fake exception it is."));
            }
        }
        private void SetupDbComand(bool isAdmin = false)
        {
            //setup for UserDbServiceHelper.GetAppUserByIdAsync() because it uses execute scaler
            MyDbConnection con = new MyDbConnection();
            //setup command
            MyDbCommand command = new MyDbCommand(isAdmin ? 1 : 0/*not admin*/);
            command.Connection = con;
            //setup repository            
            repo.Setup(x => x.CreateDbCommand(It.IsAny<CommandType>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>())).Returns(command);
            //setup service
            //Mock<IDbService<AspNetUser>> service = new Mock<IDbService<AspNetUser>>();
            userService.Setup(x => x.GetRepo()).Returns(repo.Object);
        }

        public void SetupEmailer()
        {
            emailer.Setup(x => x.UserUnInstalledAppAsync(It.IsAny<AppUser>())).ReturnsAsync(true);
        }

        private void SetupFindSingleWhere(bool throwsException)
        {
            if (throwsException)
            {
                userService.Setup(x => x.FindSingleWhere(It.IsAny<Expression<Func<AspNetUser, bool>>>())).Throws(new Exception("It is a fake db exception"));
            }
            else
            {
                userService.Setup(x => x.FindSingleWhere(It.IsAny<Expression<Func<AspNetUser, bool>>>())).Returns(new AspNetUser()
                {
                    MyShopifyDomain = "myshop.myshopify.com",
                    ShopifyAccessToken = "valid-token",
                    Email = "me@myshop.com",
                    PlanId = 1,
                    Id = "1",
                });
            }
        }

        private void SetupRemoveUser(bool throwsException)
        {
            if (throwsException)
            {
                userService.Setup(x => x.Delete(It.IsAny<object>())).Throws(new Exception("It is a fake db exception for delete"));
            }
            else
            {
                userService.Setup(x => x.Delete(It.IsAny<object>())).Returns(true);
            }
        }

        private void SetupCacheClearUser(bool throwsException)
        {
            if (throwsException)
            {
                userCaching.Setup(x => x.ClearUser(It.IsAny<string>())).Throws(new Exception("This is a fake error on cealr cache."));
            }
            else
            {
                userCaching.Setup(x => x.ClearUser(It.IsAny<string>()));
            }
        }
        private void InitAllMocks()
        {
            userCaching = new Mock<IUserCaching>();
            emailer = new Mock<IShopifyEventsEmailer>();
            shopifyApi = new Mock<IShopifyApi>();
            config = new Mock<IConfiguration>();
            repo = new Mock<IDbRepository<AspNetUser>>();
            userService = new Mock<IDbService<AspNetUser>>();
        }
        private IDbSettingsReader GetSettings()
        {
            var options = new DbContextOptionsBuilder<ExicoShopifyDbContext>()
                .UseInMemoryDatabase("settings_db_my_profile")
                .Options;

            ExicoShopifyDbContext testContext = new ExicoShopifyDbContext(options);
            //emptying
            testContext.Database.EnsureDeleted();
            ////recreating
            testContext.Database.EnsureCreated();
            //seeding

            testContext.SystemSettings.Add(new SystemSetting()
            {
                Value = "shopify",
                GroupName = "CORE",
                SettingName = CORE_SYSTEM_SETTING_NAMES.SHOPIFY_CONTROLLER.ToString()
            });

            testContext.SystemSettings.Add(new SystemSetting()
            {
                Value = "myprofile",
                GroupName = "CORE",
                SettingName = CORE_SYSTEM_SETTING_NAMES.MY_PROFILE_CONTOLLER.ToString()
            });

            testContext.SaveChanges();
            var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
            var factory = serviceProvider.GetService<Microsoft.Extensions.Logging.ILoggerFactory>();
            ILogger<IDbSettingsReader> logger = factory.CreateLogger<IDbSettingsReader>();
            var m = new MemoryCache(new MemoryCacheOptions() { });
            var service = new TestService<SystemSetting>(new TestRepository<SystemSetting>(new TestUnitOfWork(testContext)));
            DbSettingsReader r = new DbSettingsReader(service, m, logger);
            return r;

        }
        private MyUninstaller InitController()
        {
            settings = GetSettings();
            var logger = new Mock<ILogger<MyUninstaller>>().Object;
            MyUninstaller c = new MyUninstaller(
                            userCaching.Object,
                            emailer.Object,
                            shopifyApi.Object,
                            userService.Object,
                            config.Object,
                            settings,
                            logger
                         );
            return c;
        }
        #endregion
    }


    #region helper classes
    public class MyUninstaller : ABaseAppUninstallController
    {
        public bool UserIsDeleted_is_called = false;
        public bool CouldNotDeleteUser_is_called = false;
        public MyUninstaller(
            IUserCaching userCache,
            IShopifyEventsEmailer emailer,
            IShopifyApi shopifyApi,
            IDbService<AspNetUser> userService,
            IConfiguration config,
            IDbSettingsReader settings,
            ILogger<MyUninstaller> logger) : base(
            userCache,
            emailer,
            shopifyApi,
            userService,
            config,
            settings,
            logger)
        {
            UserIsDeleted_is_called = false;
            CouldNotDeleteUser_is_called = false;
        }

        public override async Task CouldNotDeleteUser(AppUser user, Exception ex)
        {
            CouldNotDeleteUser_is_called = true;
        }

        public override async Task UserIsDeleted(AppUser user)
        {
            UserIsDeleted_is_called = true;
        }

        protected override string GetPageTitle()
        {
            throw new NotImplementedException();
        }
    }

    #endregion  

}
