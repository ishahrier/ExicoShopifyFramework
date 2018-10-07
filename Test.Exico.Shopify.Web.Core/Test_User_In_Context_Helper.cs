using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Web.Core.Extensions;
using Exico.Shopify.Web.Core.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Test.Exico.Shopify.Web.Core
{
    [Collection("Sequential")]
    public class Test_User_In_Context_Helper
    {
        private ServiceProvider serviceProvider;
        private string UserName = "test_user";
        private string UserId = Guid.NewGuid().ToString();

        public Test_User_In_Context_Helper()
        {

        }


        private void InitServiceProvider(Boolean userIsLoggedOn)
        {
            var services = new ServiceCollection();
            var httpContext = new HttpContextAccessor()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = userIsLoggedOn ?  new ClaimsPrincipal(new
                        ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, UserName),
                            new Claim(ClaimTypes.NameIdentifier,UserId)
                        }, "SomeAuthTYpe")) : null
                },
            };
            services.AddSingleton<IHttpContextAccessor>(httpContext);
            serviceProvider = services.BuildServiceProvider();
            httpContext.HttpContext.RequestServices = serviceProvider;
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Get_Current_User_Id_Return_Null_If_Not_Authenticated(bool userIsLoggedOn)
        {
            InitServiceProvider(userIsLoggedOn);
            var contextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            
            if (userIsLoggedOn)
            {
                Assert.Equal(UserId, UserInContextHelper.GetCurrentUserId(contextAccessor.HttpContext));
            }
            else
            {
                Assert.Null(UserInContextHelper.GetCurrentUserId(contextAccessor.HttpContext));
            }
        }
    }
}
