using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text; 
using System.Linq;
using System.Linq.Expressions;
 

namespace Exico.Shopify.Data.Framework.Extensions
{
   public static class RepositoryExtensions
    {
        public static T GetFirst<T>(this IDbRepository<T> repo  ) where T:class
        {
            var _ctx =   repo.GetUnitOfWork().GetContext().Set<T>();
            return _ctx.AsQueryable().FirstOrDefault();
        }
    }
  
}
