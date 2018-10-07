using Exico.Shopify.Data;
using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Exico.Shopify.Web.Core.Modules;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Test.Exico.Shopify.Web.Core
{
    [Collection("Sequential")]
    public class Test_Plan_Reader_helper
    {
        private int TOTAL_PLAN { get; set; } = 5;
        private int TOTAL_DEV_PLAN { get; set; } = 1;
        private ILogger<PlansReader> TheLogger { get; set; }

        public Test_Plan_Reader_helper()
        {
            var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
            var factory = serviceProvider.GetService<ILoggerFactory>();
            TheLogger = factory.CreateLogger<PlansReader>();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Should_Return_Correct_List_Of_Plans(bool includeDev)
        {
            var dbService = GetDbService();
            var m = new MemoryCache(new MemoryCacheOptions() { });
            var helper = new PlansReader(dbService,m, TheLogger);
            if (includeDev) Assert.Equal(TOTAL_PLAN, helper.GetAllPlans(includeDev).Count);
            else Assert.Equal(TOTAL_PLAN - TOTAL_DEV_PLAN, helper.GetAllPlans(includeDev).Count);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(5, true)]
        [InlineData(1, false)]
        [InlineData(5, false)]
        public void Test_Upgradibility(int planId, bool includeDev)
        {
            var dbService = GetDbService();
            var m = new MemoryCache(new MemoryCacheOptions() { });
            var helper = new PlansReader(dbService, m, TheLogger);
            if (TOTAL_PLAN == planId)   /*highest index = highest plan*/
                Assert.False(helper.CanUpgrade(planId, includeDev));
            else Assert.True(helper.CanUpgrade(planId, includeDev));
        }

        [Fact]
        public void Should_Return_A_Valid_Plan_For_Valid_Id()
        {
            var dbService = GetDbService();
            var m = new MemoryCache(new MemoryCacheOptions() { });
            var helper = new PlansReader(dbService, m, TheLogger);
            var plan = helper[1];//first plan in collection is a dev
            Assert.NotNull(plan);
            Assert.Equal(1, plan.Id);
            Assert.True(plan.IsDev);
        }

        [Fact]
        public void Should_Return_Null_Plan_For_Invalid_Id()
        {
            var dbService = GetDbService();
            var m = new MemoryCache(new MemoryCacheOptions() { });
            var helper = new PlansReader(dbService, m, TheLogger);
            var plan = helper[100];//first plan in collection is a dev
            Assert.Null(plan);

        }

        [Fact]
        public void Should_Return_Valid_Plan_For_Valid_Name()
        {
            var dbService = GetDbService();
            var m = new MemoryCache(new MemoryCacheOptions() { });
            var helper = new PlansReader(dbService, m, TheLogger);
            var plan = helper["Test Plan 1"];//first plan in collection is a dev
            Assert.NotNull(plan);
            Assert.Equal(1, plan.Id);

        }
        [Fact]
        public void Should_Return_Null_Plan_For_Invalid_Name()
        {
            var dbService = GetDbService();
            var m = new MemoryCache(new MemoryCacheOptions() { });
            var helper = new PlansReader(dbService, m, TheLogger);
            var plan = helper["Test Plan -1"];//invalid name
            Assert.Null(plan);
        }

        [Fact]
        public void Should_Return_A_Valid_Definition_For_valid_Option_Name_And_Plan_Id()
        {
            var dbService = GetDbService();
            var m = new MemoryCache(new MemoryCacheOptions() { });
            var helper = new PlansReader(dbService, m, TheLogger);
            var planDef = helper[1, "Option 1"];//first plan in collection is a dev
            Assert.NotNull(planDef);
            Assert.Equal(1, planDef.Id);
            Assert.Equal("Value 1", planDef.OptionValue);
        }

        [Fact]
        public void Should_Return_Null_Definition_For_Invalid_Option_Name_But_Valid_Plan_Id()
        {
            var dbService = GetDbService();
            var m = new MemoryCache(new MemoryCacheOptions() { });
            var helper = new PlansReader(dbService, m, TheLogger);
            var planDef = helper[1, "Option 100"];//first plan in collection is a dev
            Assert.Null(planDef);
        }
        [Fact]
        public void Should_Return_Null_Definition_For_Valid_Option_Name_But_Invalid_Plan_Id()
        {
            var dbService = GetDbService();
            var m = new MemoryCache(new MemoryCacheOptions() { });
            var helper = new PlansReader(dbService, m, TheLogger);
            var planDef = helper[100, "Option 1"];//first plan in collection is a dev
            Assert.Null(planDef);
        }

        [Fact]
        public void Should_update_cache_Data_On_Reload()
        {
            var dbService = GetDbService();
            var m = new MemoryCache(new MemoryCacheOptions() { });
            var helper = new PlansReader(dbService, m, TheLogger);
            dbService = GetDbService();
            dbService.Delete(1);
            Assert.Equal(TOTAL_PLAN, helper.GetAllPlans().Count);//before reload
            helper.ReloadFromDBAndUpdateCache( );//PlanData inside this clas is now reinitialized from memory cache data        
            Assert.Equal(TOTAL_PLAN - 1, helper.GetAllPlans().Count);//before reload
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(4)]
        public void Should_Return_A_List_Of_Valid_Upgradable_Plan_List( int currentPlanId)
        {
            var dbService = GetDbService();
            var m = new MemoryCache(new MemoryCacheOptions() { });
            var helper = new PlansReader(dbService, m, TheLogger);

            if (currentPlanId == TOTAL_PLAN)
                Assert.Empty(helper.GetAvailableUpgrades(currentPlanId));
            else
                Assert.Equal(TOTAL_PLAN - currentPlanId, helper.GetAvailableUpgrades(currentPlanId).Count);
        }

        public IDbService<Plan> GetDbService()
        {
            var options = new DbContextOptionsBuilder<ExicoShopifyDbContext>().UseInMemoryDatabase("plan_helper").Options;

            ExicoShopifyDbContext testContext = new ExicoShopifyDbContext(options);
            //emptying
            testContext.Database.EnsureDeleted();
            ////recreating
            testContext.Database.EnsureCreated();

            //seeding 
            for (int i = 1; i <= TOTAL_PLAN; i++)
            {
                var plan = new Plan()
                {
                    Active = true,
                    Description = $"Test Description {i}",
                    Footer = $"Test Footer {i}",
                    IsDev = (i == 1),
                    IsTest = false,
                    Name = $"Test Plan {i}",
                    Price = i,
                    DisplayOrder = i,
                    TrialDays = (short)(5 * i),
                    Id = i
                };

                for (int j = 1; j <= 5; j++)
                {
                    plan.PlanDefinitions.Add(new PlanDefinition()
                    {
                        Id = (5 * (i - 1)) + j,
                        Description = $"Description {j}",
                        OptionName = $"Option {j}",
                        OptionValue = $"Value {j}",
                        PlanId = i
                    });
                }
                testContext.Add(plan);
            }

            testContext.SaveChanges();

            return new TestService<Plan>(new TestRepository<Plan>(new TestUnitOfWork(testContext)));
        }
    }
}
