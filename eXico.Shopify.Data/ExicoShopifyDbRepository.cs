using Exico.Shopify.Data.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Exico.Shopify.Data
{
    public class ExicoShopifyDbRepository<T> : ABaseDbRepository<T> where T : class
    {
        public ExicoShopifyDbRepository(ExicoShopifyDbUnitOfWork uow) : base(uow)
        {

        }
    }

}
