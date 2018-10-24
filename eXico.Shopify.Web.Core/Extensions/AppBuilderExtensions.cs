
using Exico.Shopify.Data;
using Exico.Shopify.Data.Domain.AppModels;
using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Exico.Shopify.Web.Core.Filters;
using Exico.Shopify.Web.Core.Helpers;
using Exico.Shopify.Web.Core.Modules;
using Exico.Shopify.Web.Core.Plugins.Email;
using Exico.Shopify.Web.Core.Plugins.WebMsg;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Exico.Shopify.Web.Core.Extensions
{
    /// <summary>
    /// Extension method IApplicationBuilder that are to be used in the startup.cs
    /// </summary>
    public static class AppBuilderExtensions
    {

        /// <summary>
        /// Adds all aspnet core services (AddMemoryCache(),AddSession() etc) required for the framework
        /// as well as registers all ExioSopifyFramework services.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="Configuration"></param>
        /// <param name="mvcBuilder"></param>
        public static void AddExicoShopifyRequiredServices(this IServiceCollection services, IConfiguration Configuration, IMvcBuilder mvcBuilder)
        {
            #region DB context
            services.AddDbContext<ExicoIdentityDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString(AppSettingsAccessor.DB_CON_STRING_NAME)));
            services.AddIdentity<AspNetUser, IdentityRole>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
            })
                .AddEntityFrameworkStores<ExicoIdentityDbContext>()
                .AddDefaultTokenProviders();
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = AppSettingsAccessor.IDENTITY_CORE_AUTH_COOKIE_NAME;
            });
            services.AddDbContext<ExicoShopifyDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString(AppSettingsAccessor.DB_CON_STRING_NAME)));
            #endregion

            #region Db services
            services.AddTransient<ExicoShopifyDbUnitOfWork, ExicoShopifyDbUnitOfWork>();

            services.AddTransient<ExicoShopifyDbRepository<Plan>, ExicoShopifyDbRepository<Plan>>();
            services.AddTransient<IDbService<Plan>, ExicoShopifyDbService<Plan>>();

            services.AddTransient<ExicoShopifyDbRepository<AspNetUser>, ExicoShopifyDbRepository<AspNetUser>>();
            services.AddTransient<IDbService<AspNetUser>, ExicoShopifyDbService<AspNetUser>>();

            services.AddTransient<ExicoShopifyDbRepository<SystemSetting>, ExicoShopifyDbRepository<SystemSetting>>();
            services.AddTransient<IDbService<SystemSetting>, ExicoShopifyDbService<SystemSetting>>();

            #endregion

            #region Filters
            services.AddScoped<AdminPasswordVerification>();
            services.AddScoped<IPAddressVerification>();
            services.AddScoped<RequiresPlan>();
            services.AddScoped<RequireSubscription>();
            #endregion

            #region Scoped services
            services.AddScoped<IPlansReader, PlansReader>();
            services.AddScoped<IDbSettingsReader, DbSettingsReader>();
            services.AddScoped<IUserCaching, UserCaching>();
            services.AddScoped<IWebMsgConfig, DefaultWebMsgConfig>();
            #endregion

            #region Scoped Services
            services.AddScoped<IGenerateUserPassword, DefaultPasswordGenerator>();
            services.AddScoped<IShopifyApi, ShopifyApi>();
            services.AddScoped<IWebMessenger, DefaultWebMessenger>();
            services.AddScoped<IAppSettingsAccessor, AppSettingsAccessor>();
            #endregion

            #region Transient Services
            services.AddTransient<IEmailer, SendGridEmailer>();
            services.AddTransient<IShopifyEventsEmailer, ShopifyEventsEmailer>();
            #endregion

            services.AddMemoryCache();
            var scopeFactory = services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var logger = scope.ServiceProvider.GetService<ILogger<Startup>>();                
                var isEmbeded = AppSettingsAccessor.IsUsingEmbededSdk(Configuration);
                logger.LogInformation($"Embeded app sdk usage is set to '{isEmbeded}'.");
                if (isEmbeded)
                {
                    logger.LogInformation("Setting up cookie provider for temp data.");                    
                    mvcBuilder.AddCookieTempDataProvider(x => x.Cookie.SameSite = SameSiteMode.None);
                    logger.LogInformation("Done setting up temp data cookie provider.");
                    logger.LogInformation("Setting up site cookie policy to 'SameSiteMode.None'.");
                    services.ConfigureApplicationCookie(options =>
                    {
                        options.Cookie.SameSite = SameSiteMode.None;
                    });

                    logger.LogInformation("Done setting up cookie policy.");
                    logger.LogInformation("Setting up anti forgery SuppressXFrameOptionsHeader = true.");
                    services.AddAntiforgery(x => x.SuppressXFrameOptionsHeader = true);
                    logger.LogInformation("Done setting up anti forgery.");
                }

            }

        }

        /// <summary>
        /// Uses necessary components (app.UseSession(),app.UseAuthentication() etc)
        /// as well as starts DB migrations, inserts initial system settings, plan data and admin user in the database tables.
        /// </summary>
        /// <remarks>Call this method after calling <code>app.UseMvc(...)</code>.</remarks>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public static void UseExicoShopifyFramework(this IApplicationBuilder app, IHostingEnvironment env)
        {

            //app.UseSession();//needed for webmsg
            app.UseAuthentication();
            var scopeFactory = app.ApplicationServices.GetService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var logger = scope.ServiceProvider.GetService<ILogger<Startup>>();
                var config = scope.ServiceProvider.GetService<IConfiguration>();
                try
                {
                    var identityContext = scope.ServiceProvider.GetService<ExicoIdentityDbContext>();
                    var exicoDbContext = scope.ServiceProvider.GetService<ExicoShopifyDbContext>();
                    if (env.IsDevelopment() && config[AppSettingsAccessor.DB_DROP_RECREATE_IN_DEV] == "1")
                    {
                        identityContext.Database.EnsureDeleted();
                        exicoDbContext.Database.EnsureDeleted();
                    }
                    identityContext.Database.EnsureCreated();
                    exicoDbContext.Database.Migrate();
                    logger.LogInformation("Starting seeding data.");
                    SeedData(exicoDbContext, identityContext, logger, config, scope);
                    logger.LogInformation("Finished seeding data.");

                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message);
                }
            }            
            app.UseMiddleware<LoggingScopeMiddleware>();

        }

        #region seed data 
        private static void SeedSettingsData(ExicoShopifyDbContext exicoDbContext, IConfiguration config, ILogger logger)
        {
            if (!exicoDbContext.SystemSettings.Any())
            {
                SettingsSeederAppModel settSeed = new SettingsSeederAppModel();
                config.Bind("SettingsSeed", settSeed);
                logger.LogInformation("Seeding settings data {@data}.", settSeed);
                exicoDbContext.SystemSettings.Add(new SystemSetting()
                {
                    DefaultValue = "Account",
                    Description = "Default account controller name without the controller part.",
                    DisplayName = "Account controller",
                    GroupName = "CORE",
                    SettingName = CORE_SYSTEM_SETTING_NAMES.ACCOUNT_CONTOLLER.ToString(),
                    Value = settSeed.ACCOUNT_CONTOLLER ?? "Account"
                });
                exicoDbContext.SystemSettings.Add(new SystemSetting()
                {
                    DefaultValue = "Dashboard",
                    Description = "Default dashboard controller name without the controller part.",
                    DisplayName = "Dashboard controller",
                    GroupName = "CORE",
                    SettingName = CORE_SYSTEM_SETTING_NAMES.DASHBOARD_CONTOLLER.ToString(),
                    Value = settSeed.DASHBOARD_CONTOLLER ?? "Dashboard"
                });
                exicoDbContext.SystemSettings.Add(new SystemSetting()
                {
                    DefaultValue = "MyProfile",
                    Description = "Default my profile controller name without the controller part.",
                    DisplayName = "My profile controller",
                    GroupName = "CORE",
                    SettingName = CORE_SYSTEM_SETTING_NAMES.MY_PROFILE_CONTOLLER.ToString(),
                    Value = settSeed.MY_PROFILE_CONTOLLER ?? "MyProfile",
                });
                exicoDbContext.SystemSettings.Add(new SystemSetting()
                {
                    DefaultValue = "Shopify",
                    Description = "Default my Shopify controller name without the controller part.",
                    DisplayName = "Shopify controller",
                    GroupName = "CORE",
                    SettingName = CORE_SYSTEM_SETTING_NAMES.SHOPIFY_CONTROLLER.ToString(),
                    Value = settSeed.SHOPIFY_CONTROLLER ?? "Shopify",
                });
                exicoDbContext.SystemSettings.Add(new SystemSetting()
                {
                    DefaultValue = "AppUninstall",
                    Description = "Default my app uninstall controller name without the controller part.",
                    DisplayName = "App uninstall controller",
                    GroupName = "CORE",
                    SettingName = CORE_SYSTEM_SETTING_NAMES.UNINSTALL_CONTROLLER.ToString(),
                    Value = settSeed.UNINSTALL_CONTROLLER ?? "Uninstall",
                });
                exicoDbContext.SystemSettings.Add(new SystemSetting()
                {
                    DefaultValue = "127.0.0.1",
                    Description = "List of privileged ip addresses.Use comma for multiples.",
                    DisplayName = "Privileged IPs",
                    GroupName = "CORE",
                    SettingName = CORE_SYSTEM_SETTING_NAMES.PRIVILEGED_IPS.ToString(),
                    Value = settSeed.PRIVILEGED_IPS ?? "127.0.0.1",
                });
                exicoDbContext.SystemSettings.Add(new SystemSetting()
                {
                    DefaultValue = "Welcome to the app!",
                    Description = "A welcome message template for new users.It can contain HTML.",
                    DisplayName = "Welcome Message",
                    GroupName = "CORE",
                    SettingName = CORE_SYSTEM_SETTING_NAMES.WELCOME_EMAIL_TEMPLATE.ToString(),
                    Value = settSeed.WELCOME_EMAIL_TEMPLATE ?? "Welcome to the app!",
                });
                exicoDbContext.SystemSettings.Add(new SystemSetting()
                {
                    DefaultValue = "support@myapp.com",
                    Description = "App support email address.",
                    DisplayName = "Support Email",
                    GroupName = "CORE",
                    SettingName = CORE_SYSTEM_SETTING_NAMES.APP_SUPPORT_EMAIL_ADDRESS.ToString(),
                    Value = settSeed.APP_SUPPORT_EMAIL_ADDRESS ?? ""
                });
                exicoDbContext.SystemSettings.Add(new SystemSetting()
                {
                    DefaultValue = "shopiyfy ap key",
                    Description = "API key of your shopify app.",
                    DisplayName = "API Key",
                    GroupName = "CORE",
                    SettingName = CORE_SYSTEM_SETTING_NAMES.API_KEY.ToString(),
                    Value = settSeed.API_KEY ?? ""
                });
                exicoDbContext.SystemSettings.Add(new SystemSetting()
                {
                    DefaultValue = "exicoShopifyFramework",
                    Description = "App name same as shopify app store.",
                    DisplayName = "App Name",
                    GroupName = "CORE",
                    SettingName = CORE_SYSTEM_SETTING_NAMES.APP_NAME.ToString(),
                    Value = settSeed.APP_NAME ?? ""
                });
                exicoDbContext.SystemSettings.Add(new SystemSetting()
                {
                    DefaultValue = "https://localhost:44300",
                    Description = "App URL without ending trail.",
                    DisplayName = "App URL",
                    GroupName = "CORE",
                    SettingName = CORE_SYSTEM_SETTING_NAMES.APP_BASE_URL.ToString(),
                    Value = settSeed.APP_BASE_URL ?? "https://localhost:44300"
                });
                exicoDbContext.SystemSettings.Add(new SystemSetting()
                {
                    DefaultValue = "secret key",
                    Description = "Secret key of your shopify app.",
                    DisplayName = "Secret Key",
                    GroupName = "CORE",
                    SettingName = CORE_SYSTEM_SETTING_NAMES.SECRET_KEY.ToString(),
                    Value = settSeed.SECRET_KEY ?? ""
                });
                exicoDbContext.SystemSettings.Add(new SystemSetting()
                {
                    DefaultValue = "send grid api key",
                    Description = "Send grid API key.",
                    DisplayName = "Sendgrid Key",
                    GroupName = "CORE",
                    SettingName = CORE_SYSTEM_SETTING_NAMES.SEND_GRID_API_KEY.ToString(),
                    Value = settSeed.SEND_GRID_API_KEY ?? ""
                });
                exicoDbContext.SystemSettings.Add(new SystemSetting()
                {
                    DefaultValue = "https://apps.shopify.com/",
                    Description = "Shopify app store URL",
                    DisplayName = "App Store",
                    GroupName = "CORE",
                    SettingName = CORE_SYSTEM_SETTING_NAMES.SHOPIFY_APP_STOER_URL.ToString(),
                    Value = settSeed.SHOPIFY_APP_STOER_URL ?? "https://apps.shopify.com/"
                });
                exicoDbContext.SystemSettings.Add(new SystemSetting()
                {
                    DefaultValue = "from@myapp.com",
                    Description = "Email address that will be used as from address.",
                    DisplayName = "From Email",
                    GroupName = "CORE",
                    SettingName = CORE_SYSTEM_SETTING_NAMES.SHOPIFY_EMAILS_FROM_ADDRESS.ToString(),
                    Value = settSeed.SHOPIFY_EMAILS_FROM_ADDRESS ?? ""
                });
                exicoDbContext.SystemSettings.Add(new SystemSetting()
                {
                    DefaultValue = "subscriber@myapp.com",
                    Description = "Emaile address that will receive any emails generated by the app/framework.Use comma for multiples.",
                    DisplayName = "Subscribers",
                    GroupName = "CORE",
                    SettingName = CORE_SYSTEM_SETTING_NAMES.SHOPIFY_EVENT_EMAIL_SUBSCRIBERS.ToString(),
                    Value = settSeed.SHOPIFY_EVENT_EMAIL_SUBSCRIBERS ?? ""
                });

                exicoDbContext.SystemSettings.Add(new SystemSetting()
                {
                    DefaultValue = "1.0.0",
                    Description = "Version of your app",
                    DisplayName = "Version",
                    GroupName = "CORE",
                    SettingName = CORE_SYSTEM_SETTING_NAMES.APP_VERSION.ToString(),
                    Value = settSeed.APP_VERSION ?? "1.0.0"
                });
                exicoDbContext.SystemSettings.Add(new SystemSetting()
                {
                    DefaultValue = "0.0.0",
                    Description = "Version of the Exico Shopify Framework that created this database",
                    DisplayName = "Seeder Framework Version",
                    GroupName = "CORE",
                    SettingName = CORE_SYSTEM_SETTING_NAMES.SEEDER_FRAMEWORK_VERSION.ToString(),
                    Value = AppSettingsAccessor.GetFrameWorkBuildNumber(true) 
                });
                exicoDbContext.SystemSettings.Add(new SystemSetting()
                {
                    DefaultValue = "",
                    Description = "Default auser password salt",
                    DisplayName = "User Password Salt",
                    GroupName = "CORE",
                    SettingName = CORE_SYSTEM_SETTING_NAMES.PASSWORD_SALT.ToString(),
                    Value = Guid.NewGuid().ToString()
                });
                var total = exicoDbContext.SaveChanges();
                logger.LogInformation($"Finished seeding settings data. Total {total} records.");
            }
            else
            {
                logger.LogInformation("Settings data alraedy exists.Skipping seeding for settings data.");
            }

        }
        private static void SeedPlanData(ExicoShopifyDbContext exicoDbContext, ILogger logger, IConfiguration config)
        {
            if (!exicoDbContext.Plans.Any())
            {
                List<Plan> plans = new List<Plan>();
                config.Bind("PlansSeed", plans);
                logger.LogInformation("Seeding plan data {@plans}.", plans);
                if (plans.Count() > 0)
                {
                    var totalPlanAdded = 0;
                    foreach (var p in plans)
                    {
                        exicoDbContext.Plans.Add(p);
                        int i = exicoDbContext.SaveChanges();
                        if (i == 0)
                        {
                            logger.LogError($"Could not seed Plan data for {p.Name}.");
                        }
                        else
                        {
                            totalPlanAdded++;
                            logger.LogInformation($"Successfully added plan {p.Name}.");
                        }
                    }
                    logger.LogInformation($"Finished seeding plan data. Total '{totalPlanAdded}' record(s).");
                }
                else
                {
                    logger.LogInformation("Plans seed data contains no plan data.");
                }

            }
            else
            {
                logger.LogInformation("Skipping plans data seeding as database alraedy has some.");
            }
        }
        private static void SeedAdminRoleAndUser(ExicoIdentityDbContext identityContext, ExicoShopifyDbContext exicoDbContext, ILogger logger, IConfiguration config, IServiceScope scope)
        {
            //TODO check and make it a little bit robus
            var roleStore = new RoleStore<IdentityRole>(identityContext);
            IdentityResult result = null;
            logger.LogInformation("creating admin role.");
            if (!identityContext.Roles.Any(r => r.Name == UserInContextHelper.ADMIN_ROLE))
            {
                result = roleStore.CreateAsync(new IdentityRole()
                {
                    Name = UserInContextHelper.ADMIN_ROLE,
                    NormalizedName = UserInContextHelper.ADMIN_ROLE
                }).Result;
                if (result.Succeeded == false)
                    logger.LogError($"Could not create admin role.{string.Join('.', result.Errors)}");
                else
                    logger.LogInformation("Successfully created admin role");
            }
            else
            {
                logger.LogInformation("Admin role already exists.So skipping role creation.");
                result = IdentityResult.Success;
            }
            if (result.Succeeded)
            {
                UserManager<AspNetUser> _userManager = scope.ServiceProvider.GetService<UserManager<AspNetUser>>();
                //read config values for admin user
                var user = new AspNetUser();
                config.Bind("AdminSeed", user);

                if (!identityContext.Users.Any(u => u.UserName == user.UserName))
                {
                    logger.LogInformation("Creating user.");
                    //prepare user
                    user.PlanId = user.PlanId ?? exicoDbContext.Plans.First().Id;
                    user.BillingOn = user.BillingOn ?? DateTime.Today;
                    user.ShopifyAccessToken = user.ShopifyAccessToken ?? "an-invalid-token";
                    user.ShopifyChargeId = user.ShopifyChargeId ?? 123456;

                    var passGenrerator = scope.ServiceProvider.GetService<IGenerateUserPassword>();
                    var pass = passGenrerator.GetPassword(new Data.Domain.AppModels.PasswordGeneratorInfo(user));
                    result = _userManager.CreateAsync(user, pass).Result;
                    if (result.Succeeded)
                        logger.LogInformation($"Finished creating user '{user.UserName}'");
                    else
                        logger.LogError($"Error creating user.{string.Join('.', result.Errors)}");

                }
                else
                {
                    result = IdentityResult.Success;
                    logger.LogInformation($"Skipping user creation. User '{user.UserName}' already exists.");
                }
                if (result.Succeeded)
                {
                    logger.LogInformation($"Assigning admin role to user '{user.UserName}'");
                    user = _userManager.FindByEmailAsync(user.Email).Result;
                    var userIsInRole = _userManager.IsInRoleAsync(user, UserInContextHelper.ADMIN_ROLE).Result;
                    if (!userIsInRole)
                    {
                        result = _userManager.AddToRolesAsync(user, new string[] { UserInContextHelper.ADMIN_ROLE }).Result;
                        if (result.Succeeded)
                            logger.LogInformation($"Finished assigning admin role to user '{user.UserName}'");
                        else
                            logger.LogError($"Error assigning user to admin role.{string.Join('.', result.Errors)}");
                    }
                    else
                    {
                        logger.LogWarning($"Skipping assigning user to admin role because user '{user.UserName}' already is an admin.");
                    }
                }
                else
                {
                    logger.LogWarning($"Skipping assigning user to admin role because user reation failed.");
                }
            }
            logger.LogInformation("Finished seeding admin role and user");
        }
        private static void SeedData(ExicoShopifyDbContext exicoDbContext, ExicoIdentityDbContext identityContext, ILogger logger, IConfiguration config, IServiceScope scope)
        {

            SeedSettingsData(exicoDbContext, config, logger);

            SeedPlanData(exicoDbContext, logger, config);

            SeedAdminRoleAndUser(identityContext, exicoDbContext, logger, config, scope);
        }
        #endregion

    }

    //we need this cause IdentityDbContext<AspNetUser> doesnt work in IOC
    public class ExicoIdentityDbContext : IdentityDbContext<AspNetUser> { public ExicoIdentityDbContext(DbContextOptions<ExicoIdentityDbContext> options) : base(options) { } }

}