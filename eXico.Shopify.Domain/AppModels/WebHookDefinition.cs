using System;
using System.Collections.Generic;
using System.Text;

namespace Exico.Shopify.Data.Domain.AppModels
{
    public class WebHookDefinition
    {
        public string Topic { get; set; } = string.Empty;
        public string Callback { get; set; } = string.Empty;
    }
}
