using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Web.Core.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Test.Exico.Shopify.Web.Core
{
    public class IDentityDataPreparation
    {
        public ExicoIdentityDbContext DbContext { get; }
        public UserManager<AspNetUser> UMgr { get; }
        public SignInManager<AspNetUser> SMgr { get; }

        // Taken from https://github.com/aspnet/MusicStore/blob/dev/test/MusicStore.Test/ManageControllerTest.cs (and modified)
        // IHttpContextAccessor is required for SignInManager, and UserManager

        public IDentityDataPreparation()
        {
            var services = new ServiceCollection();

            services.AddDbContext<ExicoIdentityDbContext>(options => options.UseInMemoryDatabase("entity_database"));
            services.AddIdentity<AspNetUser, IdentityRole>().AddEntityFrameworkStores<ExicoIdentityDbContext>();

            var httpContext = new DefaultHttpContext();
            httpContext.Features.Set<IHttpAuthenticationFeature>(new HttpAuthenticationFeature());
            services.AddSingleton<IHttpContextAccessor>(h => new HttpContextAccessor { HttpContext = httpContext });
            var serviceProvider = services.BuildServiceProvider();
            httpContext.RequestServices = serviceProvider; ;
            DbContext = serviceProvider.GetRequiredService<ExicoIdentityDbContext>();
            DbContext.Database.EnsureCreated();
            DbContext.Database.EnsureDeleted();
            UMgr = serviceProvider.GetRequiredService<UserManager<AspNetUser>>();
            SMgr = serviceProvider.GetRequiredService<SignInManager<AspNetUser>>();
        }
  

    }

}
