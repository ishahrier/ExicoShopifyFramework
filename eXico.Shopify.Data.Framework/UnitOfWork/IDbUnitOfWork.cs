using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Exico.Shopify.Data.Framework
{
    public interface IDbUnitOfWork: IDisposable
    {
        DbContext GetContext();
        int Save(); 
        void TestConnection();
        bool Disposed { get;   }
    }
}
