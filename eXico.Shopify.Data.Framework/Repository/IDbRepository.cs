using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Exico.Shopify.Data.Framework
{
    public interface IDbRepository<T> : IDisposable
    where T : class
    {
        T Add(T t);

        T Update(T updated, object key);

        void Delete(object key);

        int Save();

        int Count(List<Expression<Func<T, bool>>> filters = null);

        T GetByKey(object key);

        T GetSingle(List<Expression<Func<T, bool>>> filters = null, string includeProperties = "");

        IQueryable<T> GetMany(List<Expression<Func<T, bool>>> filters=null, string includeProperties = "", Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null);

        IDbUnitOfWork GetUnitOfWork();
        DbCommand CreateDbCommand(CommandType type, string text, Dictionary<string, object> paramDictionary);
    }
}
