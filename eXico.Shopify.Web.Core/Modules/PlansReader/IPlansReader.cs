using System.Collections.Generic;
using Exico.Shopify.Data.Domain.AppModels;
using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;

namespace Exico.Shopify.Web.Core.Modules
{
    /// <summary>
    /// Interface for implementing Plan and Plan Definitions reading from the database strorage.
    /// The default implementation of this is <see cref="PlansReader"/> and it is registered on startup
    /// as Singleton service.
    /// </summary>
    public interface IPlansReader
    {
        PlanAppModel this[int planId] { get; }
        PlanAppModel this[string planName] { get; }
        PlanDefinitionAppModel this[int planId, string optionName] { get; }

        bool CanUpgrade(int currentPlanId, bool includeDev = true);
        List<PlanAppModel> GetAllPlans(bool includeDev = true);
        List<PlanAppModel> GetAvailableUpgrades(int currentPlanId, bool includeDev = true);
        void ReloadFromDBAndUpdateCache( );
    }
}