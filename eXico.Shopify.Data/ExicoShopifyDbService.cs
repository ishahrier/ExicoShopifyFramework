using Exico.Shopify.Data.Framework;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Exico.Shopify.Data
{
    public class ExicoShopifyDbService<T>:ABaseDbService<T> where T:class
    {
        public ExicoShopifyDbService(ExicoShopifyDbRepository<T> repo) : base(repo)
        {

        }
    }
}
