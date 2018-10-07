using Exico.Shopify.Data;
using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Test.Exico.Shopify.Data.Framework
{
    public abstract class ABaseTestClass
    {
        protected ITestOutputHelper Output;

        public ABaseTestClass(ITestOutputHelper testOutputHelper)
        {
            Output = testOutputHelper;
        }



    }

    public class TestUnitOfWork : ABaseDbUnitOfWork
    {
        public TestUnitOfWork(ExicoShopifyDbContext context) : base(context)
        {

        }
    }
    public class TestRepository<T> : ABaseDbRepository<T> where T : class
    {
        public TestRepository(TestUnitOfWork uow) : base(uow)
        {

        }
    }
    public class TestService<T> : ABaseDbService<T> where T : class
    {
        public TestService(TestRepository<T> repo) : base(repo)
        {

        }
    }
}
