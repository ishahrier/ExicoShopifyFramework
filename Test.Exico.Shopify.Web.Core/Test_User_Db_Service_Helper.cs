using Exico.Shopify.Data;
using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Exico.Shopify.Web.Core.Extensions;
using Exico.Shopify.Web.Core.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Test.Exico.Shopify.Web.Core
{


    [Collection("Sequential")]
    public class Test_User_Db_Service_Helper
    {
        private AspNetUser UserOne;
        private AspNetUser UserTwo;
        private IDbService<AspNetUser> service;
        public Test_User_Db_Service_Helper()
        {
            //valid user but not authrized
            UserOne = new AspNetUser()
            {
                Email = "test@gmail.com",
                MyShopifyDomain = "test.myshopify.com",
                UserName = "test.myshopify.com",
                Id = Guid.NewGuid().ToString()
            };
            //is an authorized user
            UserTwo = new AspNetUser()
            {
                Email = "test2@gmail.com",
                MyShopifyDomain = "test2.myshopify.com",
                UserName = "test2.myshopify.com",
                Id = Guid.NewGuid().ToString(),
                ShopifyAccessToken = "validtoken",
                BillingOn = DateTime.Now,
                PlanId = 1,
                ShopifyChargeId = 1

            };
            this.service = InitService();
        }

        [Fact]
        public void Get_User_By_Shop_Domain_Sould_Return_Valid_User_For_Valid_Shop()
        {
            //setup connection
            MyDbConnection con = new MyDbConnection();
            //setup command
            MyDbCommand command = new MyDbCommand(1/*admin*/);
            command.Connection = con;
            //setup repository
            Mock<IDbRepository<AspNetUser>> repo = new Mock<IDbRepository<AspNetUser>>();
            repo.Setup(x => x.CreateDbCommand(It.IsAny<CommandType>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>())).Returns(command);
            //setup service
            Mock<IDbService<AspNetUser>> service = new Mock<IDbService<AspNetUser>>();
            service.Setup(x => x.FindSingleWhere(It.IsAny<Expression<Func<AspNetUser, bool>>>())).Returns(UserOne);
            service.Setup(x => x.GetRepo()).Returns(repo.Object);
            var data =  UserDbServiceHelper.GetUserByShopDomain(service.Object, UserOne.MyShopifyDomain).Result;
            Assert.NotNull(data);
            Assert.Equal(UserOne.Id, data.Id);
        }
        [Fact]
        public void Get_User_By_Shop_Domain_Should_Return_NUll_For_Invalid_Shop()
        {
            var data = UserDbServiceHelper.GetUserByShopDomain(service, "invalid shop name").Result;
            Assert.Null(data);
        }

        [Fact]
        public void Shop_Is_Authorized_Should_Return_False()
        {
            var ret = UserDbServiceHelper.ShopIsAuthorized(service, UserOne.MyShopifyDomain);
            Assert.False(ret);
        }
        [Fact]
        public void Shop_Is_Authorized_Should_Return_True()
        {
            var ret = UserDbServiceHelper.ShopIsAuthorized(service, UserTwo.MyShopifyDomain);
            Assert.True(ret);
        }

        [Fact]
        public void Un_Set_User_Charge_Info_Should_null_Charge_values_in_db_record()
        {
            //This is Assert valid user with valid billing  info
            var user = new AspNetUser()
            {
                Email = "test4@gmail.com",
                MyShopifyDomain = "test4.myshopify.com",
                UserName = "test4.myshopify.com",
                Id = Guid.NewGuid().ToString(),
                ShopifyAccessToken = "validtoken",
                BillingOn = DateTime.Now,
                PlanId = 1,
                ShopifyChargeId = 1

            };
            service.Add(user);

            var ret = UserDbServiceHelper.UnSetUserChargeInfo(service, user.Id);
            Assert.True(ret);
            var data = service.FindSingleWhere(x => x.Id == user.Id);
            Assert.Null(data.BillingOn);
            Assert.Null(data.ShopifyChargeId);
            Assert.Null(data.PlanId);

        }

        [Fact]
        public void Set_Users_Charge_Info_Should_Set_Billing_Related_Info_in_Db()
        {
            //This is Assert valid user with NO  billing  info
            var user = new AspNetUser()
            {
                Email = "test3@gmail.com",
                MyShopifyDomain = "test3.myshopify.com",
                UserName = "test3.myshopify.com",
                Id = Guid.NewGuid().ToString(),
                ShopifyAccessToken = "validtoken",
                BillingOn = null,
                PlanId = null,
                ShopifyChargeId = null

            };
            service.Add(user);

            var ret = UserDbServiceHelper.SetUsersChargeInfo(service, user.Id, 2, 2, DateTime.Now);
            Assert.True(ret);
            var data = service.FindSingleWhere(x => x.Id == user.Id);
            Assert.NotNull(data.BillingOn);
            Assert.Equal(2, data.ShopifyChargeId);
            Assert.Equal(2, data.PlanId);
        }

        [Fact]
        public void Get_App_User_By_Id_Async_Should_Return_Valid_Admin_User()
        {
            //setup connection
            MyDbConnection con = new MyDbConnection();
            //setup command
            MyDbCommand command = new MyDbCommand(1/*admin*/);
            command.Connection = con;
            //setup repository
            Mock<IDbRepository<AspNetUser>> repo = new Mock<IDbRepository<AspNetUser>>();
            repo.Setup(x => x.CreateDbCommand(It.IsAny<CommandType>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>())).Returns(command);
            //setup service
            Mock<IDbService<AspNetUser>> service = new Mock<IDbService<AspNetUser>>();
            service.Setup(x => x.FindSingleWhere(It.IsAny<Expression<Func<AspNetUser, bool>>>())).Returns(UserOne);
            service.Setup(x => x.GetRepo()).Returns(repo.Object);
            var data = UserDbServiceHelper.GetAppUserByIdAsync(service.Object, UserOne.Id).Result;
            Assert.NotNull(data);
            Assert.True(data.IsAdmin);
        }

        [Fact]
        public void Get_App_User_By_Id_Async_Should_Return_Non_Admin_User()
        {
            //setup connection
            MyDbConnection con = new MyDbConnection();
            //setup command
            MyDbCommand command = new MyDbCommand(0/*not admin*/);
            command.Connection = con;
            //setup repository
            Mock<IDbRepository<AspNetUser>> repo = new Mock<IDbRepository<AspNetUser>>();
            repo.Setup(x => x.CreateDbCommand(It.IsAny<CommandType>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>())).Returns(command);
            //setup service
            Mock<IDbService<AspNetUser>> service = new Mock<IDbService<AspNetUser>>();
            service.Setup(x => x.FindSingleWhere(It.IsAny<Expression<Func<AspNetUser, bool>>>())).Returns(UserOne);
            service.Setup(x => x.GetRepo()).Returns(repo.Object);
            var data = UserDbServiceHelper.GetAppUserByIdAsync(service.Object, UserOne.Id).Result;
            Assert.NotNull(data);
            Assert.False(data.IsAdmin);
        }

        [Fact]
        public void Get_App_User_By_Id_Async_Should_Return_Null()
        {
            //setup connection
            MyDbConnection con = new MyDbConnection();
            //setup command
            MyDbCommand command = new MyDbCommand(0/*not admin*/);
            command.Connection = con;
            //setup repository
            Mock<IDbRepository<AspNetUser>> repo = new Mock<IDbRepository<AspNetUser>>();
            repo.Setup(x => x.CreateDbCommand(It.IsAny<CommandType>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>())).Returns(command);
            //setup service
            Mock<IDbService<AspNetUser>> service = new Mock<IDbService<AspNetUser>>();
            service.Setup(x => x.FindSingleWhere(It.IsAny<Expression<Func<AspNetUser, bool>>>()));
            service.Setup(x => x.GetRepo()).Returns(repo.Object);
            var data = UserDbServiceHelper.GetAppUserByIdAsync(service.Object, UserOne.Id).Result;
            Assert.Null(data);
        }

        [Fact]
        public void remove_user_should_return_true_for_valid_user_id()
        {
            var user = new AspNetUser()
            {
                Email = "test5@gmail.com",
                MyShopifyDomain = "test5.myshopify.com",
                UserName = "test5.myshopify.com",
                Id = Guid.NewGuid().ToString(),
                ShopifyAccessToken = "validtoken",
                BillingOn = null,
                PlanId = null,
                ShopifyChargeId = null

            };
            service.Add(user);           

            var retTrue = UserDbServiceHelper.RemoveUser(service, user.Id);
            Assert.True(retTrue);

            var retNull = UserDbServiceHelper.GetAppUserByIdAsync(service, user.Id).Result;
            Assert.Null(retNull);
            
        }
        [Fact]
        public void remove_user_should_return_false_for_invalid_user_id()
        {
            //making sure it doesnt exist in the db
            var retNull = UserDbServiceHelper.GetAppUserByIdAsync(service, "invalid_id").Result;
            Assert.Null(retNull);

            var retFalse = UserDbServiceHelper.RemoveUser(service, "invalid_id");
            Assert.False(retFalse);
        }

        protected IDbService<AspNetUser> InitService()
        {
            var options = new DbContextOptionsBuilder<ExicoShopifyDbContext>()
                .UseInMemoryDatabase("Test_User_Db_Service_Helper_DB")
                .Options;

            ExicoShopifyDbContext testContext = new ExicoShopifyDbContext(options);
            //emptying
            testContext.Database.EnsureDeleted();
            ////recreating
            testContext.Database.EnsureCreated();
            //seeding

            testContext.AspNetUsers.Add(UserOne);
            testContext.AspNetUsers.Add(UserTwo);
            testContext.SaveChanges();
            return new TestService<AspNetUser>(new TestRepository<AspNetUser>(new TestUnitOfWork(testContext)));
        }



    }

    #region Dummy Class Implementation For GetAppUserByIdAsync ; THIS IS PURE OVERKILL
    public class MyDbCommand : DbCommand
    {

        public MyDbCommand(int sqlRunResult) : base()
        {
            this.SqlRunResult = sqlRunResult;
        }
        public override string CommandText { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override int CommandTimeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override CommandType CommandType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override bool DesignTimeVisible { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override UpdateRowSource UpdatedRowSource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        protected override DbConnection DbConnection { get => Connection; set => throw new NotImplementedException(); }

        protected override DbParameterCollection DbParameterCollection => throw new NotImplementedException();

        protected override DbTransaction DbTransaction { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Cancel()
        {
            throw new NotImplementedException();
        }

        public override int ExecuteNonQuery()
        {
            throw new NotImplementedException();
        }

        public override object ExecuteScalar()
        {
            return SqlRunResult;
        }

        public override void Prepare()
        {
            throw new NotImplementedException();
        }

        protected override DbParameter CreateDbParameter()
        {
            throw new NotImplementedException();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            throw new NotImplementedException();
        }
        public new DbConnection Connection { get; set; }
        public int SqlRunResult { get; set; }

        public new Task<object> ExecuteScalarAsync()
        {
            return Task.FromResult<object>(SqlRunResult);
        }
    }
    public class MyDbConnection : DbConnection
    {
        public override string ConnectionString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override string Database => throw new NotImplementedException();

        public override string DataSource => throw new NotImplementedException();

        public override string ServerVersion => throw new NotImplementedException();

        public override ConnectionState State => throw new NotImplementedException();

        public override void ChangeDatabase(string databaseName)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            //
        }

        public override void Open()
        {
            //throw new NotImplementedException();
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            throw new NotImplementedException();
        }

        protected override DbCommand CreateDbCommand()
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
