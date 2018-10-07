using Exico.Shopify.Data;
using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Exico.Shopify.Web.Core.Helpers;
using Exico.Shopify.Web.Core.Modules;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace Test.Exico.Shopify.Web.Core
{
    [Collection("Sequential")]
    public class Test_Ip_Address_Helper
    {

        public string ADMIN_IPS { get; private set; } = "10.0.0.19,255.255.255.0";

        [Theory]
        [InlineData("10.0.0.19",true)]
        [InlineData("10.1.0.20", false)]
        public void Should_Be_True_If_Given_Ip_Is_Admin_Otherwise_False(string testThisIp,bool admin)
        {
            var ip = IPAddressHelper.IsInPrivilegedIpList(testThisIp, GetDefaultSettingsReader());
            if(admin==true)Assert.True(ip);
            if (admin==false) Assert.False(ip);
        }

        private ExicoShopifyDbContext SetUpDbContext()
        {
            var options = new DbContextOptionsBuilder<ExicoShopifyDbContext>()
                .UseInMemoryDatabase("ip_address_helper")
                .Options;

            ExicoShopifyDbContext testContext = new ExicoShopifyDbContext(options);
            //emptying
            testContext.Database.EnsureDeleted();
            ////recreating
            testContext.Database.EnsureCreated();
            //seeding            
            var s = new SystemSetting()
            {

                DefaultValue = "127.0.0.1",
                Description = "Admin IP addresses",
                DisplayName = "Admin Ips",
                    GroupName = DbSettingsReaderExtensions.CORE_SETTINGS_GROUP_NAME,
                SettingName = CORE_SYSTEM_SETTING_NAMES.PRIVILEGED_IPS.ToString(),
                Value = ADMIN_IPS
            };

            testContext.SystemSettings.Add(s);
            testContext.SaveChanges();
            return testContext;
        }

        private DbSettingsReader GetDefaultSettingsReader()
        {
            var db = new TestService<SystemSetting>(new TestRepository<SystemSetting>(new TestUnitOfWork(SetUpDbContext())));
            var cache = new MemoryCache(new MemoryCacheOptions() { });
            var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
            var factory = serviceProvider.GetService<ILoggerFactory>();
            var logger = factory.CreateLogger<IDbSettingsReader>();

            DbSettingsReader r = new DbSettingsReader(db, cache, logger);

            return r;
        }
    }
}
