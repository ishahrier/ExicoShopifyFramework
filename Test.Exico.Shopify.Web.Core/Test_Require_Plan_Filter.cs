using Exico.Shopify.Data;
using Exico.Shopify.Data.Domain.AppModels;
using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Domain.ShopifyApiModels;
using Exico.Shopify.Data.Framework;
using Exico.Shopify.Web.Core.Controllers.BaseControllers.Interfaces;
using Exico.Shopify.Web.Core.Filters;
using Exico.Shopify.Web.Core.Modules;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Test.Exico.Shopify.Web.Core
{
    [Collection("Sequential")]
    public class Test_Require_Plan_Filter
    {
        private DbSettingsReader realSettingsReader;
        private Mock<ILogger<RequiresPlan>> mockLogger;

        public Test_Require_Plan_Filter()
        {
            //common dependencies
            var realMemoryCahce = new MemoryCache(new MemoryCacheOptions() { });
            var service = new TestService<SystemSetting>(new TestRepository<SystemSetting>(new TestUnitOfWork(this._InitDbContext())));
            var mockLogger2 = new Mock<ILogger<DbSettingsReader>>();
            realSettingsReader = new DbSettingsReader(service, realMemoryCahce, mockLogger2.Object);
            mockLogger = new Mock<ILogger<RequiresPlan>>();
        }

        [Fact]
        public void user_doesnt_have_a_plan_throws_error()
        {
            var testUser = new AppUser(new AspNetUser()
            {
                Id = Guid.NewGuid().ToString(),
                BillingOn = DateTime.Now,
                MyShopifyDomain = "store1.myshopify.com",
                ShopifyChargeId = 1,
                Email = "store1@email.com",
                PlanId = null,
                UserName = "store1.myshopify.com",
                ShopifyAccessToken = "thetoken"
            }, false);

            //mock user caching
            var userCache = new Mock<IUserCaching>();
            userCache.Setup(x => x.GetLoggedOnUser(It.IsAny<bool>())).Returns(Task.FromResult(testUser));

            var planReader = new Mock<IPlansReader>();

            RequiresPlan obj = new RequiresPlan(userCache.Object, planReader.Object, realSettingsReader, mockLogger.Object, 1, "HasThis", "yes");
            var authCtx = _GetMockAuthrizationContext();
            var ex = Assert.Throws<Exception>(() => obj.OnAuthorization(authCtx));
            userCache.Verify(x => x.GetLoggedOnUser(It.IsAny<bool>()), Times.Exactly(1));
            Assert.Equal("Your account is not associated with any valid plan.Contact Support.", ex.Message);

        }

        [Fact]
        public void user_doesnt_have_required_plan_id_should_redirect()
        {
            //requires plan id 1 but user has 5
            var testUser = new AppUser(new AspNetUser()
            {
                Id = Guid.NewGuid().ToString(),
                BillingOn = DateTime.Now,
                MyShopifyDomain = "store1.myshopify.com",
                ShopifyChargeId = 1,
                Email = "store1@email.com",
                PlanId = 5,
                UserName = "store1.myshopify.com",
                ShopifyAccessToken = "thetoken"
            }, false);

            //mock user caching
            var userCache = new Mock<IUserCaching>();
            userCache.Setup(x => x.GetLoggedOnUser(It.IsAny<bool>())).Returns(Task.FromResult(testUser));

            var planReader = new Mock<IPlansReader>();
            planReader.Setup(x => x[It.IsAny<int>()]).Returns(new PlanAppModel() { IsDev = false });

            //requires plan id 1
            RequiresPlan obj = new RequiresPlan(userCache.Object, planReader.Object, realSettingsReader, mockLogger.Object, 1, "HasThis", "yes");
            var authCtx = _GetMockAuthrizationContext();
            obj.OnAuthorization(authCtx);
            var result = authCtx.Result;
            userCache.VerifyAll();
            Assert.IsType<RedirectToRouteResult>(result);
            var route = (RedirectToRouteResult)result;
            var controller = route.RouteValues["controller"];
            var action = route.RouteValues["action"];
            Assert.Equal(realSettingsReader.GetAppDashboardControllerName(), controller);
            Assert.Equal(DASHBOARD_ACTIONS.PlanDoesNotAllow.ToString(), action);

        }

        [Fact]
        public void user_has_required_plan_id_x_but_db_doesn_not_have_it_anymore_thorws_error()
        {
            //requires plan id 1 and user has 1 but it doesnt exist in the DB anymore
            var reqPlanId = 1;
            var testUser = new AppUser(new AspNetUser()
            {
                Id = Guid.NewGuid().ToString(),
                BillingOn = DateTime.Now,
                MyShopifyDomain = "store1.myshopify.com",
                ShopifyChargeId = 1,
                Email = "store1@email.com",
                PlanId = reqPlanId,
                UserName = "store1.myshopify.com",
                ShopifyAccessToken = "thetoken"
            }, false);

            //mock user caching
            var userCache = new Mock<IUserCaching>();
            userCache.Setup(x => x.GetLoggedOnUser(It.IsAny<bool>())).Returns(Task.FromResult(testUser));

            var planReader = new Mock<IPlansReader>();
            planReader.Setup(x => x[It.IsAny<int>()]).Returns((PlanAppModel)null);

            //requires plan id 1

            RequiresPlan obj = new RequiresPlan(userCache.Object, planReader.Object, realSettingsReader, mockLogger.Object, reqPlanId, "HasThis", "yes");
            var authCtx = _GetMockAuthrizationContext();
            var ex = Assert.Throws<Exception>(() => obj.OnAuthorization(authCtx));
            Assert.Equal($"Current user '{testUser.MyShopifyDomain}' plan id ='{testUser.PlanId.Value}' is not found in the loaded plans list.", ex.Message);
            userCache.Verify(x => x.GetLoggedOnUser(It.IsAny<bool>()), Times.Exactly(1));

        }

        [Fact]
        public void user_has_right_plan_and_option_should_pass()
        {
            var reqPlanId = 1;
            var optionName = "HasThis";
            var optionValue = "Yes";
            var testUser = new AppUser(new AspNetUser()
            {
                Id = Guid.NewGuid().ToString(),
                BillingOn = DateTime.Now,
                MyShopifyDomain = "store1.myshopify.com",
                ShopifyChargeId = 1,
                Email = "store1@email.com",
                PlanId = reqPlanId,
                UserName = "store1.myshopify.com",
                ShopifyAccessToken = "thetoken"
            }, false);

            //mock user caching
            var userCache = new Mock<IUserCaching>();
            userCache.Setup(x => x.GetLoggedOnUser(It.IsAny<bool>())).Returns(Task.FromResult(testUser));

            var plan = new PlanAppModel(new Plan()
            {
                Id = reqPlanId
            });
            var def = new PlanDefinitionAppModel(new PlanDefinition()
            {
                OptionName = optionName,
                OptionValue = optionValue,
                PlanId = plan.Id
            });
            var planReader = new Mock<IPlansReader>();
            planReader.Setup(x => x[It.IsAny<int>()]).Returns(plan);
            planReader.Setup(x => x[It.IsAny<int>(), It.IsAny<string>()]).Returns(def);

            //requires plan id 1

            RequiresPlan obj = new RequiresPlan(userCache.Object, planReader.Object, realSettingsReader, mockLogger.Object, reqPlanId, optionName, optionValue);
            var authCtx = _GetMockAuthrizationContext();
            obj.OnAuthorization(authCtx);
            userCache.Verify(x => x.GetLoggedOnUser(It.IsAny<bool>()), Times.Exactly(1));
            planReader.VerifyAll();
            Assert.Null(authCtx.Result);


        }

        [Fact]
        public void user_has_right_plan_wrong_option_value_should_redirect()
        {
            var reqPlanId = 1;
            var optionName = "HasThis";
            var optionValue = "Yes";
            var testUser = new AppUser(new AspNetUser()
            {
                Id = Guid.NewGuid().ToString(),
                BillingOn = DateTime.Now,
                MyShopifyDomain = "store1.myshopify.com",
                ShopifyChargeId = 1,
                Email = "store1@email.com",
                PlanId = reqPlanId,
                UserName = "store1.myshopify.com",
                ShopifyAccessToken = "thetoken"
            }, false);

            //mock user caching
            var userCache = new Mock<IUserCaching>();
            userCache.Setup(x => x.GetLoggedOnUser(It.IsAny<bool>())).Returns(Task.FromResult(testUser));

            var plan = new PlanAppModel(new Plan()
            {
                Id = reqPlanId
            });
            var def = new PlanDefinitionAppModel(new PlanDefinition()
            {
                OptionName = optionName,
                OptionValue = "wrong value",
                PlanId = plan.Id
            });
            var planReader = new Mock<IPlansReader>();
            planReader.Setup(x => x[It.IsAny<int>()]).Returns(plan);
            planReader.Setup(x => x[It.IsAny<int>(), It.IsAny<string>()]).Returns(def);

            //requires plan id 1

            RequiresPlan obj = new RequiresPlan(userCache.Object, planReader.Object, realSettingsReader, mockLogger.Object, reqPlanId, optionName, optionValue);
            var authCtx = _GetMockAuthrizationContext();
            obj.OnAuthorization(authCtx);
            userCache.Verify(x => x.GetLoggedOnUser(It.IsAny<bool>()), Times.Exactly(1));
            planReader.VerifyAll();
            Assert.IsType<RedirectToRouteResult>(authCtx.Result);
            var route = (RedirectToRouteResult)authCtx.Result;
            var controller = route.RouteValues["controller"];
            var action = route.RouteValues["action"];
            Assert.Equal(realSettingsReader.GetAppDashboardControllerName(), controller);
            Assert.Equal(DASHBOARD_ACTIONS.PlanDoesNotAllow.ToString(), action);


        }

        [Fact]
        public void user_has_right_plan_but_option_not_present_should_redirect()
        {
            var reqPlanId = 1;
            var optionName = "HasThis";
            var optionValue = "Yes";
            var testUser = new AppUser(new AspNetUser()
            {
                Id = Guid.NewGuid().ToString(),
                BillingOn = DateTime.Now,
                MyShopifyDomain = "store1.myshopify.com",
                ShopifyChargeId = 1,
                Email = "store1@email.com",
                PlanId = reqPlanId,
                UserName = "store1.myshopify.com",
                ShopifyAccessToken = "thetoken"
            }, false);

            //mock user caching
            var userCache = new Mock<IUserCaching>();
            userCache.Setup(x => x.GetLoggedOnUser(It.IsAny<bool>())).Returns(Task.FromResult(testUser));

            var plan = new PlanAppModel(new Plan()
            {
                Id = reqPlanId
            });
            PlanDefinitionAppModel def = null;

            var planReader = new Mock<IPlansReader>();
            planReader.Setup(x => x[It.IsAny<int>()]).Returns(plan);
            planReader.Setup(x => x[It.IsAny<int>(), It.IsAny<string>()]).Returns(def);

            //requires plan id 1

            RequiresPlan obj = new RequiresPlan(userCache.Object, planReader.Object, realSettingsReader, mockLogger.Object, reqPlanId, optionName, optionValue);
            var authCtx = _GetMockAuthrizationContext();
            obj.OnAuthorization(authCtx);
            userCache.Verify(x => x.GetLoggedOnUser(It.IsAny<bool>()), Times.Exactly(1));
            planReader.VerifyAll();
            Assert.IsType<RedirectToRouteResult>(authCtx.Result);
            var route = (RedirectToRouteResult)authCtx.Result;
            var controller = route.RouteValues["controller"];
            var action = route.RouteValues["action"];
            Assert.Equal(realSettingsReader.GetAppDashboardControllerName(), controller);
            Assert.Equal(DASHBOARD_ACTIONS.PlanDoesNotAllow.ToString(), action);


        }

        [Fact]
        public void user_has_dev_plan_should_ignore_any_requirement_and_pass()
        {
            var reqPlanId = 4;
            var optionName = "HasThis";
            var optionValue = "Yes";
            //dev plan id is different than required plan id
            //and it doesnt even has any options
            var devPlan = new PlanAppModel(new Plan()
            {
                Id = 1,
                IsDev = true
            });
            var testUser = new AppUser(new AspNetUser()
            {
                Id = Guid.NewGuid().ToString(),
                BillingOn = DateTime.Now,
                MyShopifyDomain = "store1.myshopify.com",
                ShopifyChargeId = 1,
                Email = "store1@email.com",
                PlanId = devPlan.Id,
                UserName = "store1.myshopify.com",
                ShopifyAccessToken = "thetoken"
            }, false);

            //mock user caching
            var userCache = new Mock<IUserCaching>();
            userCache.Setup(x => x.GetLoggedOnUser(It.IsAny<bool>())).Returns(Task.FromResult(testUser));


            var planReader = new Mock<IPlansReader>();
            planReader.Setup(x => x[It.IsAny<int>()]).Returns(devPlan);
            //  planReader.Setup(x => x[It.IsAny<int>(), It.IsAny<string>()]).Returns(new PlanDefinitionAppModel(new PlanDefinition()));

            //requires plan id 1

            RequiresPlan obj = new RequiresPlan(userCache.Object, planReader.Object, realSettingsReader, mockLogger.Object, reqPlanId, optionName, optionValue);
            var authCtx = _GetMockAuthrizationContext();
            obj.OnAuthorization(authCtx);
            userCache.Verify(x => x.GetLoggedOnUser(It.IsAny<bool>()), Times.Exactly(1));
            planReader.VerifyAll();
            Assert.Null(authCtx.Result);

        }

        private ExicoShopifyDbContext _InitDbContext()
        {
            var options = new DbContextOptionsBuilder<ExicoShopifyDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            ExicoShopifyDbContext testContext = new ExicoShopifyDbContext(options);
            //emptying
            testContext.Database.EnsureDeleted();
            ////recreating
            testContext.Database.EnsureCreated();
            //seeding


            var s = new SystemSetting()
            {
                DefaultValue = "APP_DASHBOARD_CONTOLLER",
                Description = "APP_DASHBOARD_CONTOLLER",
                DisplayName = "APP_DASHBOARD_CONTOLLER",
                GroupName = DbSettingsReaderExtensions.CORE_SETTINGS_GROUP_NAME,
                SettingName = CORE_SYSTEM_SETTING_NAMES.DASHBOARD_CONTOLLER.ToString(),
                Value = "DashBoard"
            };
            testContext.SystemSettings.Add(s);

            testContext.SaveChanges();
            return testContext;
        }

        private AuthorizationFilterContext _GetMockAuthrizationContext()
        {
            //moq authrization filter context
            var mockContext = new Mock<HttpContext>();
            var actionContext = new ActionContext(
                mockContext.Object,
                new Mock<RouteData>().Object,
                new Mock<ActionDescriptor>().Object
            );
            return new AuthorizationFilterContext(
                 actionContext,
                 new List<IFilterMetadata>()
             );
        }

        //TODO NULL user redirect to login test

    }
}
