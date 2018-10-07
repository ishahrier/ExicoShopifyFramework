using Exico.Shopify.Data;
using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Exico.Shopify.Web.Core.Helpers;
using Exico.Shopify.Web.Core.Modules;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Test.Exico.Shopify.Web.Core
{
    [Collection("Sequential")]
    public class Test_Settings_Reader_Helper
    {
        private const int TOTAL_CORE_SETTINGS = 16;
        private const int TOTAL_APP_SETTINGS = 2;
        private const int TOTAL_SETTINGS = 18;
        private const string APP_SETTING_GROUP_NAME = "App";
        private ExicoShopifyDbContext context = null;
        private ILogger<IDbSettingsReader> logger;

        public Test_Settings_Reader_Helper()
        {

            var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
            var factory = serviceProvider.GetService<ILoggerFactory>();
            logger = factory.CreateLogger<IDbSettingsReader>();
            context = InitDbContext();
        }

        [Fact]
        public void Constructor_Should_Load_settings()
        {
            var r = GetDefaultSettingsReader();

            Assert.Equal(2, r.AllSettings.Count());
            Assert.Equal(TOTAL_CORE_SETTINGS, r.AllSettings[DbSettingsReaderExtensions.CORE_SETTINGS_GROUP_NAME].Count());
            Assert.Equal(TOTAL_APP_SETTINGS, r.AllSettings[APP_SETTING_GROUP_NAME].Count());
        }

        [Fact]
        public void Should_Get_A_List_Of_Settings_By_Group_Name()
        {

            var r = GetDefaultSettingsReader();
            var data = r.GetSettings(APP_SETTING_GROUP_NAME);
            Assert.Equal(TOTAL_APP_SETTINGS, data.Count());

            data = r.GetSettings(DbSettingsReaderExtensions.CORE_SETTINGS_GROUP_NAME);
            Assert.Equal(TOTAL_CORE_SETTINGS, data.Count());

        }

        [Fact]
        public void Should_Get_Setting_Object_By_Group_And_Name()
        {
            var r = GetDefaultSettingsReader();
            var sName = CORE_SYSTEM_SETTING_NAMES.PRIVILEGED_IPS.ToString();
            var obj = r.GetSetting(DbSettingsReaderExtensions.CORE_SETTINGS_GROUP_NAME, sName);
            Assert.Equal(sName, obj.SettingName);
        }

        [Fact]
        public void Should_Return_Null_On_Invalid_Setting_name()
        {
            var r = GetDefaultSettingsReader();
            var obj = r.GetSetting(DbSettingsReaderExtensions.CORE_SETTINGS_GROUP_NAME, "blahblah");
            Assert.Null(obj);

        }

        [Fact]
        public void Should_Return_Empty_String_On_Invalid_Setting_name_And_Or_Invalid_Group_Name()
        {
            var r = GetDefaultSettingsReader();
            var value = r.GetValue("blahblah", "blahblah");
            Assert.NotNull(value);
            Assert.Equal(string.Empty, value);
        }

        [Fact]
        public void Should_Return_Null_On_Invalid_Group_Name()
        {
            var r = GetDefaultSettingsReader();
            var sName = CORE_SYSTEM_SETTING_NAMES.PRIVILEGED_IPS.ToString();
            var obj = r.GetSetting("blahblah", sName);
            Assert.Null(obj);
        }

        [Fact]
        public void Should_Return_Default_When_Value_Is_Unavailable()
        {
            var r = GetDefaultSettingsReader();
            var obj = r.GetValue(APP_SETTING_GROUP_NAME, "AppSetting1");
            Assert.NotNull(obj);
            Assert.Equal("AppSetting1-DefaultValue", obj);

        }


        [Fact]
        public void Should_Return_Value_When_Value_Is_Available()
        {
            var r = GetDefaultSettingsReader();
            var obj = r.GetValue(DbSettingsReaderExtensions.CORE_SETTINGS_GROUP_NAME, CORE_SYSTEM_SETTING_NAMES.PRIVILEGED_IPS.ToString());
            Assert.NotNull(obj);
            Assert.Contains($"{CORE_SYSTEM_SETTING_NAMES.PRIVILEGED_IPS.ToString()}-Value", obj);

        }


        [Fact]
        public void Should_Not_Go_To_Db_If_Available_In_Cache()
        {

            var m = new MemoryCache(new MemoryCacheOptions() { });
            var service = new TestService<SystemSetting>(new TestRepository<SystemSetting>(new TestUnitOfWork(context)));
            DbSettingsReader r = new DbSettingsReader(service, m, logger);
            Assert.Equal(TOTAL_SETTINGS, r.AllSettings[DbSettingsReaderExtensions.CORE_SETTINGS_GROUP_NAME].Count() + r.AllSettings[APP_SETTING_GROUP_NAME].Count());

            var item = service.FindSingleWhere(x => x.SettingName == CORE_SYSTEM_SETTING_NAMES.PRIVILEGED_IPS.ToString());
            Assert.NotNull(item);
            service.Delete(item.Id);//deleting from live storage
            Assert.Equal(TOTAL_SETTINGS - 1, service.Count());//should 1 less in the storage

            var r2 = new DbSettingsReader(service, m, logger);//but this guys is not awre of storage, cause it is going to serve from cache
            Assert.Equal(TOTAL_SETTINGS, r2.AllSettings[DbSettingsReaderExtensions.CORE_SETTINGS_GROUP_NAME].Count() + r2.AllSettings[APP_SETTING_GROUP_NAME].Count());
            r2.ReloadFromDbAndUpdateCache( );//now the cache will be updated using storage data
            Assert.Equal(TOTAL_SETTINGS - 1, r2.AllSettings[DbSettingsReaderExtensions.CORE_SETTINGS_GROUP_NAME].Count() + r2.AllSettings[APP_SETTING_GROUP_NAME].Count());
        }

        //Adds 16 CORE settings plus 2 cutoms app setting  = 18 in total
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
            for (int i = 0; i < TOTAL_CORE_SETTINGS; i++)
            {
                var settingName = Enum.GetNames(typeof(CORE_SYSTEM_SETTING_NAMES)).ToList()[i];
                var s = new SystemSetting()
                {

                    DefaultValue = $"{settingName}-DefaultValue",
                    Description = $"{settingName}-Description",
                    DisplayName = $"{settingName}-DisplayName",
                    GroupName = DbSettingsReaderExtensions.CORE_SETTINGS_GROUP_NAME,
                    SettingName = settingName,
                    Value = $"{settingName}-Value"
                };

                testContext.SystemSettings.Add(s);
            }

            //add two custom settings which only have default value
            for (int i = 1; i <= TOTAL_APP_SETTINGS; i++)
            {
                var settingName = $"AppSetting{i}";
                var s = new SystemSetting()
                {

                    DefaultValue = $"{settingName}-DefaultValue",
                    Description = $"{settingName}-Description",
                    DisplayName = $"{settingName}-DisplayName",
                    GroupName = APP_SETTING_GROUP_NAME,
                    SettingName = settingName,
                    Value = ""
                };

                testContext.SystemSettings.Add(s);
            }

            testContext.SaveChanges();
            return testContext;
        }

        private DbSettingsReader GetDefaultSettingsReader()
        {
            var m = new MemoryCache(new MemoryCacheOptions() { });
            var service = new TestService<SystemSetting>(new TestRepository<SystemSetting>(new TestUnitOfWork(context)));
            DbSettingsReader r = new DbSettingsReader(service, m, logger);
            return r;
        }

    }
}
