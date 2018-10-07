using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules.XConsole
{
    public interface IXCommand
    {
        Task Run(XConsole xc);
        string GetDescription();
        string GetName();
    }
}
