using Exico.Shopify.Data;
using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Web.Core.Controllers.BaseControllers.Interfaces;
using Exico.Shopify.Web.Core.Helpers;
using Exico.Shopify.Web.Core.Modules;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Test.Exico.Shopify.Web.Core
{
    [Collection("Sequential")]
    public class Test_Shopify_Url_helper
    {
        private IDbSettingsReader settings;
        private string APP_BASE_URL = "http://localhost:5000";

        public Test_Shopify_Url_helper()
        {
            Mock<ILogger<IDbSettingsReader>> logger = new Mock<ILogger<IDbSettingsReader>>();
            var m = new MemoryCache(new MemoryCacheOptions() { });
            var service = new TestService<SystemSetting>(new TestRepository<SystemSetting>(new TestUnitOfWork(this.InitDbContext())));
            settings = new DbSettingsReader(service, m, logger.Object);

        }

        [Fact]
        public void Get_Auth_Result_Handler_Url_Should_Returl_Valid_Url()
        {
            var data = ShopifyUrlHelper.GetAuthResultHandlerUrl(settings);
            Assert.NotNull(data);
            Assert.Equal($"{APP_BASE_URL}/shopify/{SHOPIFY_ACTIONS.AuthResult}", data);
        }
        [Fact]
        public void Get_Charge_Result_Handler_Url_Should_Returl_Valid_Url()
        {
            var data = ShopifyUrlHelper.GetChargeResultHandlerUrl(settings);
            Assert.NotNull(data);
            Assert.Equal($"{APP_BASE_URL}/shopify/{SHOPIFY_ACTIONS.ChargeResult}", data);
        }

        [Fact]
        public void Get_App_Uninstall_Web_Hook_Url_Should_Returl_Valid_Url()
        {
            var data = ShopifyUrlHelper.GetAppUninstallWebHookUrl(settings, "123456");
            Assert.NotNull(data);
            Assert.Equal($"{APP_BASE_URL}/appuninstaller/{UNINSTALL_ACTIONS.AppUninstalled}?userId=123456", data);
        }

        [Fact]
        public void Get_admin_url_should_return_valid_admin_url()
        {
            var shop = "test.myshopify.com";
            var ret = ShopifyUrlHelper.GetAdminUrl(shop);
            Assert.Equal($"https://{shop}/admin", ret);
        }

        [Fact]
        public void Get_selected_plan_handler_url_should_return_a_valid_url()
        {
            var ret = ShopifyUrlHelper.GetSelectedPlanHandlerUrl(settings, 2);            
            Assert.Equal($"{APP_BASE_URL}/shopify/{SHOPIFY_ACTIONS.SelectedPlan}?planId={2}", ret);
        }

        [Fact]
        public void Get_plan_chosing_url_should_return_a_valid_url()
        {
            var ret = ShopifyUrlHelper.GetPlanChoosingUrl(settings);            
            Assert.Equal($"{APP_BASE_URL}/shopify/{SHOPIFY_ACTIONS.ChoosePlan}", ret);
        }

        [Fact]
        public void Get_Liquid_File_Link_Should_Return_Valid_Link()
        {
            var ret = ShopifyUrlHelper.GetLiquidFileLink(THEME_FOLDER_NAMES.layout, "store.myshopify.com", "product",".myext");
            Assert.Equal($"https://store.myshopify.com/admin/themes/current/?key={THEME_FOLDER_NAMES.layout}/product.myext",ret);
        }

        [Fact]
        public void Get_Rate_My_App_Url_Should_Return_Valid_Url()
        {
            var ret = ShopifyUrlHelper.GetRateMyAppUrl(settings, "myapp");
            Assert.Equal("https://apps.shopify.com/myapp", ret);
        }

        [Fact]
        public void make_store_friendly_app_name_should_return_valid_name()
        {
            var unfriendlyName = "my app version 2.0";
            var ret = ShopifyUrlHelper.MakeStoreFriendlyAppName(unfriendlyName, null);
            Assert.Equal("my-app-version-2.0", ret);
        }
        [Fact]
        public void make_store_friendly_app_name_should_dashify_supplied_char_array()
        {
            var unfriendlyName = "my app version 2.0";
            var ret = ShopifyUrlHelper.MakeStoreFriendlyAppName(unfriendlyName, new char[] {'.' });
            Assert.Equal("my-app-version-2-0", ret);
        }

        private ExicoShopifyDbContext InitDbContext()
        {
            var options = new DbContextOptionsBuilder<ExicoShopifyDbContext>()
                .UseInMemoryDatabase("settings_db")
                .Options;

            ExicoShopifyDbContext testContext = new ExicoShopifyDbContext(options);
            //emptying
            testContext.Database.EnsureDeleted();
            ////recreating
            testContext.Database.EnsureCreated();
            //seeding
            testContext.SystemSettings.Add(new SystemSetting()
            {
                Value = APP_BASE_URL,
                GroupName = "CORE",
                SettingName = CORE_SYSTEM_SETTING_NAMES.APP_BASE_URL.ToString()
            });
            testContext.SystemSettings.Add(new SystemSetting()
            {
                Value = "shopify",
                GroupName = "CORE",
                SettingName = CORE_SYSTEM_SETTING_NAMES.SHOPIFY_CONTROLLER.ToString()
            });

            testContext.SystemSettings.Add(new SystemSetting()
            {
                Value = "appuninstaller",
                GroupName = "CORE",
                SettingName = CORE_SYSTEM_SETTING_NAMES.UNINSTALL_CONTROLLER.ToString()
            });
            //testContext.SystemSettings.Add(new SystemSetting()
            //{
            //    Value = "uninstall",
            //    GroupName = "CORE",
            //    SettingName = CORE_SYSTEM_SETTING_NAMES.UNINSTALL_WEBHOOK_ACTION_NAME.ToString()
            //});
            testContext.SystemSettings.Add(new SystemSetting()
            {
                Value = "https://apps.shopify.com",
                GroupName = "CORE",
                SettingName = CORE_SYSTEM_SETTING_NAMES.SHOPIFY_APP_STOER_URL.ToString()
            });
            testContext.SaveChanges();
            return testContext;
        }



    }

}