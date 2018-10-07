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
    public class Test_Require_Subscription_Filter
    {
        private DbSettingsReader realSettingsReader;
        private Mock<IShopifyEventsEmailer> emailer;
        private Mock<ILogger<RequireSubscription>> mockLogger;

        public Test_Require_Subscription_Filter()
        {
            //common dependencies
            var realMemoryCahce = new MemoryCache(new MemoryCacheOptions() { });
            var service = new TestService<SystemSetting>(new TestRepository<SystemSetting>(new TestUnitOfWork(this._InitDbContext())));
            var mockLogger2 = new Mock<ILogger<DbSettingsReader>>();
            realSettingsReader = new DbSettingsReader(service, realMemoryCahce, mockLogger2.Object);

            emailer = new Mock<IShopifyEventsEmailer>();
            emailer.Setup(x => x.InActiveChargeIdDetectedAsync(It.IsAny<AppUser>(), It.IsAny<string>()));

            mockLogger = new Mock<ILogger<RequireSubscription>>();
        }

        [Fact]
        public void shopify_get_recurring_charge_aync_failue_should_throw_error()
        {
            var testUser = new AppUser(new AspNetUser()
            {
                Id = Guid.NewGuid().ToString(),
                BillingOn = DateTime.Now,
                MyShopifyDomain = "store1.myshopify.com",
                ShopifyChargeId = 1,
                Email = "store1@email.com",
                PlanId = 1,
                UserName = "store1.myshopify.com",
                ShopifyAccessToken = "thetoken"
            }, false);

            //moq ishopifyapi
            var shopifyApi = new Mock<IShopifyApi>();
            shopifyApi.Setup(x => x.GetRecurringChargeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Get recurring charge exception occurred"));

            //mock user caching
            var userCache = new Mock<IUserCaching>();
            userCache.Setup(x => x.GetLoggedOnUser(It.IsAny<bool>())).Returns(Task.FromResult(testUser));

            var userdb = new Mock<IDbService<AspNetUser>>();
            //userdb.Setup(x => x.FindSingleWhere(It.IsAny<Expression<Func<AspNetUser, bool>>>()))
            //    .Returns(() => new AspNetUser() { Id = testUser.Id });
            //userdb.Setup(x => x.Update(It.IsAny<AspNetUser>(), It.IsAny<object>())).Returns(() => It.IsAny<AspNetUser>());

            //mockLogger.Setup(x => x.LogError(It.IsAny<string>()));

            RequireSubscription obj = new RequireSubscription(shopifyApi.Object, realSettingsReader, emailer.Object, userCache.Object, userdb.Object, mockLogger.Object);

            var exception = Assert.ThrowsAny<Exception>(() => obj.OnAuthorization(_GetMockAuthrizationContext()));
            Assert.Contains("Get recurring charge exception occurred", exception.Message);
            //mockLogger.VerifyAll();
            userCache.VerifyAll();
        }

        [Fact]
        public void frozen_charge_status_should_throw_exception()
        {
            var testUser = new AppUser(new AspNetUser()
            {
                Id = Guid.NewGuid().ToString(),
                BillingOn = DateTime.Now,
                MyShopifyDomain = "store1.myshopify.com",
                ShopifyChargeId = 1,
                Email = "store1@email.com",
                PlanId = 1,
                UserName = "store1.myshopify.com",
                ShopifyAccessToken = "thetoken"
            }, false);

            //moq ishopifyapi
            var shopifyApi = new Mock<IShopifyApi>();
            shopifyApi.Setup(x => x.GetRecurringChargeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new ShopifyRecurringChargeObject() { Status = "frozen" }));

            //mock user caching
            var userCache = new Mock<IUserCaching>();
            userCache.Setup(x => x.GetLoggedOnUser(It.IsAny<bool>())).Returns(Task.FromResult(testUser));
            userCache.Setup(x => x.ClearLoggedOnUser());

            var userdb = new Mock<IDbService<AspNetUser>>();
            userdb.Setup(x => x.FindSingleWhere(It.IsAny<Expression<Func<AspNetUser, bool>>>()))
                .Returns(() => new AspNetUser() { Id = testUser.Id });
            userdb.Setup(x => x.Update(It.IsAny<AspNetUser>(), It.IsAny<object>())).Returns(() => It.IsAny<AspNetUser>());

            RequireSubscription obj = new RequireSubscription(shopifyApi.Object, realSettingsReader, emailer.Object, userCache.Object, userdb.Object, mockLogger.Object);

            Assert.ThrowsAny<Exception>(() => obj.OnAuthorization(_GetMockAuthrizationContext()));
        }

        [Theory]
        [InlineData("accepted")]
        [InlineData("active")]
        public void active_accepted_should_should_pass(string status)
        {
            var testUser = new AppUser(new AspNetUser()
            {
                Id = Guid.NewGuid().ToString(),
                BillingOn = DateTime.Now,
                MyShopifyDomain = "store1.myshopify.com",
                ShopifyChargeId = 1,
                Email = "store1@email.com",
                PlanId = 1,
                UserName = "store1.myshopify.com",
                ShopifyAccessToken = "thetoken"
            }, false);

            //moq ishopifyapi
            var shopifyApi = new Mock<IShopifyApi>();
            shopifyApi.Setup(x => x.GetRecurringChargeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new ShopifyRecurringChargeObject() { Status = status }));

            //mock user caching
            var userCache = new Mock<IUserCaching>();
            userCache.Setup(x => x.GetLoggedOnUser(It.IsAny<bool>())).Returns(Task.FromResult(testUser));
            userCache.Setup(x => x.ClearLoggedOnUser());

            var userdb = new Mock<IDbService<AspNetUser>>();
            userdb.Setup(x => x.FindSingleWhere(It.IsAny<Expression<Func<AspNetUser, bool>>>()))
                .Returns(() => new AspNetUser() { Id = testUser.Id });
            userdb.Setup(x => x.Update(It.IsAny<AspNetUser>(), It.IsAny<object>())).Returns(() => It.IsAny<AspNetUser>());

            RequireSubscription obj = new RequireSubscription(shopifyApi.Object, realSettingsReader, emailer.Object, userCache.Object, userdb.Object, mockLogger.Object);
            var authCtx = _GetMockAuthrizationContext();
            obj.OnAuthorization(authCtx);

            shopifyApi.VerifyAll();//shopify api was called to check charge status
            userdb.Verify(x => x.Update(It.IsAny<AspNetUser>(), It.IsAny<string>()), Times.Never());//nothing is saved in the db
            userCache.Verify(x => x.ClearLoggedOnUser(), Times.Never());//nothing is eared from cache
            Assert.Null(authCtx.Result);

        }

        [Theory]
        [InlineData("declined")]
        [InlineData("pending")]
        [InlineData("expired")]
        public void decline_expired_pending_status_should_redirect(string status)
        {
            var testUser = new AppUser(new AspNetUser()
            {
                Id = Guid.NewGuid().ToString(),
                BillingOn = DateTime.Now,
                MyShopifyDomain = "store1.myshopify.com",
                ShopifyChargeId = 1,
                Email = "store1@email.com",
                PlanId = 1,
                UserName = "store1.myshopify.com",
                ShopifyAccessToken = "thetoken"
            }, false);

            //moq ishopifyapi
            var shopifyApi = new Mock<IShopifyApi>();
            shopifyApi.Setup(x => x.GetRecurringChargeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new ShopifyRecurringChargeObject() { Status = status }));

            //mock user caching
            var userCache = new Mock<IUserCaching>();
            userCache.Setup(x => x.GetLoggedOnUser(It.IsAny<bool>())).Returns(Task.FromResult(testUser));
            userCache.Setup(x => x.ClearLoggedOnUser());
            //db service mock    
            var userdb = new Mock<IDbService<AspNetUser>>();
            userdb.Setup(x => x.FindSingleWhere(It.IsAny<Expression<Func<AspNetUser, bool>>>()))
                .Returns(() => new AspNetUser() { Id = testUser.Id });
            userdb.Setup(x => x.Update(It.IsAny<AspNetUser>(), It.IsAny<object>())).Returns(() => It.IsAny<AspNetUser>());

            RequireSubscription obj = new RequireSubscription(shopifyApi.Object, realSettingsReader, emailer.Object, userCache.Object, userdb.Object, mockLogger.Object);
            var authCtx = _GetMockAuthrizationContext();
            obj.OnAuthorization(authCtx);
            //check that we are clearing log
            userCache.Verify(x => x.ClearLoggedOnUser(), Times.Exactly(1));
            //check that we are finding that user and  unsetting (updating) billing information
            userdb.VerifyAll();
            //check shopify api was called as well to check recurring charge status
            shopifyApi.VerifyAll();
            //now check result
            var result = authCtx.Result;
            Assert.IsType<RedirectToRouteResult>(result);
            var routeResult = (RedirectToRouteResult)result;
            Assert.NotNull(routeResult.RouteValues["controller"]);
            Assert.NotNull(routeResult.RouteValues["action"]);
            Assert.Equal(routeResult.RouteValues["controller"], realSettingsReader.GetShopifyControllerName());
            Assert.Equal(routeResult.RouteValues["action"], SHOPIFY_ACTIONS.HandShake.ToString());

        }

        [Fact]
        public void disconnected_shop_should_redirect()
        {
            var testUser = new AppUser(new AspNetUser()
            {
                Id = Guid.NewGuid().ToString(),
                BillingOn = DateTime.Now,
                MyShopifyDomain = "store1.myshopify.com",
                ShopifyChargeId = 1,
                Email = "store1@email.com",
                PlanId = 1,
                UserName = "store1.myshopify.com",
                ShopifyAccessToken = null /*making disconnected*/
            }, false);

            //moq ishopifyapi
            var shopifyApi = new Mock<IShopifyApi>();
            shopifyApi.Setup(x => x.GetRecurringChargeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new ShopifyRecurringChargeObject() { Status = SHOPIFY_CHARGE_STATUS.accepted.ToString() }));

            //mock user caching
            var userCache = new Mock<IUserCaching>();
            userCache.Setup(x => x.GetLoggedOnUser(It.IsAny<bool>())).Returns(Task.FromResult(testUser));
            userCache.Setup(x => x.ClearLoggedOnUser());
            //db service mock    
            var userdb = new Mock<IDbService<AspNetUser>>();
            userdb.Setup(x => x.FindSingleWhere(It.IsAny<Expression<Func<AspNetUser, bool>>>()))
                .Returns(() => new AspNetUser() { Id = testUser.Id });
            userdb.Setup(x => x.Update(It.IsAny<AspNetUser>(), It.IsAny<object>())).Returns(() => It.IsAny<AspNetUser>());

            RequireSubscription obj = new RequireSubscription(shopifyApi.Object, realSettingsReader, emailer.Object, userCache.Object, userdb.Object, mockLogger.Object);
            var authCtx = _GetMockAuthrizationContext();
            obj.OnAuthorization(authCtx);
            var result = authCtx.Result;
            Assert.IsType<RedirectToRouteResult>(result);
            var routeResult = (RedirectToRouteResult)result;
            Assert.NotNull(routeResult.RouteValues["controller"]);
            Assert.NotNull(routeResult.RouteValues["action"]);
            Assert.Equal(routeResult.RouteValues["controller"], realSettingsReader.GetShopifyControllerName());
            Assert.Equal(routeResult.RouteValues["action"], SHOPIFY_ACTIONS.HandShake.ToString());

        }

        [Fact]
        public void disconnected_billing_should_redirect()
        {
            var testUser = new AppUser(new AspNetUser()
            {
                Id = Guid.NewGuid().ToString(),
                BillingOn = DateTime.Now,
                MyShopifyDomain = "store1.myshopify.com",
                ShopifyChargeId = null,/*making diconnected*/
                Email = "store1@email.com",
                PlanId = 1,
                UserName = "store1.myshopify.com",
                ShopifyAccessToken = "asdasdasd"
            }, false);

            //moq ishopifyapi
            var shopifyApi = new Mock<IShopifyApi>();
            shopifyApi.Setup(x => x.GetRecurringChargeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new ShopifyRecurringChargeObject() { Status = SHOPIFY_CHARGE_STATUS.accepted.ToString() }));

            //mock user caching
            var userCache = new Mock<IUserCaching>();
            userCache.Setup(x => x.GetLoggedOnUser(It.IsAny<bool>())).Returns(Task.FromResult(testUser));
            userCache.Setup(x => x.ClearLoggedOnUser());
            //db service mock    
            var userdb = new Mock<IDbService<AspNetUser>>();
            userdb.Setup(x => x.FindSingleWhere(It.IsAny<Expression<Func<AspNetUser, bool>>>()))
                .Returns(() => new AspNetUser() { Id = testUser.Id });
            userdb.Setup(x => x.Update(It.IsAny<AspNetUser>(), It.IsAny<object>())).Returns(() => It.IsAny<AspNetUser>());

            RequireSubscription obj = new RequireSubscription(shopifyApi.Object, realSettingsReader, emailer.Object, userCache.Object, userdb.Object, mockLogger.Object);
            var authCtx = _GetMockAuthrizationContext();
            obj.OnAuthorization(authCtx);
            var result = authCtx.Result;
            Assert.IsType<RedirectToRouteResult>(result);
            var routeResult = (RedirectToRouteResult)result;
            Assert.NotNull(routeResult.RouteValues["controller"]);
            Assert.NotNull(routeResult.RouteValues["action"]);
            Assert.Equal(routeResult.RouteValues["controller"], realSettingsReader.GetShopifyControllerName());
            Assert.Equal(routeResult.RouteValues["action"], SHOPIFY_ACTIONS.ChoosePlan.ToString());

        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void admin_by_passes_disconnected_or_connected_billing(bool billingDiconnected)
        {
            long? chargeid = 999;
            var testUser = new AppUser(new AspNetUser()
            {
                Id = Guid.NewGuid().ToString(),
                ShopifyChargeId = billingDiconnected == true ?  chargeid : null,
                BillingOn = DateTime.Now,
                MyShopifyDomain = "store1.myshopify.com",
                Email = "store1@email.com",
                PlanId = 1,
                UserName = "store1.myshopify.com",
                ShopifyAccessToken = "asdasdasd"
            }, true);


            //moq ishopifyapi
            var shopifyApi = new Mock<IShopifyApi>();
            shopifyApi.Setup(x => x.GetRecurringChargeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>()))
                .Returns(Task.FromResult(new ShopifyRecurringChargeObject() { Status = SHOPIFY_CHARGE_STATUS.accepted.ToString() }));

            //mock user caching
            var userCache = new Mock<IUserCaching>();
            userCache.Setup(x => x.GetLoggedOnUser(It.IsAny<bool>())).Returns(Task.FromResult(testUser));
            userCache.Setup(x => x.ClearLoggedOnUser());
            //db service mock    
            var userdb = new Mock<IDbService<AspNetUser>>();
            userdb.Setup(x => x.FindSingleWhere(It.IsAny<Expression<Func<AspNetUser, bool>>>()))
                .Returns(() => new AspNetUser() { Id = testUser.Id });
            userdb.Setup(x => x.Update(It.IsAny<AspNetUser>(), It.IsAny<object>())).Returns(() => It.IsAny<AspNetUser>());

            RequireSubscription obj = new RequireSubscription(shopifyApi.Object, realSettingsReader, emailer.Object, userCache.Object, userdb.Object, mockLogger.Object);
            var authCtx = _GetMockAuthrizationContext();
            obj.OnAuthorization(authCtx);
            var result = authCtx.Result;
            //shopify api is NEVER called to check admin's chage status
            shopifyApi.Verify(x => x.GetRecurringChargeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>()), Times.Never());
            userdb.Verify(x => x.Update(It.IsAny<AspNetUser>(), It.IsAny<string>()), Times.Never());//nothing is saved in the db
            userCache.Verify(x => x.ClearLoggedOnUser(), Times.Never());//nothing is cleared from cache
            Assert.Null(result);//means no redirect happened

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
                DefaultValue = "Shopify",
                Description = "Shopify Controller name",
                DisplayName = "Shopify Controller name",
                GroupName = DbSettingsReaderExtensions.CORE_SETTINGS_GROUP_NAME,
                SettingName = CORE_SYSTEM_SETTING_NAMES.SHOPIFY_CONTROLLER.ToString(),
                Value = "Shopify"
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

        //TODO : null user login redirection test
    }
}
