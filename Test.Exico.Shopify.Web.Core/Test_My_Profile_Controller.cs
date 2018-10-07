
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
    public class Test_My_Profile_Controller
    {

        [Fact]
        public void index_should_return_valid_view_for_admin_with_disconnected_billing()
        {
            //arrange
            InitAllMocks();
            userCaching.Setup(x => x.GetLoggedOnUser(false)).ReturnsAsync(new AppUser(new AspNetUser()
            {
                Id = "1",
                PlanId = 1,
                ShopifyChargeId = null /*disconnected billing*/
            }, true));

            planReader.Setup(x => x[1]).Returns(new PlanAppModel()
            {
                Id = 1
            });

            //act
            var c = InitController();
            var result = c.Index().Result;

            //assert 

            shopifyApi.VerifyAll();
            planReader.VerifyAll();
            userCaching.VerifyAll();

            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);

            var vResult = (ViewResult)result;
            Assert.NotNull(vResult.Model);
            Assert.IsType<MyProfileViewModel>(vResult.Model);

            var model = (MyProfileViewModel)vResult.Model;

            Assert.NotNull(model.Me);
            Assert.NotNull(model.MyPlan);
            Assert.Null(model.ChargeData);

            Assert.Equal("1", model.Me.Id);
            Assert.Equal(1, model.MyPlan.Id);




        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void index_should_return_valid_view_for_connected_billing(bool isAdmin)
        {
            //arrange
            InitAllMocks();
            userCaching.Setup(x => x.GetLoggedOnUser(false)).ReturnsAsync(new AppUser(new AspNetUser()
            {
                Id = "1",
                PlanId = 1,
                ShopifyChargeId = 1 /*always connected billing*/
            }, isAdmin));

            planReader.Setup(x => x[1]).Returns(new PlanAppModel()
            {
                Id = 1
            });

            var obj = new ShopifyRecurringChargeObject()
            {
                Id = 12345,
            };
            shopifyApi.Setup(x => x.GetRecurringChargeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(obj);

            //act
            var c = InitController();
            var result = c.Index().Result;

            //assert 

            if (isAdmin == false)/*because admin==true && billing is always connected, in that situation we will not be computed*/
                shopifyApi.Verify(x => x.GetRecurringChargeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>()));

            //shopifyApi.VerifyAll();
            planReader.VerifyAll();
            userCaching.VerifyAll();

            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);

            var vResult = (ViewResult)result;
            Assert.NotNull(vResult.Model);
            Assert.IsType<MyProfileViewModel>(vResult.Model);

            var model = (MyProfileViewModel)vResult.Model;

            Assert.NotNull(model.Me);
            Assert.NotNull(model.MyPlan);
            if (isAdmin == false) Assert.NotNull(model.ChargeData);
            if (isAdmin == true) Assert.Null(model.ChargeData);

            Assert.Equal("1", model.Me.Id);
            Assert.Equal(1, model.MyPlan.Id);
            if (isAdmin == false) Assert.Equal(12345, model.ChargeData.Id);

        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void upgrade_plan_should_redirect_to_valid_url_if_user_can_not_upgrade_anymore(bool proceed, bool isadmin)
        {
            //arrange
            InitAllMocks();
            userCaching.Setup(x => x.GetLoggedOnUser(false)).ReturnsAsync(new AppUser(new AspNetUser()
            {
                Id = "1",
                PlanId = 1,
                ShopifyChargeId = 1 /*connected billing*/
            }, isadmin));
            planReader.Setup(x => x.CanUpgrade(It.IsAny<int>(), It.IsAny<bool>())).Returns(false);
            webMsg.Setup(x => x.AddTempInfo(It.IsAny<Controller>(), It.IsAny<String>(), It.IsAny<bool>(), It.IsAny<bool>()));
            var c = InitController();

            //act
            var result = c.ChangePlan(proceed).Result;

            //assert                
            planReader.VerifyAll();
            userCaching.VerifyAll();
            webMsg.VerifyAll();

            Assert.NotNull(result);
            Assert.IsType<RedirectToActionResult>(result);
            var rResult = (RedirectToActionResult)result;
            Assert.Equal("myprofile", rResult.ControllerName);
            Assert.Equal(PROFILE_ACTIONS.Index.ToString(), rResult.ActionName);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void upgrade_plan_proceeds_should_redirect_to_valid_url_if_user_can_upgrade(bool isadmin)
        {
            //arrange
            InitAllMocks();
            userCaching.Setup(x => x.GetLoggedOnUser(false)).ReturnsAsync(new AppUser(new AspNetUser()
            {
                Id = "1",
                PlanId = 1,
                ShopifyChargeId = 1 /*connected billing*/
            }, isadmin));
            planReader.Setup(x => x.CanUpgrade(It.IsAny<int>(), It.IsAny<bool>())).Returns(true /*this is what matter*/);
            var c = InitController();

            //act
            var result = c.ChangePlan(true).Result;

            //assert                
            planReader.VerifyAll();
            userCaching.VerifyAll();
            Assert.NotNull(result);
            Assert.IsType<RedirectToActionResult>(result);
            var rResult = (RedirectToActionResult)result;
            Assert.Equal("shopify", rResult.ControllerName);
            Assert.Equal(SHOPIFY_ACTIONS.ChoosePlan.ToString(), rResult.ActionName);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void upgrade_plan_that_is_not_proceeding_should_return_a_valid_view_if_user_can_upgrade(bool isadmin)
        {
            //arrange
            InitAllMocks();
            userCaching.Setup(x => x.GetLoggedOnUser(false)).ReturnsAsync(new AppUser(new AspNetUser()
            {
                Id = "1",
                PlanId = 1,
                ShopifyChargeId = 1 /*connected billing*/
            }, isadmin));
            planReader.Setup(x => x.CanUpgrade(It.IsAny<int>(), It.IsAny<bool>())).Returns(true /*this is what matter*/);
            var c = InitController();

            //act
            var result = c.ChangePlan(false).Result;

            //assert                
            planReader.VerifyAll();
            userCaching.VerifyAll();
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        #region class variables
        SignInManager<AspNetUser> signInManager;
        Mock<IPlansReader> planReader;
        Mock<IUserCaching> userCaching;
        Mock<IShopifyEventsEmailer> emailer;
        Mock<IShopifyApi> shopifyApi;
        Mock<IConfiguration> config;
        IDbSettingsReader settings;
        Mock<IWebMessenger> webMsg;
        Mock<IDbRepository<AspNetUser>> repo;
        #endregion

        #region Helper Methods
        private void SetupPlanReaderById(bool returnsNull, int id)
        {
            if (returnsNull)
            {
                planReader.Setup(x => x[It.IsAny<int>()]);
            }
            else
            {
                planReader.Setup(x => x[It.IsAny<int>()]).Returns(new PlanAppModel()
                {
                    Id = id
                });
            }
        }
        private void SetupGetRecurringChargeAsync(string status, bool throwsError = false)
        {
            if (throwsError == false)
            {
                shopifyApi.Setup(x => x.GetRecurringChargeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), null)).ReturnsAsync(new ShopifyRecurringChargeObject()
                {
                    Name = "valid-plan-name",
                    Status = status,
                });
            }
            else
            {
                shopifyApi.Setup(x => x.GetRecurringChargeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), null)).ThrowsAsync(new Exception("this is a fake exception"));
            }
        }
        private void InitAllMocks(FakeSignInManager s = null)
        {
            IDentityDataPreparation i = new IDentityDataPreparation();

            if (s == null)
            {
                var context = new Mock<HttpContext>();
                var contextAccessor = new Mock<IHttpContextAccessor>();
                contextAccessor.Setup(x => x.HttpContext).Returns(context.Object);
                signInManager = new FakeSignInManager(contextAccessor.Object);
            }
            else signInManager = s;
            planReader = new Mock<IPlansReader>();
            userCaching = new Mock<IUserCaching>();
            emailer = new Mock<IShopifyEventsEmailer>();
            shopifyApi = new Mock<IShopifyApi>();
            config = new Mock<IConfiguration>();
            webMsg = new Mock<IWebMessenger>();
            repo = new Mock<IDbRepository<AspNetUser>>();

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
        private IMyProfileController InitController()
        {
            settings = GetSettings();
            var logger = new Mock<ILogger<MyProfileController>>().Object;
            MyProfileController c = new MyProfileController(
                            webMsg.Object,
                            shopifyApi.Object,
                            planReader.Object,
                            userCaching.Object,
                            config.Object,
                            settings,
                            logger
                         );
            return c;
        }
        #endregion
    }


    #region helper classes
    public class MyProfileController : ABaseMyProfileController
    {
        public MyProfileController(

            IWebMessenger webMsg,
            IShopifyApi shopifyApi,
            IPlansReader planReader,
            IUserCaching userCache,
            IConfiguration config,
            IDbSettingsReader settings,
            ILogger<MyProfileController> logger) : base(

            webMsg,
            shopifyApi,
            planReader,
            userCache,
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
