using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules
{
    public interface IRenderViewAsString
    {
        Task<string> RenderToStringAsync(string viewName, object model);
    }
}
