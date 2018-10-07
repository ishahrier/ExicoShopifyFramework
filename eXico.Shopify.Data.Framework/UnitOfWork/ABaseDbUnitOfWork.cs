using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Exico.Shopify.Data.Framework
{
    public    class ABaseDbUnitOfWork : IDbUnitOfWork
    {
        private ABaseDbUnitOfWork() { }
        private DbContext _Context { get;  set; }         
        public ABaseDbUnitOfWork(DbContext context)
        {
            this._Context = context;
        }

        public bool Disposed { get; private set; }
        protected virtual void Dispose(bool disposing)
        {
            if (!this.Disposed)
            {
                if (disposing)
                {
                    _Context.Dispose();
                }
            }
            this.Disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        public int Save()
        {
            return this._Context.SaveChanges();
        }
 

        public void TestConnection()
        {            
            using (var db = (DbContext)Activator.CreateInstance(_Context.GetType()))
            {
                    db.Database.OpenConnection();
                    db.Database.CloseConnection();
            }
        }

        DbContext IDbUnitOfWork.GetContext()
        {
            return _Context;
        }

        
    }
}
