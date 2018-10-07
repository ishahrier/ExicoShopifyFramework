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
    public class Test_Services : ABaseTestClass
    {
        private int TOTAL_PLAN_RECORDS = 20;
        private int TOTAL_PLAN_DEFINITION_RECORDS_PER_PLAN = 5;

        public Test_Services(ITestOutputHelper output) : base(output)
        {
        }


        #region Mixed
        [Fact]
        public void Should_Return_Repo()
        {
            using (var service = GetServiceWithData())
            {
                var repo = service.GetRepo();
                Assert.NotNull(repo);
                Assert.IsType<TestRepository<Plan>>(repo);
            }
        }

        [Fact]
        public void Should_Dispose_Context()
        {
            IDbRepository<Plan> repo;
            using (var service = GetServiceWithData())
            {
                repo = service.GetRepo();
                Assert.NotNull(repo);
                Assert.IsType<TestRepository<Plan>>(repo);
            }

            Assert.True(repo.GetUnitOfWork().Disposed);
        }

        [Fact]
        public void Should_NOT_Dispose_Context()
        {
            IDbRepository<Plan> repo;
            using (var service = GetServiceWithData())
            {
                repo = service.GetRepo();
                Assert.NotNull(repo);
                Assert.IsType<TestRepository<Plan>>(repo);
                Assert.False(repo.GetUnitOfWork().Disposed);
            }


        }

        [Fact]
        public void Should_Add_A_Record()
        {
            using (var service = GetServiceWithData())
            {

                service.Add(new Plan()
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
                    Id = TOTAL_PLAN_RECORDS + 1

                });

                Assert.Equal(TOTAL_PLAN_RECORDS + 1, service.GetRepo().Count());
            }
        }

        [Fact]
        public void Should_Delete_A_Record()
        {
            using (var service = GetServiceWithData())
            {
                int beforeDeleting = TOTAL_PLAN_RECORDS;
                service.Delete(1);
                int afterDeleting = TOTAL_PLAN_RECORDS - 1;
                Output.WriteLine($"Count Before Deleting : {beforeDeleting} and After should be {afterDeleting} ");
                Assert.Equal(afterDeleting, service.GetRepo().Count());
            }

        }

        [Fact]
        public void Should_Update_Record_Disconnectedly()
        {

            //this will delete everything from other tests and re seed data
            using (var service = GetServiceWithData()) { }

            //now i do not want reseed or delete, i want to update the esiting database 
            using (var service = GetServiceOnly())
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
                service.Update(plan, plan.Id);


            }

            //simulating dosconnected database
            using (var service = GetServiceOnly())
            {
                var updated = service.GetRepo().GetByKey(1);
                Assert.Equal("name", updated.Name);
                Assert.Equal("footer", updated.Footer);
                Assert.Equal(1, updated.Id);
            }
        }

        #endregion

        #region Count
        [Fact]
        public void Test_Count_With_No_Filter()
        {
            using (var service = GetServiceWithData())
            {
                Assert.Equal(TOTAL_PLAN_RECORDS, service.Count());
            }
        }

        [Fact]
        public void Test_Count_With_Single_Filter()
        {
            using (var service = GetServiceWithData())
            {
                Assert.Equal(TOTAL_PLAN_RECORDS - 10, service.Count(x => x.Id <= 10));
            }
        }


        [Fact]
        public void Test_Count_With_List_Of_Filters()
        {
            using (var service = GetServiceWithData())
            {
                var list = new List<System.Linq.Expressions.Expression<Func<Plan, bool>>>();
                list.Add(x => x.Id <= 15);//-5
                list.Add(x => x.Price <= 5);//-10
                Assert.Equal(((TOTAL_PLAN_RECORDS - 5) - 10), service.Count(list));
            }
        }

        #endregion

        #region Find Single Where

        [Fact]
        public void Test_Find_Single_Where_With_One_Where()
        {
            using (var service = GetServiceWithData())
            {
                var data = service.FindSingleWhere(x => x.Id == 10);
                Assert.Equal(10, data.Id);
            }
        }
        [Fact]
        public void Find_Single_Where_With_One_Where_Should_Return_Null_If_Where_Doesnt_Match()
        {
            using (var service = GetServiceWithData())
            {
                var data = service.FindSingleWhere(x => x.Id == 100000);
                Assert.Null(data);
            }
        }

        [Fact]
        public void Test_Find_Single_Where_With_One_Where_And_Property()
        {
            using (var service = GetServiceWithData()) { }
            using (var service = GetServiceOnly())
            {
                var data = service.FindSingleWhere(x => x.Id == 10,"PlanDefinitions");
                Assert.Equal(10, data.Id);
                Assert.Equal(TOTAL_PLAN_DEFINITION_RECORDS_PER_PLAN, data.PlanDefinitions.Count);
            }
        }

        [Fact]
        public void Test_Find_Single_Where_With_List_Of_Wheres()
        {
            using (var service = GetServiceWithData())
            {
                var list = new List<System.Linq.Expressions.Expression<Func<Plan, bool>>>();
                list.Add(x => x.Id == 10);
                list.Add(x => x.Price < 9);
                Plan data = service.FindSingleWhere(list);
                Assert.Null(data);
                list.RemoveAt(1);
                list.Add(x => x.Price <= 10);
                data = service.FindSingleWhere(list);
                Assert.Equal(10, data.Id);
            }
        }

        [Fact]
        public void Test_Find_Single_Where_With_List_Of_Wheres_And_Property()
        {
            using (var service = GetServiceWithData())
            {
                var list = new List<System.Linq.Expressions.Expression<Func<Plan, bool>>>();
                list.Add(x => x.Id == 10);
                list.Add(x => x.Price < 9);
                Plan data = service.FindSingleWhere(list, "PlanDefinitions");
                Assert.Null(data);
                list.RemoveAt(1);
                list.Add(x => x.Price <= 10);
                data = service.FindSingleWhere(list,"PlanDefinitions");
                Assert.Equal(10, data.Id);
                Assert.Equal(TOTAL_PLAN_DEFINITION_RECORDS_PER_PLAN, data.PlanDefinitions.Count);
            }
        }

        #endregion

        #region Find Many Where

        [Fact]
        public void Test_Find_Many_With_Where()
        {
            using (var service = GetServiceWithData())
            {
                Assert.Equal(TOTAL_PLAN_RECORDS - 10, service.FindManyWhere(x => x.Id <= 10).Count);
            }
        }

        [Fact]
        public void Test_Find_Many_With_Where_And_Property()
        {
            using (var service = GetServiceWithData())
            { }
            using (var service = GetServiceOnly())
            {
                var data = service.FindManyWhere(x => x.Id <= 10, "PlanDefinitions"); ;
                Assert.NotEmpty(data);
                Assert.Equal(TOTAL_PLAN_RECORDS - 10, data.Count);
                Assert.Equal(TOTAL_PLAN_DEFINITION_RECORDS_PER_PLAN, data[0].PlanDefinitions.Count);
            }
        }

        [Fact]
        public void Test_Find_Many_With_Where_And_Property_Sorted()
        {
            using (var service = GetServiceWithData())
            { }

            using (var service = GetServiceOnly())
            {
                var vanillaResult = service.GetRepo().GetMany().Where(x => x.Id <= 10).OrderByDescending(x => x.Id);

                var findResult = service.FindManyWhere(x => x.Id <= 10, "PlanDefinitions", x => x.OrderByDescending(y => y.Id));
                Assert.NotEmpty(findResult);
                Assert.Equal(TOTAL_PLAN_RECORDS - 10, findResult.Count);
                Assert.Equal(TOTAL_PLAN_DEFINITION_RECORDS_PER_PLAN, findResult[0].PlanDefinitions.Count);
                Assert.Equal(vanillaResult, findResult);
            }
        }

        [Fact]
        public void Test_Find_Many_With_Where_And_Sorted()
        {
            using (var service = GetServiceWithData())
            {
                var vanillaResult = service.GetRepo().GetMany().Where(x => x.Id <= 10).OrderByDescending(x => x.Id);

                var findResult = service.FindManyWhere(x => x.Id <= 10, x => x.OrderByDescending(y => y.Id));
                Assert.NotEmpty(findResult);
                Assert.Equal(TOTAL_PLAN_RECORDS - 10, findResult.Count);
                Assert.Equal(vanillaResult, findResult);
            }
        }

        [Fact]
        public void Test_Find_Many_With_List_Of_Wheres()
        {
            using (var service = GetServiceWithData())
            {
                var list = new List<System.Linq.Expressions.Expression<Func<Plan, bool>>>();
                list.Add(x => x.Id <= 15);//-5
                list.Add(x => x.Price <= 10);//-5
                var result = service.FindManyWhere(list);
                Assert.NotEmpty(result);
                Assert.Equal(TOTAL_PLAN_RECORDS - 10, result.Count);
            }
        }

        [Fact]
        public void Test_Find_Many_With_List_Of_Wheres_And_Properties()
        {
            using (var service = GetServiceWithData())
            { }
            using (var service = GetServiceOnly())
            {
                var list = new List<System.Linq.Expressions.Expression<Func<Plan, bool>>>();
                list.Add(x => x.Id <= 15);//-5
                list.Add(x => x.Price <= 10);//-5
                var result = service.FindManyWhere(list, "PlanDefinitions");
                Assert.NotEmpty(result);
                Assert.Equal(TOTAL_PLAN_RECORDS - 10, result.Count);
                Assert.Equal(TOTAL_PLAN_DEFINITION_RECORDS_PER_PLAN, result[0].PlanDefinitions.Count);
            }
        }

        [Fact]
        public void Test_Find_Many_With_List_Of_Wheres_And_Properties_And_Sorted()
        {
            using (var service = GetServiceWithData())
            { }
            using (var service = GetServiceOnly())
            {
                var vanillaResult = service.GetRepo().GetMany().OrderByDescending(x => x.Id).Where(x => x.Id <= 15).Where(x => x.Price <= 10);

                var list = new List<System.Linq.Expressions.Expression<Func<Plan, bool>>>();
                list.Add(x => x.Id <= 15);//-5
                list.Add(x => x.Price <= 10);//-5
                var result = service.FindManyWhere(list, "PlanDefinitions", x => x.OrderByDescending(y => y.Id));
                Assert.NotEmpty(result);
                Assert.Equal(TOTAL_PLAN_RECORDS - 10, result.Count);
                Assert.Equal(TOTAL_PLAN_DEFINITION_RECORDS_PER_PLAN, result[0].PlanDefinitions.Count);
                Assert.Equal(vanillaResult, result);
            }
        }

        [Fact]
        public void Test_Find_Many_With_List_Of_Wheres_And_Sorted()
        {
            using (var service = GetServiceWithData())
            {
                //has plans and definitoina in memory
            }

            using (var service = GetServiceOnly()) //only plans unless we explicitely call for definitions
            {
                var vanillaResult = service.GetRepo().GetMany().OrderByDescending(x => x.Id).Where(x => x.Id <= 15).Where(x => x.Price <= 10);

                var list = new List<System.Linq.Expressions.Expression<Func<Plan, bool>>>();
                list.Add(x => x.Id <= 15);//-5
                list.Add(x => x.Price <= 10);//-5
                var result = service.FindManyWhere(list, "", x => x.OrderByDescending(y => y.Id));
                Assert.NotEmpty(result);
                Assert.Equal(TOTAL_PLAN_RECORDS - 10, result.Count);
                Assert.Empty(result[0].PlanDefinitions);
                Assert.Equal(vanillaResult, result);
            }
        }

        #endregion

        #region Find All
        [Fact]
        public void Test_Find_All()
        {
            using (var service = GetServiceWithData())
            {
                var list = new List<System.Linq.Expressions.Expression<Func<Plan, bool>>>();
                Assert.Equal(TOTAL_PLAN_RECORDS, service.FindAll().Count);
            }
        }

        [Fact]
        public void Test_Find_All_With_Properties()
        {
            using (var service = GetServiceWithData())
            {
            }

            using (var service = GetServiceOnly())
            {
                var list = new List<System.Linq.Expressions.Expression<Func<Plan, bool>>>();
                var data = service.FindAll("PlanDefinitions");
                Assert.NotEmpty(data);
                Assert.Equal(TOTAL_PLAN_DEFINITION_RECORDS_PER_PLAN, data[0].PlanDefinitions.Count);
            }
        }

        [Fact]
        public void Test_Find_All_Sorted()
        {
            using (var service = GetServiceWithData())
            {
                var vanillaResult = service.GetRepo().GetMany().OrderByDescending(x => x.Id);

                var list = new List<System.Linq.Expressions.Expression<Func<Plan, bool>>>();
                var findResult = service.FindAll(x => x.OrderByDescending(y => y.Id));
                Assert.NotEmpty(findResult);
                Assert.Equal(vanillaResult, findResult);
            }
        }

        [Fact]
        public void Test_Find_All_With_Properties_And_Sorted()
        {
            using (var service = GetServiceWithData())
            { }
            using (var service = GetServiceOnly())
            {
                var vanillaResult = service.GetRepo().GetMany().OrderByDescending(x => x.Id);

                var list = new List<System.Linq.Expressions.Expression<Func<Plan, bool>>>();
                var findResult = service.FindAll("PlanDefinitions", x => x.OrderByDescending(y => y.Id));
                Assert.NotEmpty(findResult);
                Assert.Equal(vanillaResult, findResult);
                Assert.Equal(TOTAL_PLAN_DEFINITION_RECORDS_PER_PLAN, findResult[0].PlanDefinitions.Count);

            }
        }

        #endregion

        #region Find Many Where - Paged

        [Fact]
        public void Test_Find_Many_Where_Paged_With_One_Where()
        {
            using (var service = GetServiceWithData())
            {
                //total 19 results
                var vanillaResult = service.GetRepo().GetMany().OrderByDescending(x => x.Id).Where(x => x.Id <= 19);

                //should have 19 results in total
                //pages = 4 pages
                var pagedData = service.FindManyWherePaged(4, 5, x => x.OrderByDescending(y => y.Id), x => x.Id <= 19);

                Assert.Equal(pagedData.TotalCount, vanillaResult.Count());//total found
                Assert.Equal(4, pagedData.TotalPagesCount);//test total pages
                Assert.Equal(4, pagedData.PageNum);//current page
                Assert.Equal(5, pagedData.ItemsPerPage);//items per page
                Assert.Equal(4, pagedData.ResultCount);//total result returned for current page, we are on the last page
                Assert.Equal(1, pagedData.ResultData.Last().Id);//checking data, because we did reverse order
                Assert.Equal(vanillaResult.Skip(15).Take(4), pagedData.ResultData);//check our last page content
            }
        }

        [Fact]
        public void Test_Find_Many_Where_Paged_With_One_Where_And_Property()
        {
            using (var service = GetServiceWithData())
            {
            }
            using (var service = GetServiceOnly())
            {
                //total 19 results
                var vanillaResult = service.GetRepo().GetMany().OrderByDescending(x => x.Id).Where(x => x.Id <= 19);

                //should have 19 results in total
                //pages = 4 pages
                var pagedData = service.FindManyWherePaged(4, 5, x => x.OrderByDescending(y => y.Id), x => x.Id <= 19, "PlanDefinitions");

                Assert.Equal(pagedData.TotalCount, vanillaResult.Count());//total found
                Assert.Equal(4, pagedData.TotalPagesCount);//test total pages
                Assert.Equal(4, pagedData.PageNum);//current page
                Assert.Equal(5, pagedData.ItemsPerPage);//items per page
                Assert.Equal(4, pagedData.ResultCount);//total result returned for current page, we are on the last page
                Assert.Equal(1, pagedData.ResultData.Last().Id);//checking data, because we did reverse order
                Assert.Equal(vanillaResult.Skip(15).Take(4), pagedData.ResultData);//check our last page content
                Assert.Equal(TOTAL_PLAN_DEFINITION_RECORDS_PER_PLAN, pagedData.ResultData[0].PlanDefinitions.Count);
            }
        }

        [Fact]
        public void Test_Find_Many_Where_Paged_With_List_Of_Where()
        {
            using (var service = GetServiceWithData())
            {
                //total 18 results
                var vanillaResult = service.GetRepo().GetMany().OrderByDescending(x => x.Id).Where(x => x.Id <= 19).Where(x => x.Price <= 18);

                //should have 18 results in total
                //pages = 4 pages
                var list = new List<System.Linq.Expressions.Expression<Func<Plan, bool>>>();
                list.Add(x => x.Id <= 19);
                list.Add(x => x.Price <= 18);
                var pagedData = service.FindManyWherePaged(4, 5, x => x.OrderByDescending(y => y.Id), list);

                Assert.Equal(pagedData.TotalCount, vanillaResult.Count());//total found
                Assert.Equal(4, pagedData.TotalPagesCount);//test total pages
                Assert.Equal(4, pagedData.PageNum);//current page
                Assert.Equal(5, pagedData.ItemsPerPage);//items per page
                Assert.Equal(3, pagedData.ResultCount);//total result returned for current page, we are on the last page
                Assert.Equal(1, pagedData.ResultData.Last().Id);//checking data, because we did reverse order                
                Assert.Equal(vanillaResult.Skip(15).Take(3), pagedData.ResultData);//check our last page content

            }
        }

        [Fact]
        public void Test_Find_Many_Where_Paged_With_List_Of_Where_And_Property()
        {
            using (var service = GetServiceWithData())
            {
            }
            using (var service = GetServiceOnly())
            {
                //total 18 results
                var vanillaResult = service.GetRepo().GetMany().OrderByDescending(x => x.Id).Where(x => x.Id <= 19).Where(x => x.Price <= 18);

                //should have 18 results in total
                //pages = 4 pages
                var list = new List<System.Linq.Expressions.Expression<Func<Plan, bool>>>();
                list.Add(x => x.Id <= 19);
                list.Add(x => x.Price <= 18);
                var pagedData = service.FindManyWherePaged(4, 5, x => x.OrderByDescending(y => y.Id), list, "PlanDefinitions");

                Assert.Equal(pagedData.TotalCount, vanillaResult.Count());//total found
                Assert.Equal(4, pagedData.TotalPagesCount);//test total pages
                Assert.Equal(4, pagedData.PageNum);//current page
                Assert.Equal(5, pagedData.ItemsPerPage);//items per page
                Assert.Equal(3, pagedData.ResultCount);//total result returned for current page, we are on the last page
                Assert.Equal(1, pagedData.ResultData.Last().Id);//checking data, because we did reverse order                
                Assert.Equal(vanillaResult.Skip(15).Take(3), pagedData.ResultData);//check our last page content

                Assert.Equal(TOTAL_PLAN_DEFINITION_RECORDS_PER_PLAN, pagedData.ResultData[0].PlanDefinitions.Count);//checking properties
            }
        }

        [Fact]
        public void Test_Find_Many_Paged_Thorw_Eexception_On_Null_Ordering()
        {
            //all FindManyWherePaged uses an internal parent method, so if one thorws error we do not need to test other FindManyWherePaged functions

            using (var service = GetServiceWithData())
            {
                var exception = Assert.ThrowsAny<Exception>(() => service.FindManyWherePaged(1, 5, null, x => x.Id <= 19, "PlanDefinitions"));
                Assert.Contains("Paging Cannot Be Done Without OrderBy Clause",exception.Message);
            }
        }
        #endregion

        #region Find All - Paged

        [Fact]
        public void Test_Find_All_Paged()
        {
            using (var service = GetServiceWithData())
            {
                //total 19 results
                var vanillaResult = service.GetRepo().GetMany().OrderByDescending(x => x.Id);

                //should have 19 results in total
                //pages = 4 pages
                var pagedData = service.FindAllPaged(4, 5, x => x.OrderByDescending(y => y.Id));

                Assert.Equal(pagedData.TotalCount, vanillaResult.Count());//total found
                Assert.Equal(4, pagedData.TotalPagesCount);//test total pages
                Assert.Equal(4, pagedData.PageNum);//current page
                Assert.Equal(5, pagedData.ItemsPerPage);//items per page
                Assert.Equal(5, pagedData.ResultCount);//total result returned for current page, we are on the last page
                Assert.Equal(1, pagedData.ResultData.Last().Id);//checking data, because we did reverse order
                Assert.Equal(vanillaResult.Skip(15).Take(5), pagedData.ResultData);//check our last page content                
            }
        }

        [Fact]
        public void Test_Find_All_Paged_With_Properties()
        {
            using (var service = GetServiceWithData())
            {
            }
            using (var service = GetServiceOnly())
            {
                //total 19 results
                var vanillaResult = service.GetRepo().GetMany().OrderByDescending(x => x.Id);

                //should have 19 results in total
                //pages = 4 pages
                var pagedData = service.FindAllPaged(4, 5, x => x.OrderByDescending(y => y.Id), "PlanDefinitions");

                Assert.Equal(pagedData.TotalCount, vanillaResult.Count());//total found
                Assert.Equal(4, pagedData.TotalPagesCount);//test total pages
                Assert.Equal(4, pagedData.PageNum);//current page
                Assert.Equal(5, pagedData.ItemsPerPage);//items per page
                Assert.Equal(5, pagedData.ResultCount);//total result returned for current page, we are on the last page
                Assert.Equal(1, pagedData.ResultData.Last().Id);//checking data, because we did reverse order
                Assert.Equal(vanillaResult.Skip(15).Take(5), pagedData.ResultData);//check our last page content
                Assert.Equal(TOTAL_PLAN_DEFINITION_RECORDS_PER_PLAN, pagedData.ResultData[0].PlanDefinitions.Count);
            }
        }

        [Fact]
        public void Test_Find_All_Paged_Throws_Eexception_On_Null_Ordering()
        {
            //all FindAllPaged uses an internal parent method, so if one thorws error we do not need to test other FindAllPaged functions

            using (var service = GetServiceWithData())
            {
                var exception = Assert.ThrowsAny<Exception>(() => service.FindAllPaged(1, 5, null));
                Assert.Contains("Paging Cannot Be Done Without OrderBy Clause", exception.Message);
            }
        }
        #endregion

        #region Helpers
        protected IDbService<Plan> GetServiceOnly()
        {
            var options = new DbContextOptionsBuilder<ExicoShopifyDbContext>()
                .UseInMemoryDatabase("fixed_name")
                .Options;
            ExicoShopifyDbContext testContext = new ExicoShopifyDbContext(options);
            return new TestService<Plan>(new TestRepository<Plan>(new TestUnitOfWork(testContext)));
        }

        protected IDbService<Plan> GetServiceWithData(int? total_plans = null, int? total_plan_definitions = null)
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
            for (int i = 1; i <= (total_plans ?? TOTAL_PLAN_RECORDS); i++)
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
                    DisplayOrder = i,
                    TrialDays = (short)(5 * i),
                    Id = i


                };

                for (int j = 1; j <= (total_plan_definitions ?? TOTAL_PLAN_DEFINITION_RECORDS_PER_PLAN); j++)
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
            return new TestService<Plan>(new TestRepository<Plan>(new TestUnitOfWork(testContext)));
        }

        #endregion
    }

}
