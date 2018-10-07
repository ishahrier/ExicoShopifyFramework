
using Exico.Shopify.Data;
using Exico.Shopify.Data.Domain.AppModels;
using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Domain.ShopifyApiModels;
using Exico.Shopify.Data.Framework;
using Exico.Shopify.Web.Core.Controllers.BaseControllers;
using Exico.Shopify.Web.Core.Controllers.BaseControllers.Interfaces;
using Exico.Shopify.Web.Core.Helpers;
using Exico.Shopify.Web.Core.Modules;
using Exico.Shopify.Web.Core.Plugins.WebMsg;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Test.Exico.Shopify.Web.Core
{
    [Collection("Sequential")]
    public class Test_Shopify_Controller
    {
        #region class variables
        private string AppUrl = "https://www.myapp.com";
        private string ShopifyController = "shopify";
        private string ShopName = "my.shopify.com";
        private string ShopEmail = "i@shopify.com";
        private string DashboardControllerName = "dashboard";
        private readonly string ShopifyAuthCode = "authcode";//returned from shopify
        private readonly long ShopifyChargeId = 123456;
        private readonly long PrivilegedIp = 167772170;

        UserManager<AspNetUser> userManager;
        SignInManager<AspNetUser> signInManager;
        Mock<IPlansReader> planReader;
        Mock<IGenerateUserPassword> passwordGenerator;
        Mock<IUserCaching> userCaching;
        Mock<IShopifyEventsEmailer> emailer;
        Mock<IShopifyApi> shopifyApi;
        Mock<IConfiguration> config;
        IDbSettingsReader settings;
        Mock<IDbService<AspNetUser>> userService;
        Mock<IAppSettingsAccessor> appSettings;
        Mock<IWebMessenger> webMsg;
        Mock<IDbRepository<AspNetUser>> repo;

        #endregion

        #region Handshake Method Tests
        [Fact]
        public void handshake_thorws_error_for_unauthentic_request()
        {
            InitAllMocks();
            shopifyApi.Setup(x => x.IsAuthenticRequest(It.IsAny<HttpRequest>())).Returns(false);
            var controller = InitController();
            var result = Assert.ThrowsAnyAsync<Exception>(() => controller.Handshake("anyshop")).Result;
            Assert.Equal("Request is not authentic.", result.Message);
        }

        [Fact]
        public void handshake_should_redirect_to_authrization_url_for_unauthorized_shop()
        {
            InitAllMocks();
            shopifyApi.Setup(x => x.IsAuthenticRequest(It.IsAny<HttpRequest>())).Returns(true);//request is authentic
            shopifyApi.Setup(x => x.GetAuthorizationUrl(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), null, null)).Returns(new Uri("https://authrization_url"));
            userService.Setup(x => x.FindSingleWhere(It.IsAny<Expression<Func<AspNetUser, bool>>>()));//unauthorized shop
            var controller = InitController();
            var result = controller.Handshake(ShopName).Result;
            userService.VerifyAll();
            shopifyApi.VerifyAll();
            Assert.NotNull(result);
            Assert.IsType<RedirectResult>(result);
        }

        [Fact]
        public void handshake_should_redirect_to_landing_page_after_successfull_login_for_authorized_shop()
        {
            //arrange
            InitAllMocks();
            SetupDbComand();
            shopifyApi.Setup(x => x.IsAuthenticRequest(It.IsAny<HttpRequest>())).Returns(true);//request is authentic
            userService.Setup(x => x.FindSingleWhere(It.IsAny<Expression<Func<AspNetUser, bool>>>())).Returns(new AspNetUser()
            {
                UserName = ShopName,
                ShopifyAccessToken = "valid-token",
                Email = ShopEmail
            });
            passwordGenerator.Setup(x => x.GetPassword(It.IsAny<PasswordGeneratorInfo>())).Returns("valid_password");
            userCaching.Setup(x => x.SetLoggedOnUserInCache()).ReturnsAsync(It.IsAny<AppUser>());
            //call handshake method
            var controller = InitController();
            var result = controller.Handshake(ShopName).Result;

            //setup was called
            userService.VerifyAll();
            shopifyApi.VerifyAll();
            passwordGenerator.VerifyAll();
            userCaching.VerifyAll();
            //test
            Assert.NotNull(result);
            Assert.IsType<RedirectToActionResult>(result);
            var rResult = (RedirectToActionResult)result;
            Assert.Equal(DashboardControllerName, rResult.ControllerName);
            Assert.Equal(DASHBOARD_ACTIONS.Index.ToString(), rResult.ActionName);

        }

        [Fact]
        public void handshake_should_throw_error_on_unsuccessfull_login_for_authorized_shop()
        {
            //arrange
            InitAllMocks();
            SetupDbComand();
            ((FakeSignInManager)signInManager).PassworSignInSuccceess = false;
            shopifyApi.Setup(x => x.IsAuthenticRequest(It.IsAny<HttpRequest>())).Returns(true);//request is authentic
            userService.Setup(x => x.FindSingleWhere(It.IsAny<Expression<Func<AspNetUser, bool>>>())).Returns(new AspNetUser()
            {
                UserName = ShopName,
                ShopifyAccessToken = "valid-token",
                Email = ShopEmail
            });
            passwordGenerator.Setup(x => x.GetPassword(It.IsAny<PasswordGeneratorInfo>())).Returns("valid_password");

            //call handshake method
            var controller = InitController();
            var result = Assert.ThrowsAsync<Exception>(() => controller.Handshake(ShopName)).Result;
            //setup was called
            userService.VerifyAll();
            shopifyApi.VerifyAll();
            passwordGenerator.VerifyAll();
            Assert.StartsWith("Could not sign into your account", result.Message);
        }

        #endregion

        #region AuthResult method tests
        [Fact]
        public void auth_result_should_throw_error_on_unauthentic_request()
        {
            InitAllMocks();
            shopifyApi.Setup(x => x.IsAuthenticRequest(It.IsAny<HttpRequest>())).Returns(false);
            var c = InitController();
            var result = Assert.ThrowsAsync<Exception>(() => c.AuthResult(ShopName, ShopifyAuthCode)).Result;
            shopifyApi.VerifyAll();
            Assert.Equal("Request is not authentic.", result.Message);
        }

        [Fact]
        public void auth_result_should_throw_error_if_getting_access_code_throws_error_for_unauthorized_shop()
        {
            InitAllMocks();
            shopifyApi.Setup(x => x.IsAuthenticRequest(It.IsAny<HttpRequest>())).Returns(true);//authentic request
            userService.Setup(x => x.FindSingleWhere(It.IsAny<Expression<Func<AspNetUser, bool>>>()));//making UserDbServiceHelper.ShopIsAuthorized() return false
            shopifyApi.Setup(x => x.Authorize(It.IsAny<string>(), It.IsAny<String>())).Throws(new Exception("Api error."));//api call fails

            var c = InitController();

            var result = Assert.ThrowsAsync<Exception>(() => c.AuthResult(ShopName, ShopifyAuthCode)).Result;
            shopifyApi.VerifyAll();
            userService.VerifyAll();
            Assert.StartsWith("Shopify did not authorize.", result.Message);
        }

        [Fact]
        public void auth_result_should_throw_error_if_getting_shop_info_throws_error()
        {
            InitAllMocks();
            shopifyApi.Setup(x => x.IsAuthenticRequest(It.IsAny<HttpRequest>())).Returns(true);//authentic request
            userService.Setup(x => x.FindSingleWhere(It.IsAny<Expression<Func<AspNetUser, bool>>>()));//making UserDbServiceHelper.ShopIsAuthorized() return false
            shopifyApi.Setup(x => x.Authorize(It.IsAny<string>(), It.IsAny<String>())).Returns(Task.FromResult<string>("valid-acess-token"));//return valid access token
            shopifyApi.Setup(x => x.GetShopAsync(It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception("Api error."));//getting shop info fails
            var c = InitController();

            var result = Assert.ThrowsAsync<Exception>(() => c.AuthResult(ShopName, ShopifyAuthCode)).Result;
            shopifyApi.VerifyAll();
            userService.VerifyAll();
            Assert.StartsWith("Could not retrive shop info obj.", result.Message);
        }

        [Fact]
        public void auth_result_should_throw_error_on_user_creation_fails()
        {
            InitAllMocks();
            shopifyApi.Setup(x => x.IsAuthenticRequest(It.IsAny<HttpRequest>())).Returns(true);//authentic request
            userService.Setup(x => x.FindSingleWhere(It.IsAny<Expression<Func<AspNetUser, bool>>>()));//making UserDbServiceHelper.ShopIsAuthorized() return false
            shopifyApi.Setup(x => x.Authorize(It.IsAny<string>(), It.IsAny<String>())).Returns(Task.FromResult<string>("valid-acess-token"));//return valid access token
            shopifyApi.Setup(x => x.GetShopAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(new ShopifyShopObject()
            {
                MyShopifyDomain = ShopName,
                Email = ShopEmail
            }));//api returns valid shop info
            passwordGenerator.Setup(x => x.GetPassword(It.IsAny<PasswordGeneratorInfo>())).Returns("invalid-password");//return password
            ((FakeUserManager)userManager).UserCreationSuccess = false;//failing user creation

            var c = InitController();

            var result = Assert.ThrowsAsync<Exception>(() => c.AuthResult(ShopName, ShopifyAuthCode)).Result;
            shopifyApi.VerifyAll();
            userService.VerifyAll();
            passwordGenerator.VerifyAll();
            Assert.StartsWith("Could not create app user.", result.Message);
        }

        [Theory]
        [InlineData(true)]//doesnt matter gracefully ignore
        [InlineData(false)]//hook creation was successfull so go ahead
        public void auth_result_should_gracefully_ignores_that_unintall_hook_creation_failed(bool uninstallWebHookFailed)
        {
            //arrange
            InitAllMocks();
            SetupDbComand();
            userCaching.Setup(x => x.ClearLoggedOnUser());//DO NOT BOTHER ABOUT THIS SETUP
            emailer.Setup(x => x.UninstallHookCreationFailedAsync(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(true);
            userService.SetupSequence(x => x.FindSingleWhere(It.IsAny<Expression<Func<AspNetUser, bool>>>()))
                .Returns((AspNetUser)null) //so that we can skip if (UserDbServiceHelper.ShopIsAuthorized(UserDbService, shop)) line#85
                .Returns(new AspNetUser()
                {
                    UserName = ShopName,
                    ShopifyAccessToken = "valid-token",
                    Email = ShopEmail,
                    Id = Guid.NewGuid().ToString()
                });//making UserDbServiceHelper.GetUserByShopDomain() to return some user;

            shopifyApi.Setup(x => x.IsAuthenticRequest(It.IsAny<HttpRequest>())).Returns(true);//authentic request            
            shopifyApi.Setup(x => x.Authorize(It.IsAny<string>(), It.IsAny<String>())).Returns(Task.FromResult<string>("valid-acess-token"));//return valid access token
            shopifyApi.Setup(x => x.GetShopAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(new ShopifyShopObject()
            {
                MyShopifyDomain = ShopName,
                Email = ShopEmail
            }));//api returns valid shop info
            appSettings.Setup(x => x.BindObject<List<WebHookDefinition>>(It.IsAny<string>(), It.IsAny<IConfiguration>())).Returns(new List<WebHookDefinition>());
            if (uninstallWebHookFailed)//throws error but authreasult gracefully ignores
                shopifyApi.Setup(x => x.CreateWebhookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ShopifyWebhookObject>())).ThrowsAsync(new Exception("api error"));//uninstall web hook crteation failing
            else
                shopifyApi.Setup(x => x.CreateWebhookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ShopifyWebhookObject>()));//uninstall web hook crteation is successful


            passwordGenerator.Setup(x => x.GetPassword(It.IsAny<PasswordGeneratorInfo>())).Returns("invalid-password");//return password
            ((FakeUserManager)userManager).UserCreationSuccess = true;//user creation is always successful

            //act
            var c = InitController();

            //assert
            var result = c.AuthResult(ShopName, ShopifyAuthCode).Result;
            shopifyApi.VerifyAll();
            userService.VerifyAll();
            passwordGenerator.VerifyAll();
            userCaching.VerifyAll();//regardless of hook failer authresult is succeeding, and this method is being called everytime
            if (uninstallWebHookFailed) emailer.VerifyAll();//only when hook fails then  email is sent            
            Assert.IsType<RedirectToActionResult>(result);//doesnt matter uninstall hook failed or not if login is successfull then we are good

        }

        [Fact]
        public void auth_result_throws_error_when_login_is_unsuccessful()
        {
            //arrange
            InitAllMocks();
            SetupDbComand();
            userService.SetupSequence(x => x.FindSingleWhere(It.IsAny<Expression<Func<AspNetUser, bool>>>()))
                .Returns((AspNetUser)null) //so that we can skip if (UserDbServiceHelper.ShopIsAuthorized(UserDbService, shop)) line#85
                .Returns(new AspNetUser()
                {
                    UserName = ShopName,
                    ShopifyAccessToken = "valid-token",
                    Email = ShopEmail,
                    Id = Guid.NewGuid().ToString()
                });//making UserDbServiceHelper.GetUserByShopDomain() to return some user;
            shopifyApi.Setup(x => x.CreateWebhookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ShopifyWebhookObject>()));//uninstall web hook crteation is successful
            shopifyApi.Setup(x => x.IsAuthenticRequest(It.IsAny<HttpRequest>())).Returns(true);//authentic request            
            shopifyApi.Setup(x => x.Authorize(It.IsAny<string>(), It.IsAny<String>())).Returns(Task.FromResult<string>("valid-acess-token"));//return valid access token
            shopifyApi.Setup(x => x.GetShopAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(new ShopifyShopObject()
            {
                MyShopifyDomain = ShopName,
                Email = ShopEmail
            }));//api returns valid shop info

            passwordGenerator.Setup(x => x.GetPassword(It.IsAny<PasswordGeneratorInfo>())).Returns("invalid-password");//return password
            ((FakeUserManager)userManager).UserCreationSuccess = true;//user creation is always successful
            ((FakeSignInManager)signInManager).PassworSignInSuccceess = false;//log in not success
            appSettings.Setup(x => x.BindObject<List<WebHookDefinition>>(It.IsAny<string>(), It.IsAny<IConfiguration>())).Returns(new List<WebHookDefinition>());
            //act
            var c = InitController();

            //assert
            //var result = c.AuthResult(ShopName, ShopifyAuthCode).Result;
            var result = Assert.ThrowsAsync<Exception>(() => c.AuthResult(ShopName, ShopifyAuthCode)).Result;
            shopifyApi.VerifyAll();
            userService.VerifyAll();
            passwordGenerator.VerifyAll();
            Assert.StartsWith("Could not sign you in using app account.Sign in failed!", result.Message);

        }

        [Fact]
        public void Auth_result_should_redirect_to_choose_plan_when_authoried_user_creation_succesfull()
        {
            //arrange
            InitAllMocks();
            SetupDbComand();
            userCaching.Setup(x => x.ClearLoggedOnUser());//DO NOT BOTHER ABOUT THIS SETUP
            userService.SetupSequence(x => x.FindSingleWhere(It.IsAny<Expression<Func<AspNetUser, bool>>>()))
                .Returns((AspNetUser)null) //so that we can skip if (UserDbServiceHelper.ShopIsAuthorized(UserDbService, shop)) line#85
                .Returns(new AspNetUser()
                {
                    UserName = ShopName,
                    ShopifyAccessToken = "valid-token",
                    Email = ShopEmail,
                    Id = Guid.NewGuid().ToString()
                });//making UserDbServiceHelper.GetUserByShopDomain() to return some user;
            shopifyApi.Setup(x => x.CreateWebhookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ShopifyWebhookObject>()));//uninstall web hook crteation is successful
            shopifyApi.Setup(x => x.IsAuthenticRequest(It.IsAny<HttpRequest>())).Returns(true);//authentic request            
            shopifyApi.Setup(x => x.Authorize(It.IsAny<string>(), It.IsAny<String>())).Returns(Task.FromResult<string>("valid-acess-token"));//return valid access token
            shopifyApi.Setup(x => x.GetShopAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(new ShopifyShopObject()
            {
                MyShopifyDomain = ShopName,
                Email = ShopEmail
            }));//api returns valid shop info

            appSettings.Setup(x => x.BindObject<List<WebHookDefinition>>(It.IsAny<string>(), It.IsAny<IConfiguration>())).Returns(new List<WebHookDefinition>());

            passwordGenerator.Setup(x => x.GetPassword(It.IsAny<PasswordGeneratorInfo>())).Returns("valid-password");//return password
            ((FakeUserManager)userManager).UserCreationSuccess = true;//user creation is always successful
            ((FakeSignInManager)signInManager).PassworSignInSuccceess = true;//always successful

            //config.Setup(x => x.Bind(It.IsAny<string>(), It.IsAny<object>()));
            //act
            var c = InitController();

            //assert
            //var result = c.AuthResult(ShopName, ShopifyAuthCode).Result;
            var result = c.AuthResult(ShopName, ShopifyAuthCode).Result;
            shopifyApi.VerifyAll();
            userService.VerifyAll();
            userCaching.VerifyAll();//this also indicates success
            passwordGenerator.VerifyAll();

            Assert.NotNull(result);
            Assert.IsType<RedirectToActionResult>(result);
            var rResult = (RedirectToActionResult)result;
            Assert.Equal(ShopifyController, rResult.ControllerName);
            Assert.Equal(SHOPIFY_ACTIONS.ChoosePlan.ToString(), rResult.ActionName);
        }
        #endregion

        #region ChargeResult method tests
        [Fact]
        public void charge_result_should_should_have_authorize_attribute()
        {
            //arrange
            InitAllMocks();
            var controller = InitController();

            // Act
            var type = controller.GetType();
            var methodInfo = type.GetMethods().Where(x => x.Name == "ChargeResult").FirstOrDefault(); // .GetMethod("ChargeResult", new Type[] { });
            var attributes = methodInfo.GetCustomAttributes(typeof(AuthorizeAttribute), true);

            // Assert
            Assert.True(attributes.Any());
        }
        [Fact]
        public void charge_result_should_redirect_on_failing_to_get_recurring_charge()
        {
            //arrange
            InitAllMocks();
            var c = InitController();
            SetupContext(c);
            SetupDbComand();
            SetupFindSingleWhere(1);
            SetupGetRecurringChargeAsync("", true);

            SetupWebMsgTempDanger();

            //act
            var result = c.ChargeResult(ShopName, ShopifyChargeId).Result;

            //assert
            webMsg.VerifyAll();
            shopifyApi.VerifyAll();
            repo.VerifyAll();
            userService.VerifyAll();
            Assert.NotNull(result);
            Assert.IsType<RedirectToActionResult>(result);
            var rResult = result as RedirectToActionResult;
            Assert.Equal(ShopifyController, rResult.ControllerName);
            Assert.Equal(SHOPIFY_ACTIONS.ChoosePlan.ToString(), rResult.ActionName);

        }

        [Fact]
        public void charge_result_should_redirect_on_failing_to_read_plan_by_name()
        {
            //arrange
            InitAllMocks();
            var c = InitController();
            SetupContext(c);
            SetupDbComand();
            SetupFindSingleWhere(1);
            SetupGetRecurringChargeAsync("", false);
            SetupWebMsgTempDanger();
            SetupPlanReaderByName(true, 0);

            //assert
            var result = c.ChargeResult(ShopName, ShopifyChargeId).Result;
            shopifyApi.VerifyAll();
            repo.VerifyAll();
            userService.VerifyAll();
            planReader.VerifyAll();
            webMsg.VerifyAll();
            Assert.NotNull(result);
            Assert.IsType<RedirectToActionResult>(result);
            var rResult = result as RedirectToActionResult;
            Assert.Equal(ShopifyController, rResult.ControllerName);
            Assert.Equal(SHOPIFY_ACTIONS.ChoosePlan.ToString(), rResult.ActionName);

        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void charge_result_should_redirect_to_valid_url_if_charge_status_is_declined(bool newInstallation)
        {
            //arrange
            InitAllMocks();
            var c = InitController();
            SetupContext(c);
            SetupDbComand();
            SetupFindSingleWhere(newInstallation ? 0 : 1);
            SetupGetRecurringChargeAsync("declined");
            SetupWebMsgTempDanger();
            SetupPlanReaderByName(false, 1);

            //act   
            var result = c.ChargeResult(ShopName, ShopifyChargeId).Result;

            //assert
            shopifyApi.VerifyAll();
            repo.VerifyAll();
            userService.VerifyAll();
            planReader.VerifyAll();
            Assert.NotNull(result);

            if (newInstallation == false)/*its an upgrade*/
            {
                webMsg.VerifyAll();//showing msg to user on dashboard that upgrade didnt go well
                Assert.IsType<RedirectToActionResult>(result);
                var rResult = result as RedirectToActionResult;
                Assert.Equal(DashboardControllerName, rResult.ControllerName);
                Assert.Equal(DASHBOARD_ACTIONS.Index.ToString(), rResult.ActionName);
            }
            else
            {
                Assert.IsType<RedirectResult>(result);
                var rResult = result as RedirectResult;
                Assert.EndsWith("admin", rResult.Url);
            }

        }

        [Fact]
        public void charge_result_should_redirect_to_valid_url_if_charge_status_is_accepted_but_retriving_charge_info_fails()
        {
            //arrange
            InitAllMocks();
            var c = InitController();
            SetupContext(c);
            SetupDbComand();
            SetupFindSingleWhere(0);
            shopifyApi.SetupSequence(x => x.GetRecurringChargeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), null))
                .ReturnsAsync(new ShopifyRecurringChargeObject()
                {
                    Name = "valid-plan-name",
                    Status = "accepted"
                })
                .ThrowsAsync(new Exception("retriving_charge_info_fails 2nd time. this is a setup exception"));//so that 2nd call on line 203 fails, "retriving_charge_info_fails"

            SetupActivateRecurringChargeAsync(false);
            SetupWebMsgTempDanger();
            SetupPlanReaderByName(false, 1);

            //act
            var result = c.ChargeResult(ShopName, ShopifyChargeId).Result;
            //assert
            shopifyApi.VerifyAll();
            repo.VerifyAll();
            userService.VerifyAll();
            planReader.VerifyAll();
            webMsg.VerifyAll();

            Assert.NotNull(result);
            webMsg.VerifyAll();//showing msg to user on dashboard that upgrade didnt go well
            Assert.IsType<RedirectToActionResult>(result);
            var rResult = result as RedirectToActionResult;
            Assert.Equal(ShopifyController, rResult.ControllerName);
            Assert.Equal(SHOPIFY_ACTIONS.ChoosePlan.ToString(), rResult.ActionName);

        }

        [Fact]
        public void charge_result_should_redirect_to_valid_url_if_charge_status_is_accepted_but_retriving_charge_info_again_says_not_active()
        {
            //arrange
            InitAllMocks();
            var c = InitController();
            SetupContext(c);
            SetupDbComand();
            SetupFindSingleWhere(0);
            shopifyApi.SetupSequence(x => x.GetRecurringChargeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), null))
                .ReturnsAsync(new ShopifyRecurringChargeObject()
                {
                    Name = "valid-plan-name",
                    Status = "accepted"
                })
                .ReturnsAsync(new ShopifyRecurringChargeObject()
                {
                    Name = "valid-plan-name",
                    Status = "not-active"
                });
            shopifyApi.Setup(x => x.ActivateRecurringChargeAsync(It.IsAny<string>(), It.IsAny<String>(), It.IsAny<long>())).Returns(Task.FromResult(0));
            SetupWebMsgTempDanger();
            SetupPlanReaderByName(false, 1);

            //act
            var result = c.ChargeResult(ShopName, ShopifyChargeId).Result;

            //assert
            shopifyApi.VerifyAll();
            repo.VerifyAll();
            userService.VerifyAll();
            planReader.VerifyAll();
            webMsg.VerifyAll();

            Assert.NotNull(result);
            webMsg.VerifyAll();//showing msg to user on dashboard that upgrade didnt go well
            Assert.IsType<RedirectToActionResult>(result);
            var rResult = result as RedirectToActionResult;
            Assert.Equal(ShopifyController, rResult.ControllerName);
            Assert.Equal(SHOPIFY_ACTIONS.ChoosePlan.ToString(), rResult.ActionName);

        }

        [Fact]
        public void charge_result_should_redirect_to_valid_url_if_charge_status_is_accepted_and_set_active_but_payment_info_update_fails()
        {
            //arrange
            InitAllMocks();
            var c = InitController();
            SetupContext(c);
            SetupDbComand();
            SetupFindSingleWhere(0);
            userService.Setup(x => x.Update(It.IsAny<AspNetUser>(), It.IsAny<object>())).Returns((AspNetUser)null);//returns null so that the UserDbServiceHelper.SetUsersChargeInfo() fails
            shopifyApi.SetupSequence(x => x.GetRecurringChargeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), null))
                .ReturnsAsync(new ShopifyRecurringChargeObject()
                {
                    Name = "valid-plan-name",
                    Status = "accepted",
                    BillingOn = DateTime.Now
                })
                .ReturnsAsync(new ShopifyRecurringChargeObject()
                {
                    Name = "valid-plan-name",
                    Status = "active",
                    BillingOn = DateTime.Now
                });
            SetupActivateRecurringChargeAsync(false);
            SetupWebMsgTempDanger();
            SetupPlanReaderByName(false, 1);
            emailer.Setup(x => x.UserPaymentInfoCouldNotBeSavedAsync(It.IsAny<AppUser>(), It.IsAny<long>(), It.IsAny<string>())).ReturnsAsync(true);
            //act
            var result = c.ChargeResult(ShopName, ShopifyChargeId).Result;
            //assert
            shopifyApi.VerifyAll();
            repo.VerifyAll();
            userService.VerifyAll();
            planReader.VerifyAll();
            emailer.VerifyAll();
            webMsg.VerifyAll();//showing msg to user on dashboard that upgrade didnt go well
            Assert.NotNull(result);
            Assert.IsType<RedirectToActionResult>(result);
            var rResult = result as RedirectToActionResult;
            Assert.Equal(ShopifyController, rResult.ControllerName);
            Assert.Equal(SHOPIFY_ACTIONS.ChoosePlan.ToString(), rResult.ActionName);

        }

        [Fact]
        public void charge_result_should_redirect_to_valid_url_if_charge_status_is_active_and_it_was_an_upgrade()
        {
            //arrange
            InitAllMocks();
            var c = InitController();
            SetupContext(c);
            SetupDbComand();

            SetupFindSingleWhere(1);
            userService.Setup(x => x.Update(It.IsAny<AspNetUser>(), It.IsAny<object>())).Returns(new AspNetUser()
            {
                Id = "1"
            });//returns null so that the UserDbServiceHelper.SetUsersChargeInfo() fails
            shopifyApi.SetupSequence(x => x.GetRecurringChargeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), null))
                .ReturnsAsync(new ShopifyRecurringChargeObject()
                {
                    Name = "valid-plan-name",
                    Status = "accepted",
                    BillingOn = DateTime.Now
                })
                .ReturnsAsync(new ShopifyRecurringChargeObject()
                {
                    Name = "valid-plan-name",
                    Status = "active",
                    BillingOn = DateTime.Now
                });
            SetupActivateRecurringChargeAsync(false);
            webMsg.Setup(x => x.AddTempSuccess(It.IsAny<Controller>(), It.IsAny<object>(), It.IsAny<bool>(), It.IsAny<bool>()));
            SetupPlanReaderByName(false, 2);
            emailer.Setup(x => x.UserUpgradedPlanAsync(It.IsAny<AppUser>(), It.IsAny<int>())).ReturnsAsync(true);
            userCaching.Setup(x => x.GetLoggedOnUser(true)).ReturnsAsync(new AppUser(new AspNetUser()
            {
                Id = "1",
                PlanId = 1
            }));

            //act
            var result = c.ChargeResult(ShopName, ShopifyChargeId).Result;

            //assert
            shopifyApi.VerifyAll();
            repo.VerifyAll();
            userService.VerifyAll();
            planReader.VerifyAll();
            webMsg.VerifyAll();
            emailer.VerifyAll();//upgrade email will be sent
            userCaching.VerifyAll();//on successful installation upgrade or new, cache is always cleared
            Assert.NotNull(result);
            Assert.IsType<RedirectToActionResult>(result);
            var rResult = result as RedirectToActionResult;
            Assert.Equal(DashboardControllerName, rResult.ControllerName);
            Assert.Equal(DASHBOARD_ACTIONS.Index.ToString(), rResult.ActionName);

        }

        [Fact]
        public void charge_result_should_redirect_to_valid_url_if_charge_status_is_active_and_it_ia_new_installation()
        {
            //arrange
            InitAllMocks();
            var c = InitController();
            SetupContext(c);
            SetupDbComand();

            SetupFindSingleWhere(0);
            userService.Setup(x => x.Update(It.IsAny<AspNetUser>(), It.IsAny<object>())).Returns(new AspNetUser()
            {
                Id = "1"
            });//returns null so that the UserDbServiceHelper.SetUsersChargeInfo() fails
            shopifyApi.SetupSequence(x => x.GetRecurringChargeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), null))
                .ReturnsAsync(new ShopifyRecurringChargeObject()
                {
                    Name = "valid-plan-name",
                    Status = "accepted",
                    BillingOn = DateTime.Now
                })
                .ReturnsAsync(new ShopifyRecurringChargeObject()
                {
                    Name = "valid-plan-name",
                    Status = "active",
                    BillingOn = DateTime.Now
                });
            SetupActivateRecurringChargeAsync(false);

            //webMsg.Setup(x => x.AddTempSuccess(It.IsAny<Controller>(), It.IsAny<object>(), It.IsAny<bool>(), It.IsAny<bool>()));
            SetupPlanReaderByName(false, 1);
            emailer.Setup(x => x.UserInstalledAppAsync(It.IsAny<AppUser>())).ReturnsAsync(true);
            emailer.Setup(x => x.UserWelcomeEmailAsync(It.IsAny<string>())).ReturnsAsync(true);
            userCaching.Setup(x => x.GetLoggedOnUser(true)).ReturnsAsync(new AppUser( new AspNetUser()
            {
                Id = "1",
                PlanId = 1
            }));
            appSettings.Setup(x => x.BindObject<List<WebHookDefinition>>(It.IsAny<string>(), It.IsAny<IConfiguration>())).Returns(new List<WebHookDefinition>());
            //assert
            var result = c.ChargeResult(ShopName, ShopifyChargeId).Result;

            shopifyApi.VerifyAll();
            repo.VerifyAll();
            userService.VerifyAll();
            planReader.VerifyAll();
            emailer.VerifyAll();//upgrade email will be sent
            userCaching.VerifyAll();//on successful installation upgrade or new, cache is always cleared
            Assert.NotNull(result);
            Assert.IsType<RedirectToActionResult>(result);
            var rResult = result as RedirectToActionResult;
            Assert.Equal(DashboardControllerName, rResult.ControllerName);
            Assert.Equal(DASHBOARD_ACTIONS.Index.ToString(), rResult.ActionName);

        }

        private void SetupActivateRecurringChargeAsync(bool returnsError)
        {
            if (returnsError)
            {

            }
            else
            {
                shopifyApi.Setup(x => x.ActivateRecurringChargeAsync(It.IsAny<string>(), It.IsAny<String>(), It.IsAny<long>())).Returns(Task.FromResult(0));
            }
        }

        #endregion

        #region ChoosePlan tests
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void choose_plan_should_include_dev_plan_for_admin_users_only(bool admin)
        {
            //arrange
            InitAllMocks();
            var c = InitController();
            var cts = SetupContext(c);
            SetupFindSingleWhere(1, "1");
            SetupDbComand(admin);
            planReader.Setup(x => x.GetAvailableUpgrades(1, admin)).Returns(new List<PlanAppModel>());

            //act
            var result = c.ChoosePlan().Result;

            //assert
            planReader.VerifyAll();
            repo.VerifyAll();
            userService.VerifyAll();
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(0)]/*no previous plan*/
        [InlineData(1)]
        public void choose_plan_should_assign_valid_value_in_view(int prevPlan)
        {
            //arrange
            InitAllMocks();
            var c = InitController();
            var cts = SetupContext(c);
            planReader.Setup(x => x.GetAvailableUpgrades(It.IsAny<int>(), true)).Returns(new List<PlanAppModel>());
            SetupFindSingleWhere(prevPlan, "1");
            SetupDbComand(true);

            //act
            var result = c.ChoosePlan().Result;

            //assert
            planReader.VerifyAll();
            repo.VerifyAll();
            userService.VerifyAll();
            Assert.NotNull(result);

            var vResult = (ViewResult)result;
            var val = (bool)(vResult.ViewData["PrePlan"]);
            if (prevPlan > 0)
                Assert.True(val);
            else
                Assert.False(val);
        }

        #endregion

        #region SelectedPlan Tests
        [Fact]
        public void selected_plan_should_redirect_to_valid_url_for_downgrading_for_non_priviledged_user()
        {
            //arrange
            InitAllMocks();
            var c = InitController();
            var cts = SetupContext(c, 777772451)/*so that the ip is not privileged any more*/;
            SetupFindSingleWhere(2, "1");
            SetupDbComand(true);//doesnt matter if you are admin, as long as ip is not matched
            SetupWebMsgTempDanger();

            //act
            var result = c.SelectedPlan(1).Result;//non privileged ip user is trying to downgrade ; not allowed

            //assert
            userService.VerifyAll();
            repo.VerifyAll();
            cts.VerifyAll();
            webMsg.Verify();

            Assert.NotNull(result);
            Assert.IsType<RedirectToActionResult>(result);

            var rResult = (RedirectToActionResult)result;
            Assert.Equal(ShopifyController, rResult.ControllerName);
            Assert.Equal(SHOPIFY_ACTIONS.ChoosePlan.ToString(), rResult.ActionName);
        }

        [Fact]
        public void selected_plan_should_redirect_to_valid_url_if_plan_reader_cant_find_the_plan()
        {
            //arrange
            InitAllMocks();
            var c = InitController();
            var cts = SetupContext(c, 777772451)/*so that the ip is not privileged any more*/;
            SetupFindSingleWhere(1, "1");
            SetupDbComand(true);//doesnt matter if you are admin, as long as ip is not matched
            SetupWebMsgTempDanger();
            planReader.Setup(x => x[It.IsAny<int>()]);//plan reader fails here
            //act
            var result = c.SelectedPlan(2).Result;

            //assert
            userService.VerifyAll();
            repo.VerifyAll();
            cts.Verify(x => x.User);
            webMsg.Verify();
            planReader.VerifyAll();
            Assert.NotNull(result);
            Assert.IsType<RedirectToActionResult>(result);

            var rResult = (RedirectToActionResult)result;
            Assert.Equal(ShopifyController, rResult.ControllerName);
            Assert.Equal(SHOPIFY_ACTIONS.ChoosePlan.ToString(), rResult.ActionName);
        }

        [Fact]
        public void selected_plan_redirect_to_valid_url_if_recurring_charge_creation_fails()
        {
            //arrange
            InitAllMocks();
            var c = InitController();
            var cts = SetupContext(c, 777772451)/*so that the ip is not privileged any more*/;
            SetupFindSingleWhere(1, "1");
            SetupDbComand(true);//doesnt matter if you are admin, as long as ip is not matched
            SetupWebMsgTempDanger();
            planReader.Setup(x => x[It.IsAny<int>()]).Returns(new PlanAppModel()
            {
                Name = "name",
                Active = true,
                TrialDays = 4,
                IsTest = false,
                Price = 5.00m,
                Id = 2
            });//plan reader fails here
            shopifyApi.Setup(x => x.CreateRecurringChargeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ShopifyRecurringChargeObject>())).Throws(new Exception("fake error exception"));
            //act
            var result = c.SelectedPlan(2).Result;

            //assert
            userService.VerifyAll();
            repo.VerifyAll();
            cts.Verify(x => x.User);
            webMsg.Verify();
            planReader.VerifyAll();
            shopifyApi.VerifyAll();
            Assert.NotNull(result);
            Assert.IsType<RedirectToActionResult>(result);

            var rResult = (RedirectToActionResult)result;
            Assert.Equal(ShopifyController, rResult.ControllerName);
            Assert.Equal(SHOPIFY_ACTIONS.ChoosePlan.ToString(), rResult.ActionName);
        }

        [Fact]
        public void selected_plan_create_recurring_charge_and_redirect_to_confirmation_url()
        {
            //arrange
            InitAllMocks();
            var c = InitController();
            var cts = SetupContext(c, 777772451)/*so that the ip is not privileged any more*/;
            SetupFindSingleWhere(1, "1");
            SetupDbComand(true);//doesnt matter if you are admin, as long as ip is not matched
            SetupWebMsgTempDanger();
            planReader.Setup(x => x[It.IsAny<int>()]).Returns(new PlanAppModel()
            {
                Name = "name",
                Active = true,
                TrialDays = 4,
                IsTest = false,
                Price = 5.00m,
                Id = 2
            });//plan reader fails here
            shopifyApi.Setup(x => x.CreateRecurringChargeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ShopifyRecurringChargeObject>()))
                .Returns(Task.FromResult(new ShopifyRecurringChargeObject()
                {
                    ConfirmationUrl = "www.confirmation.com"

                }));
            //act
            var result = c.SelectedPlan(2).Result;

            //assert
            userService.VerifyAll();
            repo.VerifyAll();
            cts.Verify(x => x.User);
            webMsg.Verify();
            planReader.VerifyAll();
            shopifyApi.VerifyAll();
            Assert.NotNull(result);
            Assert.IsType<RedirectResult>(result);

            var rResult = (RedirectResult)result;
            Assert.Equal("www.confirmation.com", rResult.Url.ToString());
        }

        #endregion

        #region Helper Methods
        private void SetupPlanReaderByName(bool returnsNull, int id)
        {
            if (returnsNull)
            {
                planReader.Setup(x => x[It.IsAny<string>()]);
            }
            else
            {
                planReader.Setup(x => x[It.IsAny<string>()]).Returns(new PlanAppModel()
                {
                    Id = id
                });
            }
        }
        private void SetupGetRecurringChargeAsync(string status, bool throwsError = false)
        {
            if (throwsError == false)
            {
                shopifyApi.Setup(x => x.GetRecurringChargeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), null)).ReturnsAsync(new ShopifyRecurringChargeObject()
                {
                    Name = "valid-plan-name",
                    Status = status,
                });
            }
            else
            {
                shopifyApi.Setup(x => x.GetRecurringChargeAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), null)).ThrowsAsync(new Exception("this is a fake exception"));
            }
        }
        private void SetupWebMsgTempDanger()
        {
            webMsg.Setup(x => x.AddTempDanger(It.IsAny<Controller>(), It.IsAny<object>(), It.IsAny<bool>(), It.IsAny<bool>()));
        }
        private void SetupFindSingleWhere(int planId, string userId = "1")
        {
            userService.Setup(x => x.FindSingleWhere(It.IsAny<Expression<Func<AspNetUser, bool>>>())).Returns(new AspNetUser()
            {
                MyShopifyDomain = ShopName,
                ShopifyAccessToken = "valid-token",
                Email = ShopEmail,
                PlanId = planId,
                Id = userId,

            });
        }

        //context has a vlid remote ip of 10.0.0.10
        private Mock<HttpContext> SetupContext(Controller c, long? remoteIp = null)
        {
            var httpCtx = new Mock<HttpContext>();
            var connection = new Mock<ConnectionInfo>();
            httpCtx.Setup(x => x.Connection).Returns(connection.Object);
            httpCtx.Setup(x => x.User).Returns(new ClaimsPrincipal(new
                        ClaimsIdentity(new Claim[]
                        {
                            new Claim(ClaimTypes.Name, "a-user"),
                            new Claim(ClaimTypes.NameIdentifier,Guid.NewGuid().ToString())
                        }, "SomeAuthTYpe")));
            //167772161 = 10.0.0.10
            connection.Setup(x => x.RemoteIpAddress).Returns(new System.Net.IPAddress(remoteIp.HasValue ? remoteIp.Value : PrivilegedIp));
            //this setup is for UserInContextHelper.GetCurrentUserId
            c.ControllerContext = new ControllerContext()
            {
                HttpContext = httpCtx.Object
            };

            return httpCtx;


        }
        private void SetupDbComand(bool isAdmin = false)
        {
            //setup for UserDbServiceHelper.GetAppUserByIdAsync() because it uses execute scaler
            MyDbConnection con = new MyDbConnection();
            //setup command
            MyDbCommand command = new MyDbCommand(isAdmin ? 1 : 0/*not admin*/);
            command.Connection = con;
            //setup repository            
            repo.Setup(x => x.CreateDbCommand(It.IsAny<CommandType>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>())).Returns(command);
            //setup service
            //Mock<IDbService<AspNetUser>> service = new Mock<IDbService<AspNetUser>>();
            userService.Setup(x => x.GetRepo()).Returns(repo.Object);
        }
        private void InitAllMocks(FakeUserManager u = null, FakeSignInManager s = null)
        {
            IDentityDataPreparation i = new IDentityDataPreparation();
            userManager = u ?? new FakeUserManager();
            if (s == null)
            {
                var context = new Mock<HttpContext>();
                var contextAccessor = new Mock<IHttpContextAccessor>();
                contextAccessor.Setup(x => x.HttpContext).Returns(context.Object);
                signInManager = new FakeSignInManager(contextAccessor.Object);
            }
            else signInManager = s;
            planReader = new Mock<IPlansReader>();
            passwordGenerator = new Mock<IGenerateUserPassword>();
            userCaching = new Mock<IUserCaching>();
            emailer = new Mock<IShopifyEventsEmailer>();
            shopifyApi = new Mock<IShopifyApi>();
            config = new Mock<IConfiguration>();
            userService = new Mock<IDbService<AspNetUser>>();
            webMsg = new Mock<IWebMessenger>();
            repo = new Mock<IDbRepository<AspNetUser>>();
            appSettings = new Mock<IAppSettingsAccessor>();
        }
        private IDbSettingsReader GetSettings()
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
                Value = AppUrl,
                GroupName = "CORE",
                SettingName = CORE_SYSTEM_SETTING_NAMES.APP_BASE_URL.ToString()
            });
            testContext.SystemSettings.Add(new SystemSetting()
            {
                Value = ShopifyController,
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
            testContext.SystemSettings.Add(new SystemSetting()
            {
                Value = DashboardControllerName,
                GroupName = "CORE",
                SettingName = CORE_SYSTEM_SETTING_NAMES.DASHBOARD_CONTOLLER.ToString()
            });
            testContext.SystemSettings.Add(new SystemSetting()
            {
                Value = "10.0.0.10",
                GroupName = "CORE",
                SettingName = CORE_SYSTEM_SETTING_NAMES.PRIVILEGED_IPS.ToString()
            });
            testContext.SaveChanges();
            var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
            var factory = serviceProvider.GetService<Microsoft.Extensions.Logging.ILoggerFactory>();
            ILogger<IDbSettingsReader> logger = factory.CreateLogger<IDbSettingsReader>();
            var m = new MemoryCache(new MemoryCacheOptions() { });
            var service = new TestService<SystemSetting>(new TestRepository<SystemSetting>(new TestUnitOfWork(testContext)));
            DbSettingsReader r = new DbSettingsReader(service, m, logger);
            return r;

        }
        private ShopifyController InitController()
        {
            settings = GetSettings();
            var logger = new Mock<ILogger<ShopifyController>>().Object;
            ShopifyController c = new ShopifyController(
                            appSettings.Object,
                            userService.Object,
                            planReader.Object,
                            signInManager,
                            userManager,
                            passwordGenerator.Object,
                            userCaching.Object,
                            emailer.Object,
                            webMsg.Object,
                            shopifyApi.Object,
                            config.Object,
                            settings,
                            logger
                         );
            return c;
        }
        #endregion
    }


    #region helper classes

    public class ShopifyController : ABaseShopifyController
    {
        public ShopifyController(
            IAppSettingsAccessor appSettings,
            IDbService<AspNetUser> userService,
            IPlansReader planReader,
            SignInManager<AspNetUser> signInManager,
            UserManager<AspNetUser> userManager,
            IGenerateUserPassword passwordGenerator,
            IUserCaching userCache,
            IShopifyEventsEmailer emailer,
            IWebMessenger webMsg,
            IShopifyApi shopifyApi,
            IConfiguration config,
            IDbSettingsReader settings,
            ILogger<ShopifyController> logger) : base(
            appSettings,
            userService,
            planReader,
            signInManager,
            userManager,
            passwordGenerator,
            userCache,
            emailer,
            webMsg,
            shopifyApi,
            config,
            settings,
            logger)
        {

        }
        public override List<string> ListPermissions()
        {
            return new List<string>();
        }

        protected override string GetPageTitle()
        {
            throw new NotImplementedException();
        }
    }

    public class FakeUserManager : UserManager<AspNetUser>
    {
        public bool UserCreationSuccess { get; set; }
        public FakeUserManager()
            : base(new Mock<IUserStore<AspNetUser>>().Object,
                  new Mock<IOptions<IdentityOptions>>().Object,
                  new Mock<IPasswordHasher<AspNetUser>>().Object,
                  new IUserValidator<AspNetUser>[0],
                  new IPasswordValidator<AspNetUser>[0],
                  new Mock<ILookupNormalizer>().Object,
                  new Mock<IdentityErrorDescriber>().Object,
                  new Mock<IServiceProvider>().Object,
                  new Mock<ILogger<UserManager<AspNetUser>>>().Object
                )
        { }

        public override Task<IdentityResult> CreateAsync(AspNetUser user, string password)
        {
            if (UserCreationSuccess)
            {
                return Task.FromResult(IdentityResult.Success);
            }
            else
            {
                var err = new IdentityError()
                {
                    Description = "error",
                    Code = "100"
                };
                return Task.FromResult(IdentityResult.Failed(new IdentityError[] { err }));
            }
        }

    }
    public class FakeSignInManager : SignInManager<AspNetUser>
    {
        public bool PassworSignInSuccceess { get; set; } = true;

        public FakeSignInManager(IHttpContextAccessor contextAccessor)
            : base(new FakeUserManager(),
                  contextAccessor,
                  new Mock<IUserClaimsPrincipalFactory<AspNetUser>>().Object,
                  new Mock<IOptions<IdentityOptions>>().Object,
                  new Mock<ILogger<SignInManager<AspNetUser>>>().Object,
                  new Mock<IAuthenticationSchemeProvider>().Object)
        {


        }

        public override Task SignInAsync(AspNetUser user, bool isPersistent, string authenticationMethod = null)
        {
            return Task.FromResult(0);
        }

        public override Task<Microsoft.AspNetCore.Identity.SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            if (PassworSignInSuccceess)
                return Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Success);
            else
                return Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Failed);
        }

        public override Task SignOutAsync()
        {
            return Task.FromResult(0);
        }
    }

    #endregion  

}
