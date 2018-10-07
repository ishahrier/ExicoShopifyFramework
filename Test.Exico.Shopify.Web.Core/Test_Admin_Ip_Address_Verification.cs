using Castle.Core.Logging;
using Exico.Shopify.Data;
using Exico.Shopify.Data.Domain.DBModels;
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
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Test.Exico.Shopify.Web.Core
{
    [Collection("Sequential")]
    public class Test_Admin_Ip_Address_Verification
    {
        private DbSettingsReader realSettingsReader;
        //allowed IPS
        private long IP_10_0_0_1 = 167772161;
        private long IP_10_0_0_2 = 167772162;
        private long IP_10_0_0_3 = 167772163;        
        //Not allowed IPs
        private long IP_10_0_0_4 = 167772164;
        private long IP_10_0_0_10 = 167772170; 
        

        public Test_Admin_Ip_Address_Verification()
        {
            var realMemoryCahce = new MemoryCache(new MemoryCacheOptions() { });
            var service = new TestService<SystemSetting>(new TestRepository<SystemSetting>(new TestUnitOfWork(this.InitDbContext())));
            var mockLogger2 = new Mock<ILogger<DbSettingsReader>>();
            realSettingsReader = new DbSettingsReader(service, realMemoryCahce, mockLogger2.Object);
        }

        [Fact]
        public void disallowed_remote_ip_will_give_unauthrized_result()
        {
            var mockContext = new Mock<HttpContext>();
            //for remote , local ip != remote ip
            mockContext.Setup(x => x.Connection.LocalIpAddress).Returns(() => new IPAddress(IP_10_0_0_4));
            mockContext.Setup(x => x.Connection.RemoteIpAddress).Returns(() => new IPAddress(IP_10_0_0_10));            

            var actionContext = new ActionContext(
                mockContext.Object,
                new Mock<RouteData>().Object,
                new Mock<ActionDescriptor>().Object
            );

            var actionExecutingContext = new AuthorizationFilterContext(
                actionContext,
                new List<IFilterMetadata>()//,
                //new Dictionary<string, object>(),
                //new Mock<Controller>().Object
            );            
            
            var mockLogger = new Mock<ILogger<IPAddressVerification>>();
            IPAddressVerification obj = new IPAddressVerification(realSettingsReader, mockLogger.Object);
            obj.OnAuthorization(actionExecutingContext);
            Assert.IsType<UnauthorizedResult>(actionExecutingContext.Result);
        }

        [Fact]
        public void allowed_remote_ip_will_give_null_result()
        {
            var mockContext = new Mock<HttpContext>();
            //for remote , local ip != remote ip
            mockContext.Setup(x => x.Connection.LocalIpAddress).Returns(() => new IPAddress(IP_10_0_0_1));
            mockContext.Setup(x => x.Connection.RemoteIpAddress).Returns(() => new IPAddress(IP_10_0_0_2));

            var actionContext = new ActionContext(
                mockContext.Object,
                new Mock<RouteData>().Object,
                new Mock<ActionDescriptor>().Object
            );

            var actionExecutingContext = new AuthorizationFilterContext(
                actionContext,
                new List<IFilterMetadata>()//,
                                           //new Dictionary<string, object>(),
                                           //new Mock<Controller>().Object
            );
            var mockLogger = new Mock<ILogger<IPAddressVerification>>();
            IPAddressVerification obj = new IPAddressVerification(realSettingsReader, mockLogger.Object);
            obj.OnAuthorization(actionExecutingContext);
            Assert.Null(actionExecutingContext.Result);
        }


        [Fact]
        public void local_ip_will_give_null_result()
        {
            var mockContext = new Mock<HttpContext>();

            //local request = local ip==remote ip
            mockContext.Setup(x => x.Connection.LocalIpAddress).Returns(() => new IPAddress(IP_10_0_0_10));
            mockContext.Setup(x => x.Connection.RemoteIpAddress).Returns(() => new IPAddress(IP_10_0_0_10));

            var actionContext = new ActionContext(
                mockContext.Object,
                new Mock<RouteData>().Object,
                new Mock<ActionDescriptor>().Object
            );

            var actionExecutingContext = new AuthorizationFilterContext(
                actionContext,
                new List<IFilterMetadata>()
            );
            var mockLogger = new Mock<ILogger<IPAddressVerification>>();
            IPAddressVerification obj = new IPAddressVerification(realSettingsReader, mockLogger.Object);
            obj.OnAuthorization(actionExecutingContext);
            Assert.Null(actionExecutingContext.Result);
        }



        private ExicoShopifyDbContext InitDbContext()
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

            //list of allowed IPS
            List<IPAddress> ips = new List<IPAddress>();
            ips.Add(new IPAddress(IP_10_0_0_1));
            ips.Add(new IPAddress(IP_10_0_0_2));
            ips.Add(new IPAddress(IP_10_0_0_3));
            var s = new SystemSetting()
            {

                DefaultValue = "127.0.0.1",
                Description = "Allowed Ips",
                DisplayName = "Allowed Ips",
                GroupName = DbSettingsReaderExtensions.CORE_SETTINGS_GROUP_NAME,
                SettingName = CORE_SYSTEM_SETTING_NAMES.PRIVILEGED_IPS.ToString(),
                Value = string.Join(',', ips.Select(x => x.ToString()).ToList().ToArray())
            };

            testContext.SystemSettings.Add(s);
            testContext.SaveChanges();
            return testContext;
        }

    }


}
