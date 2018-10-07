
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
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Test.Exico.Shopify.Web.Core
{
    [Collection("Sequential")]
    public class Test_Dashboard_controller
    {
        private long PrivilegedIp = 167772170;

        [Fact]
        public void support_should_return_valid_view_result()
        {
            //arrange
            InitAllMocks();            
            SetupDbComand();
            SetupFindSingleWhere(false);
            SetupPlanReader();
            var c = InitController();
            SetupContext(c);
            userCaching.Setup(x => x.GetLoggedOnUser(It.IsAny<bool>())).ReturnsAsync(new AppUser(new AspNetUser() {MyShopifyDomain="test.myshopify.com" },false));
            //act
            var result = c.Support().Result;

            //assert    
            //repo.VerifyAll();
            userCaching.VerifyAll();
            planReader.VerifyAll();
            //userService.VerifyAll();

            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);

            var vResult = (ViewResult)result;
            Assert.NotNull(vResult.Model);
            Assert.IsType<ContactUsViewModel>(vResult.Model);
            var model = vResult.Model as ContactUsViewModel;
            Assert.False(string.IsNullOrEmpty(model.PlanName));
            Assert.False(string.IsNullOrEmpty(model.ShopDomain));


        }

        [Fact]
        public void send_message_should_send_message_and_do_valid_redirect()
        {
            //arrange
            InitAllMocks();
            SetupCacheGetLoggedUser(false);
            SetupSupportEmail();
            webMsg.Setup(x => x.AddTempSuccessPopUp(It.IsAny<Controller>(), It.IsAny<object>(), It.IsAny<bool>(), It.IsAny<bool>()));
            var c = InitController();

            //act
            var result = c.SendMsg(new ContactUsViewModel()).Result;

            //assert
            userCaching.VerifyAll();
            emailer.VerifyAll();
            webMsg.VerifyAll();

            Assert.NotNull(result);
            Assert.IsType<RedirectToActionResult>(result);
            var rResult = result as RedirectToActionResult;
            Assert.Equal(settings.GetAppDashboardControllerName(), rResult.ControllerName);
            Assert.Equal(DASHBOARD_ACTIONS.Support.ToString(), rResult.ActionName);


        }

        [Fact]
        public void consider_upgrading_plan_should_return_a_valid_view_result()
        {
            InitAllMocks();
            var c = InitController();
            var result = c.ConsiderUpgradingPlan().Result;

            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);

        }

        [Fact]
        public void index_should_return_a_valid_view_result()
        {
            InitAllMocks();
            var c = InitController();
            var result = c.Index().Result;

            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);

        }

        [Fact]
        public void plan_doesn_not_allow_should_return_a_valid_view_result()
        {
            InitAllMocks();
            var c = InitController();
            var result = c.PlanDoesNotAllow().Result;

            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);

        }

        #region class variables

        Mock<IPlansReader> planReader;
        Mock<IUserCaching> userCaching;
        Mock<IShopifyEventsEmailer> emailer;
        Mock<IShopifyApi> shopifyApi;
        Mock<IConfiguration> config;
        IDbSettingsReader settings;
        Mock<IWebMessenger> webMsg;
        Mock<IDbRepository<AspNetUser>> repo;
        Mock<IDbService<AspNetUser>> userService;

        #endregion

        #region Helper Methods

        private void SetupCacheGetLoggedUser(bool throwsException)
        {
            if (throwsException)
            {
                userCaching.Setup(x => x.GetLoggedOnUser(It.IsAny<bool>())).ThrowsAsync(new Exception("This is a fake error on  cache."));
            }
            else
            {
                userCaching.Setup(x => x.GetLoggedOnUser(It.IsAny<bool>())).ReturnsAsync(It.IsAny<AppUser>());
            }
        }

        private void SetupPlanReader()
        {
            planReader.Setup(x => x[It.IsAny<int>()]).Returns(new PlanAppModel()
            {
                Id = 1,
                Name = "A valid plan"
            });
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

        private void SetupSupportEmail()
        {
            emailer.Setup(x => x.SendSupportEmailAsync(It.IsAny<AppUser>(), It.IsAny<ContactUsViewModel>())).ReturnsAsync(true);
        }

        private Mock<HttpContext> SetupContext(Controller c, long? remoteIp = null)
        {
            var httpCtx = new Mock<HttpContext>();
            var connection = new Mock<ConnectionInfo>();
            httpCtx.Setup(x => x.Connection).Returns(connection.Object);
            httpCtx.Setup(x => x.User).Returns(new ClaimsPrincipal(new
                        ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, "a-user"),
                            new Claim(ClaimTypes.NameIdentifier,Guid.NewGuid().ToString())
                        }, "SomeAuthTYpe")));
            //167772161 = 10.0.0.10
            connection.Setup(x => x.RemoteIpAddress).Returns(new System.Net.IPAddress(remoteIp.HasValue ? remoteIp.Value : PrivilegedIp));
            //this setup is for UserInContextHelper.GetCurrentUserId
            c.ControllerContext = new ControllerContext()
            {
                HttpContext = httpCtx.Object
            };

            return httpCtx;


        }

        private void InitAllMocks()
        {
            planReader = new Mock<IPlansReader>();
            userCaching = new Mock<IUserCaching>();
            emailer = new Mock<IShopifyEventsEmailer>();
            shopifyApi = new Mock<IShopifyApi>();
            config = new Mock<IConfiguration>();
            webMsg = new Mock<IWebMessenger>();
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

        private MyDashboardController InitController()
        {
            settings = GetSettings();
            var logger = new Mock<ILogger<MyDashboardController>>().Object;
            MyDashboardController c = new MyDashboardController(
                                                webMsg.Object,
                                                emailer.Object,
                                                userService.Object,
                                                userCaching.Object,
                                                planReader.Object,
                                                config.Object,
                                                settings,
                                                logger);
            return c;
        }

        #endregion

    }


    #region helper classes
    public class MyDashboardController : ABaseAppDashBoardController
    {
        public MyDashboardController(

            IWebMessenger webMsg,
            IShopifyEventsEmailer emailer,
            IDbService<AspNetUser> userService,
            IUserCaching userCache,
            IPlansReader planReader,
            IConfiguration config,
            IDbSettingsReader settings,
            ILogger<MyDashboardController> logger) : base(
            webMsg,
            emailer,
            userService,
            userCache,
            planReader,
            config,
            settings,
            logger)
        {
        }

        protected override string GetPageTitle()
        {
            throw new NotImplementedException();
        }
    }

    #endregion

}

