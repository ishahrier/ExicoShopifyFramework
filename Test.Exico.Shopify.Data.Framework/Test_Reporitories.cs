using Exico.Shopify.Data;
using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;
using Exico.Shopify.Data.Framework.Extensions;
using System.Linq;

namespace Test.Exico.Shopify.Data.Framework
{
    [Collection("Sequential")]
    public class Test_Reporitories : ABaseTestClass
    {

        public Test_Reporitories(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void Should_Count_10()
        {
            using (var repo = GetInMemoryRepositoryWithSeed())
            {
                int total = repo.Count();
                Assert.Equal(10, total);
            }
        }

        [Fact]
        public void Should_Add_New_Record()
        {
            using (var repo = GetInMemoryRepositoryWithSeed())
            {

                int beforeAdding = repo.Count();
                repo.Add(new Plan()
                {
                    Active = true,
                    Description = "TestPlan",
                    Footer = "footer",
                    IsDev = true,
                    IsTest = false,
                    Name = "Ultimate Plan",
                    Price = 10.11m,
                    DisplayOrder = 1,
                    TrialDays = 5,
                    Id = beforeAdding + 1

                });

                repo.Save();
                int afterAdding = beforeAdding + 1;
                Output.WriteLine($"Count Before Adding : {beforeAdding} and After {afterAdding} ");
                Assert.Equal(afterAdding, repo.Count());
            }

        }

        [Fact]
        public void Should_Delete_Record()
        {
            using (var repo = GetInMemoryRepositoryWithSeed())
            {
                int beforeDeleting = repo.Count();
                repo.Delete(1);
                repo.Save();
                int afterDeleting = beforeDeleting - 1;
                Output.WriteLine($"Count Before Deleting : {beforeDeleting} and After should be {afterDeleting} ");
                Assert.Equal(afterDeleting, repo.Count());
            }

        }

        [Fact]
        public void Should_Update_Record_Disconnectedly()
        {

            //this will delete everything from other tests and re seed data
            using (var repo = GetInMemoryRepositoryWithSeed()) { }

            //now i do not want reseed or delete, i want to update the esiting database 
            using (var repo = GetInMemoryRepositoryShared())
            {
                var plan = new Plan()
                {
                    Active = false,
                    Description = "description",
                    Footer = "footer",
                    IsDev = false,
                    IsTest = true,
                    Name = "name",
                    Price = 100,
                    DisplayOrder = 1,
                    TrialDays = 100,
                    Id = 1
                };
                repo.Update(plan, plan.Id);
                repo.Save();

            }

            //simulating dosconnected database
            using (var repo = GetInMemoryRepositoryShared())
            {
                var updated = repo.GetByKey(1);
                Assert.Equal("name", updated.Name);
                Assert.Equal("footer", updated.Footer);
                Assert.Equal(1, updated.Id);
            }
        }

        [Fact]
        public void Should_Get_By_Id_1()
        {
            using (var repo = GetInMemoryRepositoryWithSeed())
            {
                int i = 1;
                var data = repo.GetByKey(i);
                Assert.NotNull(data);
                Assert.Equal(i, data.Id);
                Assert.False(string.IsNullOrEmpty(data.Name));

            }
        }

        [Fact]
        public void Should_Get_Single_By_Name()
        {
            using (var repo = GetInMemoryRepositoryWithSeed())
            {
                int i = 1;
                string name = $"Test Plan {i}";
                var list = new List<System.Linq.Expressions.Expression<Func<Plan, bool>>>();
                list.Add(x => x.Name == name);
                var data = repo.GetSingle(list, "");
                Assert.NotNull(data);
                Assert.Equal(i, data.Id);
                Assert.Equal(name, data.Name);

            }
        }

        [Fact]
        public void Should_Get_List_of_3_result()
        {
            using (var repo = GetInMemoryRepositoryWithSeed())
            {
                var list = new List<System.Linq.Expressions.Expression<Func<Plan, bool>>>();
                list.Add(x => x.Price <= 3);
                var data = repo.GetMany(list, "").ToList();
                Assert.NotNull(data);
                Assert.Equal(3, data.Count);
            }
        }

        [Fact]
        public void Test_Many_With_Multiple_Filters()
        {
            using (var repo = GetInMemoryRepositoryWithSeed())
            {
                var list = new List<System.Linq.Expressions.Expression<Func<Plan, bool>>>();
                list.Add(x => x.Price <= 3);//3 results
                list.Add(x => x.Id <= 2);//then filter again makes 2 records
                var data = repo.GetMany(list, "").ToList();
                Assert.NotNull(data);
                Assert.Equal(2, data.Count);
            }
        }

        [Fact]
        public void Test_Single_With_Multiple_Filters()
        {
            using (var repo = GetInMemoryRepositoryWithSeed())
            {
                int id = 1;
                var list = new List<System.Linq.Expressions.Expression<Func<Plan, bool>>>();
                list.Add(x => x.Price <= 5);//5 results
                list.Add(x => x.Id == id);//then filter again makes 1 record
                var data = repo.GetSingle(list, "");
                Assert.NotNull(data);
                Assert.Equal(id, data.Id);
            }
        }


        [Fact]
        public void Test_Ordering_in_Many()
        {
            using (var repo = GetInMemoryRepositoryWithSeed())
            {
                var ordered = repo.GetMany(null, "", null).ToList().OrderByDescending(x => x.Id);

                var data = repo.GetMany(null, "", (x) => x.OrderByDescending(y => y.Id)).ToList();
                Assert.Equal(ordered, data);

            }
        }

        [Fact]
        public void Test_Single_With_Navigation()
        {

            using (var repo = GetInMemoryRepositoryWithSeed())
            {
                var withNavData = repo.GetSingle(null, "PlanDefinitions");
                Assert.NotNull(withNavData);
                Assert.Equal(5, withNavData.PlanDefinitions.Count);
            }
        }

        [Fact]
        public void Test_Many_With_Navigations()
        {

            using (var repo = GetInMemoryRepositoryWithSeed())
            {
                var withNavData = repo.GetMany(null, "PlanDefinitions",null).ToList();
                Assert.NotEmpty(withNavData);
                Assert.Equal(5, withNavData[0].PlanDefinitions.Count);
            }
        }


        #region Helpers
        protected static IDbRepository<Plan> GetInMemoryRepositoryShared()
        {
            var options = new DbContextOptionsBuilder<ExicoShopifyDbContext>()
                .UseInMemoryDatabase("fixed_name")
                .Options;
            ExicoShopifyDbContext testContext = new ExicoShopifyDbContext(options);
            return new TestRepository<Plan>(new TestUnitOfWork(testContext));
        }

        protected static IDbRepository<Plan> GetInMemoryRepositoryWithSeed()
        {
            var options = new DbContextOptionsBuilder<ExicoShopifyDbContext>()
                .UseInMemoryDatabase("fixed_name")
                .Options;

            ExicoShopifyDbContext testContext = new ExicoShopifyDbContext(options);
            //emptying
            testContext.Database.EnsureDeleted();
            ////recreating
            testContext.Database.EnsureCreated();
            //seeding
            for (int i = 1; i <= 10; i++)
            {
                var plan = new Plan()
                {
                    Active = true,
                    Description = $"Test Description {i}",
                    Footer = $"Test Footer {i}",
                    IsDev = true,
                    IsTest = false,
                    Name = $"Test Plan {i}",
                    Price = i,
                   DisplayOrder = 1,
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
                testContext.Plans.Add(plan);
            }
            testContext.SaveChanges();
            return new TestRepository<Plan>(new TestUnitOfWork(testContext));
        }
        #endregion
    }

}
