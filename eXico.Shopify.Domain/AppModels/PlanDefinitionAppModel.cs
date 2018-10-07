using Exico.Shopify.Data.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Exico.Shopify.Data.Domain.AppModels
{
    public class PlanDefinitionAppModel
    {
        public PlanDefinitionAppModel(PlanDefinition dbData)
        {
            if (dbData == null) throw new Exception("PlanDefinition db data is not valid.");
            this.Id = dbData.Id;
            this.OptionName = dbData.OptionName;
            this.OptionValue = dbData.OptionValue;
            this.Description = dbData.Description;
        }
        public int Id { get; set; }
        public string OptionName { get; set; }
        public string OptionValue { get; set; }
        public string Description { get; set; }
    }
}
