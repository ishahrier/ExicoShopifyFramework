using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Exico.Shopify.Data.Framework
{
    public   class ABaseDbRepository<T> : IDbRepository<T> where T : class
    {
        protected IDbUnitOfWork uow { get; }

        private ABaseDbRepository()
        {

        }

        public ABaseDbRepository(IDbUnitOfWork uow)
        {
            this.uow = uow;
        }

        private IQueryable<T> IncludeProperty(IQueryable<T> query, string includeProperties = "")
        {
            foreach (var includeProperty in includeProperties.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }
            return query;
        }

        public T Add(T t)
        {
            uow.GetContext().Set<T>().Add(t);
            return t;
        }

        public T Update(T updated, object key)
        {
            if (updated == null)
                return null;            

            T existing = uow.GetContext().Set<T>().Find(key);
            if (existing != null)
            {
                uow.GetContext().Entry(existing).CurrentValues.SetValues(updated);
            }
            return existing;
        }

        public void Delete(object key)
        {
            T t2 = uow.GetContext().Set<T>().Find(key);
            if (t2 != null)
            {
                uow.GetContext().Set<T>().Remove(t2);
            }
        }

        public int Save()
        {
            return this.uow.Save();
        }

        public int Count(List<Expression<Func<T, bool>>> filters = null)
        {
            if (filters == null)
            {
                return uow.GetContext().Set<T>().Count();
            }
            else
            {
                if (filters.Count() > 0)
                {
                    IQueryable<T> query = uow.GetContext().Set<T>();
                    foreach (var f in filters)
                    {
                        query = query.Where(f);
                    }

                    return query.Count();
                }
                else
                {
                    return uow.GetContext().Set<T>().Count();
                }
            }
        }

        public T GetByKey(object key)
        {
            return uow.GetContext().Set<T>().Find(key);

        }

        public T GetSingle(List<Expression<Func<T, bool>>> filters = null, string includeProperties = "")
        {
            var query = this.GetMany(filters, includeProperties, null);
            return query.FirstOrDefault();
        }

        public IQueryable<T> GetMany(List<Expression<Func<T, bool>>> filters = null, string includeProperties = "", Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null)
        {
            IQueryable<T> query = uow.GetContext().Set<T>();

            if (filters != null)
            {
                if (filters.Count() > 0)
                {
                    foreach (var f in filters)
                    {
                        query = query.Where(f);
                    }
                }
            }
            query = this.IncludeProperty(query, includeProperties);

            return orderBy == null ? query.AsQueryable() : orderBy(query);
        }

        public void Dispose()
        {
            uow.Dispose();
            GC.SuppressFinalize(this);
        }

        public IDbUnitOfWork GetUnitOfWork()
        {
            return uow;
        }

        public DbCommand CreateDbCommand(CommandType type, string text, Dictionary<string,object> paramDictionary)
        {
            var command  = GetUnitOfWork().GetContext().Database.GetDbConnection().CreateCommand();
            command.CommandText = text;
            command.CommandType = type;
            if (paramDictionary != null)
            {
                foreach (var k in paramDictionary.Keys)
                {
                    var param = command.CreateParameter();
                    param.ParameterName = k;
                    param.Value = paramDictionary[k];
                    command.Parameters.Add(param);
                }
            }
            return command;
        }
    }
}
