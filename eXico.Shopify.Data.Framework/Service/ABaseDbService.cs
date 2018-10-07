using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Exico.Shopify.Data.Framework
{

    public   class ABaseDbService<T> : IDbService<T> where T : class
    {        
        private  IDbRepository<T> Repo { get; set; }
        private ABaseDbService() { }
        public ABaseDbService(IDbRepository<T> repo)
        {
            this.Repo = repo;
        }

        #region Mixed
        public IDbRepository<T> GetRepo()
        {
            // return this.ServiceProvider.GetService<IRepository<T>>();
            return Repo;
        }

        public T Add(T t)
        {
            if (ValidateData(t))
            {
                var r = GetRepo();
                var item = r.Add(t);
                if (r.Save() > 0)
                {
                    return item;
                }
                else
                {
                    return null;
                }

            }
            else
            {
                //throw new ValidationException(null, "error");
                throw new Exception("Model Not Valid");

            }

        }

        public bool Delete(object key)
        {
            var r = GetRepo();
            r.Delete(key);
            return r.Save() > 0 ? true : false;

        }

        public T Update(T t, object key)
        {
            if (ValidateData(t))
            {
                var r = GetRepo();
                var item = r.Update(t, key);
                if (item == null) return null;
                else r.Save();//we dont check if r.Save()>0 cause if nothing is changed then it returns 0
                return item;
            }
            else
            {
                return null;
            }
        }

        public bool ValidateData(T t)
        {
            // IValidator obj = t as IValidator;
            // return obj == null ? true : obj.IsValid();
            return true;
        }

        #endregion  

        #region count

        private int _Count(List<Expression<Func<T, bool>>> whrClauses = null)
        {
            var r = GetRepo();
            return r.Count(whrClauses);
        }

        public int Count(List<Expression<Func<T, bool>>> whrClauses)
        {
            return _Count(whrClauses);
        }

        public int Count(Expression<Func<T, bool>> whrClause)
        {
            return _Count(_ToListOfWheres(whrClause));
        }

        public int Count()
        {
            return _Count(null);
        }
        #endregion

        #region single
        private T _FindSingleWhere(List<Expression<Func<T, bool>>> whrClauses, string includeProperties)
        {
            var r = GetRepo();
            var item = r.GetSingle(whrClauses, includeProperties);
            return item;

        }

        public T FindSingleWhere(Expression<Func<T, bool>> whrClause)
        {
            return _FindSingleWhere(_ToListOfWheres(whrClause), "");
        }

        public T FindSingleWhere(Expression<Func<T, bool>> whrClause, string includeProperties)
        {
            return _FindSingleWhere(_ToListOfWheres(whrClause), includeProperties);
        }

        public T FindSingleWhere(List<Expression<Func<T, bool>>> whrClauses)
        {
            return _FindSingleWhere(whrClauses, "");
        }

        public T FindSingleWhere(List<Expression<Func<T, bool>>> whrClauses, string includeProperties)
        {
            return _FindSingleWhere(whrClauses, includeProperties);
        }
        #endregion

        #region Find many where
        private List<T> _FindManyWhere(List<Expression<Func<T, bool>>> whrClauses, string includeProperties, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy)
        {
            var r = GetRepo();
            var items = r.GetMany(whrClauses, includeProperties, orderBy).ToList();
            return items;
        }

        public List<T> FindManyWhere(Expression<Func<T, bool>> whrClause)
        {
            return _FindManyWhere(_ToListOfWheres(whrClause), "", null);
        }

        public List<T> FindManyWhere(Expression<Func<T, bool>> whrClause, string includeProperties)
        {
            return _FindManyWhere(_ToListOfWheres(whrClause), includeProperties, null);
        }

        public List<T> FindManyWhere(Expression<Func<T, bool>> whrClause, string includeProperties, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy)
        {
            return _FindManyWhere(_ToListOfWheres(whrClause), includeProperties, orderBy);
        }

        public List<T> FindManyWhere(Expression<Func<T, bool>> whrClause, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy)
        {
            return _FindManyWhere(_ToListOfWheres(whrClause), "", orderBy);
        }

        public List<T> FindManyWhere(List<Expression<Func<T, bool>>> whrClauses)
        {
            return _FindManyWhere(whrClauses, "", null);
        }

        public List<T> FindManyWhere(List<Expression<Func<T, bool>>> whrClauses, string includeProperties)
        {
            return _FindManyWhere(whrClauses, includeProperties, null);
        }

        public List<T> FindManyWhere(List<Expression<Func<T, bool>>> whrClauses, string includeProperties, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy)
        {
            return _FindManyWhere(whrClauses, includeProperties, orderBy);
        }

        public List<T> FindManyWhere(List<Expression<Func<T, bool>>> whrClauses, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy)
        {
            return _FindManyWhere(whrClauses, "", orderBy);
        }

        #endregion

        #region Find All

        private List<T> _FindAll(string includeProperties, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy)
        {

            var r = GetRepo();
            var items = r.GetMany(null, includeProperties, orderBy).ToList();
            return items;

        }

        public List<T> FindAll()
        {
            return _FindAll("", null);
        }

        public List<T> FindAll(string includeProperties, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy)
        {
            return _FindAll(includeProperties, orderBy);
        }

        public List<T> FindAll(Func<IQueryable<T>, IOrderedQueryable<T>> orderBy)
        {
            return _FindAll("", orderBy);
        }

        public List<T> FindAll(string includeProperties)
        {
            return _FindAll(includeProperties, null);
        }
        #endregion      

        #region Find Many Where - PAGED
        private PagedResult<T> _FindManyWherePaged(int page, int itemsPerPage, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, List<Expression<Func<T, bool>>> whrClauses, string includeProperties)
        {

            if (orderBy == null)
            {
                throw new Exception("Paging Cannot Be Done Without OrderBy Clause");
            }
            if (page <= 0)
            {
                page = 1; //lets correct it rather than throwing error
            }
            if (itemsPerPage <= 0)
            {
                itemsPerPage = PagedResult<T>.DEFAULT_ITEMS_PER_PAGE;//lets correct it rather than throwing error
            }

            var r = GetRepo();
            var items = r.GetMany(whrClauses, includeProperties, orderBy).Skip((page - 1) * itemsPerPage).Take(itemsPerPage).ToList();
            var count = r.Count(whrClauses);
            return new PagedResult<T>(items, count, page, itemsPerPage);
        }


        public PagedResult<T> FindManyWherePaged(int page, int itemsPerPage, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, Expression<Func<T, bool>> whrClause, string includeProperties)
        {
            return _FindManyWherePaged(page, itemsPerPage, orderBy, _ToListOfWheres(whrClause), includeProperties);
        }

        public PagedResult<T> FindManyWherePaged(int page, int itemsPerPage, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, Expression<Func<T, bool>> whrClause)
        {
            return _FindManyWherePaged(page, itemsPerPage, orderBy, _ToListOfWheres(whrClause), "");
        }

        public PagedResult<T> FindManyWherePaged(int page, int itemsPerPage, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, List<Expression<Func<T, bool>>> whrClauses, string includeProperties)
        {
            return _FindManyWherePaged(page, itemsPerPage, orderBy, whrClauses, includeProperties);
        }

        public PagedResult<T> FindManyWherePaged(int page, int itemsPerPage, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, List<Expression<Func<T, bool>>> whrClauses)
        {
            return _FindManyWherePaged(page, itemsPerPage, orderBy, whrClauses, "");
        }
        #endregion

        #region Find All - PAGED
        public PagedResult<T> _FindAllPaged(int page, int itemsPerPage, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, string includeProperties)
        {
            if (orderBy == null)
            {
                throw new Exception("Paging Cannot Be Done Without OrderBy Clause");
            }
            if (page <= 0)
            {
                page = 1; //lets correct it rather than throwing error
            }
            if (itemsPerPage <= 0)
            {
                itemsPerPage = PagedResult<T>.DEFAULT_ITEMS_PER_PAGE;//lets correct it rather than throwing error
            }

            var r = GetRepo();
            var items = r.GetMany(null, includeProperties, orderBy).Skip((page - 1) * itemsPerPage).Take(itemsPerPage).ToList();
            var count = r.Count();
            return new PagedResult<T>(items, count, page, itemsPerPage);
        }

        public PagedResult<T> FindAllPaged(int page, int itemsPerPage, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy)
        {
            return _FindAllPaged(page, itemsPerPage, orderBy, "");
        }

        public PagedResult<T> FindAllPaged(int page, int itemsPerPage, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, string includeProperties)
        {
            return _FindAllPaged(page, itemsPerPage, orderBy, includeProperties);
        }
        #endregion

        #region helpers
        private List<Expression<Func<T, bool>>> _ToListOfWheres(Expression<Func<T, bool>> whrClause)
        {
            List<Expression<Func<T, bool>>> list = new List<Expression<Func<T, bool>>>();
            list.Add(whrClause);
            return list;
        }

        public void Dispose()
        {
            Repo.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion

    }
}
