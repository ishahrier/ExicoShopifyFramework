using Castle.Core.Logging;
using Exico.Shopify.Data.Domain.AppModels;
using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Web.Core.Modules;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using Xunit;
using Microsoft.Extensions.Logging;
using Exico.Shopify.Data;
using Microsoft.EntityFrameworkCore;

namespace Test.Exico.Shopify.Web.Core
{
    [Collection("Sequential")]
    public class Test_Default_Password_Generator
    {

        private readonly string ValidPassword = "8808C05D5DBCB9895EE19E991DACFE9B";
        private AspNetUser GetUser() => new AspNetUser()
        {

            MyShopifyDomain = "ishahrier.myshopify.com",
            Email = "ishahrier@exico.ca"
        };

        [Fact]
        public void Should_generate_valid_passwords()
        {
            var user = GetUser();
            DefaultPasswordGenerator d = new DefaultPasswordGenerator( GetDefaultSettingsReader());
            var pass = d.GetPassword(new PasswordGeneratorInfo(user));
            var pass1 = d.GetPassword(new PasswordGeneratorInfo(user.MyShopifyDomain, user.Email));

            Assert.NotNull(pass);
            Assert.NotNull(pass1);
            Assert.Equal(pass, pass1);
            //Assert.Equal(ValidPassword, pass);
        }

        [Fact]
        public void Should_throw_exception_on_invalid_domain_name()
        {

            DefaultPasswordGenerator d = new DefaultPasswordGenerator(GetDefaultSettingsReader());
            var user = GetUser();
            user.MyShopifyDomain = null;
            
            Assert.Throws<Exception>(() => d.GetPassword(new PasswordGeneratorInfo(user))).Message.Contains("My shopify domain name is not valid");
        }

        private ExicoShopifyDbContext SetUpDbContext()
        {
            var options = new DbContextOptionsBuilder<ExicoShopifyDbContext>()
                .UseInMemoryDatabase("password_generator_db")
                .Options;

            ExicoShopifyDbContext testContext = new ExicoShopifyDbContext(options);
            //emptying
            testContext.Database.EnsureDeleted();
            ////recreating
            testContext.Database.EnsureCreated();
            //seeding            
            var s = new SystemSetting()
            {

                DefaultValue = "",
                Description = "Password salt",
                DisplayName = "Password salt",
                GroupName = DbSettingsReaderExtensions.CORE_SETTINGS_GROUP_NAME,
                SettingName = CORE_SYSTEM_SETTING_NAMES.PASSWORD_SALT.ToString(),
                Value = "1234567"
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
            var factory = serviceProvider.GetService<Microsoft.Extensions.Logging.ILoggerFactory>();
            var logger = factory.CreateLogger<IDbSettingsReader>();

            DbSettingsReader r = new DbSettingsReader(db, cache, logger);

            return r;
        }
    }
}
