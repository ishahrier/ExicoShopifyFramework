using Exico.Shopify.Data.Framework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Exico.Shopify.Data
{
    public class ExicoShopifyDbUnitOfWork : ABaseDbUnitOfWork
    {
        public ExicoShopifyDbUnitOfWork(ExicoShopifyDbContext context) : base(context)
        {

        }
    }
}
