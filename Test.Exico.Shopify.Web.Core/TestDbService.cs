using Exico.Shopify.Data;
using Exico.Shopify.Data.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Exico.Shopify.Web.Core
{
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
