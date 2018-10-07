using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Exico.Shopify.Data.Framework
{
    public interface IDbService<T> : IDisposable
        where T : class
    {
        #region Mixed

        IDbRepository<T> GetRepo();
        T Add(T t);
        bool Delete(object key);
        T Update(T t, object key);
        bool ValidateData(T t);

        #endregion

        #region count
        int Count(List<Expression<Func<T, bool>>> whrClauses);
        int Count(Expression<Func<T, bool>> whrClause);
        int Count();
        #endregion

        #region single
        T FindSingleWhere(Expression<Func<T, bool>> whrClause);
        T FindSingleWhere(Expression<Func<T, bool>> whrClause, string includeProperties);

        T FindSingleWhere(List<Expression<Func<T, bool>>> whrClauses);
        T FindSingleWhere(List<Expression<Func<T, bool>>> whrClauses, string includeProperties);

        #endregion

        #region Find Many Where
        List<T> FindManyWhere(Expression<Func<T, bool>> whrClause);
        List<T> FindManyWhere(Expression<Func<T, bool>> whrClause, string includeProperties);
        List<T> FindManyWhere(Expression<Func<T, bool>> whrClause, string includeProperties, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy);
        List<T> FindManyWhere(Expression<Func<T, bool>> whrClause, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy);
        List<T> FindManyWhere(List<Expression<Func<T, bool>>> whrClauses);
        List<T> FindManyWhere(List<Expression<Func<T, bool>>> whrClauses, string includeProperties);
        List<T> FindManyWhere(List<Expression<Func<T, bool>>> whrClauses, string includeProperties, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy);
        List<T> FindManyWhere(List<Expression<Func<T, bool>>> whrClauses, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy);
        #endregion

        #region Find All
        List<T> FindAll();
        List<T> FindAll(string includeProperties);
        List<T> FindAll(string includeProperties, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy);
        List<T> FindAll(Func<IQueryable<T>, IOrderedQueryable<T>> orderBy);
        #endregion

        #region Find Many Where - PAGED
        PagedResult<T> FindManyWherePaged(int page, int itemsPerPage, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, Expression<Func<T, bool>> whrClause);
        PagedResult<T> FindManyWherePaged(int page, int itemsPerPage, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, Expression<Func<T, bool>> whrClause, string includeProperties);
        PagedResult<T> FindManyWherePaged(int page, int itemsPerPage, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, List<Expression<Func<T, bool>>> whrClauses);
        PagedResult<T> FindManyWherePaged(int page, int itemsPerPage, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, List<Expression<Func<T, bool>>> whrClauses, string includeProperties);
        #endregion

        #region Find All - PAGED
        PagedResult<T> FindAllPaged(int page, int itemsPerPage, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, string includeProperties);
        PagedResult<T> FindAllPaged(int page, int itemsPerPage, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy);


        #endregion

    }
}
