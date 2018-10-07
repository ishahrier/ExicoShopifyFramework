using Exico.Shopify.Data.Domain.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Exico.Shopify.Data.Domain.AppModels
{
    public class PlanAppModel
    {
        public PlanAppModel() { }
        public PlanAppModel(Plan dbData)
        {
            if (dbData == null) throw new Exception("PlanDefinition db data is not valid.");

            this.Id = dbData.Id;
            this.Name = dbData.Name;
            this.Price = dbData.Price;
            this.TrialDays = dbData.TrialDays;
            this.IsDev = dbData.IsDev;
            this.IsTest = dbData.IsTest;
            this.DisplayOrder = dbData.DisplayOrder;
            this.Active = dbData.Active;
            this.Description = dbData.Description;
            this.Footer = dbData.Footer;
            this.IsPopular = dbData.IsPopular;

            Definitions = dbData.PlanDefinitions.ToDictionary(x => x.OptionName, k => new PlanDefinitionAppModel(k));
        }

        public Dictionary<string, PlanDefinitionAppModel> Definitions;

        public int Id { get; set; }
        public string Name { get; set; }
        public short TrialDays { get; set; }
        public bool IsTest { get; set; }
        public int DisplayOrder { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string Footer { get; set; }
        public bool Active { get; set; }
        public bool IsDev { get; set; }
        public bool IsPopular { get; set; }






    }
}
