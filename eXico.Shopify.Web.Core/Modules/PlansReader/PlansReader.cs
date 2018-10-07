using Exico.Shopify.Data.Domain.AppModels;
using Exico.Shopify.Data.Domain.DBModels;
using Exico.Shopify.Data.Framework;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Exico.Shopify.Web.Core.Modules
{
    /// <summary>
    /// Helps retriving plans and options for a plan. This is registered as a single ton class on start up.
    /// This is the default implementation of the <see cref="Exico.Shopify.Web.Core.Helpers.IPlansReader" />.    
    /// </summary>
    /// <remarks>
    /// Internally, a plan with bigger Id is better plan.
    /// i.e. Plan B with id = 2 is considered better and pricier then Plan A with id = 1
    /// </remarks>
    /// <seealso cref="Exico.Shopify.Web.Core.Helpers.IPlansReader" />
    public class PlansReader : IPlansReader
    {
        /// <summary>
        /// The plan data memory cache key
        /// </summary>
        public const string PLAN_DATA_CACHE_KEY = "PLAN_DATA_CACHE_KEY";
        /// <summary>
        /// The cache for settings expire after 12 hours.
        /// </summary>
        public const int CACHE_EXPIRE_AFTER_HOURS = 12; //TODO config?

        private readonly ILogger<PlansReader> Logger;
        private readonly IMemoryCache MemCache;
        private readonly IDbService<Plan> DBService;
        protected Dictionary<int, PlanAppModel> PlanData { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="PlansReader"/> class.
        /// and loads the plans and definitions from the database
        /// </summary>
        /// <param name="db">The database service.</param>
        /// <param name="logger">The logger.</param>
        public PlansReader(IDbService<Plan> db, IMemoryCache cache,ILogger<PlansReader> logger)
        {
            Logger = logger;
            MemCache = cache;
            this.DBService = db;
            _LoadPlans();
        }

        /// <summary>
        /// Gets the <see cref="PlanAppModel"/> with the specified plan name.
        /// NOTE: If it is not absolutely necessary, then avoid using this indexer.
        /// Use the int indexer <see cref="this[int]"/> instead/>
        /// </summary>
        /// <value>
        /// The <see cref="PlanAppModel"/>.
        /// </value>
        /// <param name="planName">Name of the plan.</param>
        /// <returns>
        /// <c>PlanAppModel </c> if found, null otherwise
        /// </returns>
        public PlanAppModel this[string planName]
        {
            get
            {
                var plan = PlanData.Values.Where(x => x.Name == planName).FirstOrDefault();
                if (plan != null)
                {
                    Logger.LogInformation($"Found plan with name '{planName}'");
                    return plan;
                }
                else
                {
                    Logger.LogWarning($"Could not find plan with name '{planName}'");
                    return null;
                }
            }
        }


        /// <summary>
        /// Everytime it is called, it goes to database, reads in the settings and assigns into the <see cref="PlanData"/>
        /// and then saves it into cache.
        /// </summary>
        public void ReloadFromDBAndUpdateCache( )
        {
            Logger.LogInformation("Realoading plans from db.");
            var _PlanData = DBService.FindAll("PlanDefinitions", x => x.OrderBy(y => y.DisplayOrder))
                     .Where(x => x.Active == true)
                     .ToDictionary(x => x.Id, v => new PlanAppModel(v));
            Logger.LogInformation($"Done loading plans from db.");
            Logger.LogInformation($"Total {_PlanData.Count()} plans found in the db.");
            Logger.LogInformation("Now saving the loaded DB plan data in to memory cache.");
            MemCache.Set(PLAN_DATA_CACHE_KEY, _PlanData, new MemoryCacheEntryOptions()
            {
                Priority = CacheItemPriority.NeverRemove,
                SlidingExpiration = TimeSpan.FromHours(CACHE_EXPIRE_AFTER_HOURS)
            });
            PlanData = (Dictionary<int, PlanAppModel>)MemCache.Get(PLAN_DATA_CACHE_KEY);
            Logger.LogInformation("Done saving plan data to memory cache.");
        }

        /// <summary>
        /// Gets the <see cref="PlanAppModel"/> with the specified plan identifier.
        /// </summary>
        /// <value>
        /// The <see cref="PlanAppModel"/>.
        /// </value>
        /// <param name="planId">The plan identifier.</param>
        /// <returns>
        /// <c>PlanAppModel </c> if found, null otherwise
        /// </returns>
        public PlanAppModel this[int planId]
        {
            get
            {
                if (PlanData.ContainsKey(planId))
                {
                    Logger.LogInformation($"Found plan with id '{planId}'");
                    return PlanData[planId];
                }
                else
                {
                    Logger.LogWarning($"Could not find plan with id '{planId}'");
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="PlanDefinitionAppModel"/> with the specified plan identifier.
        /// </summary>
        /// <value>
        /// The <see cref="PlanDefinitionAppModel"/>.
        /// </value>
        /// <param name="planId">The plan identifier.</param>
        /// <param name="optionName">Name of the option.</param>
        /// <returns>
        /// <c>PlanDefinitionAppModel</c> if found , otherwise null
        /// </returns>
        public PlanDefinitionAppModel this[int planId, string optionName]
        {
            get
            {
                var plan = this[planId];
                return _GetDefinition(plan, optionName);
            }
        }

        /// <summary>
        /// Gets all plans.
        /// </summary>
        /// <param name="includeDev">if set to <c>true</c> then include the plans that are meant to be seen in dev environment as well.</param>
        /// <returns>
        /// List of plans as <c>List<PlanAppModel></c>
        /// </returns>
        public List<PlanAppModel> GetAllPlans(bool includeDev = true)
        {
            var list = PlanData.Values.ToList();
            list = includeDev ? list : list.Where(x => x.IsDev == false).ToList();
            Logger.LogInformation($"Found total '{list.Count()}' plans { (includeDev ? "including" : "excluding")} dev plans.");
            return list;
        }

        /// <summary>
        /// Gets the available upgrades available to current plan.
        /// </summary>
        /// <param name="currentPlanId">The current plan identifier.</param>
        /// <param name="includeDev">If set to <c>true</c> then include the plans that are meant to be seen in dev environment as well while determining upgradability.</param>
        /// <returns>
        /// List of available updagrades, <c>List<PlanAppModel></c>
        /// </returns>
        public List<PlanAppModel> GetAvailableUpgrades(int currentPlanId, bool includeDev = true)
        {
            var list = GetAllPlans(includeDev);
            list = list.Where(x => x.Id > currentPlanId).ToList();
            Logger.LogInformation($"Found '{list.Count()}' upgradable plans { (includeDev ? "including" : "excluding")} dev plans.");
            return list;
        }

        /// <summary>
        /// Determines whether any upgradable plans are available for the specified current plan identifier.
        /// </summary>
        /// <param name="currentPlanId">The current plan identifier.</param>
        /// <param name="includeDev">If set to <c>true</c> then include the plans that are meant to be seen in dev environment as well while determining upgradability.</param>
        /// <returns>
        ///   <c>true</c> if current plan can be upgraded to available upgradable plans, <c>false</c> toherwise
        /// </returns>
        public bool CanUpgrade(int currentPlanId, bool includeDev = true)
        {
            var list = GetAvailableUpgrades(currentPlanId, includeDev);
            var ret = list.Count() > 0;
            Logger.LogInformation($"Upgrade is possible.");
            return ret;
        }

        #region Helpers        
        /// <summary>
        /// Checks if cache has the plan data dictionary already, if not then  
        /// calls the <see cref="ReloadFromDBAndUpdateCache()"/> method.
        /// </summary>
        private void _LoadPlans()
        {
            Logger.LogInformation("Starting loading plan data.Checking cache first.");
            PlanData = (Dictionary<int, PlanAppModel>)MemCache.Get(PLAN_DATA_CACHE_KEY);
            if (PlanData==null)
            {
                Logger.LogInformation("Plan data memory cache is empty. Reloading from DB instead.");
                ReloadFromDBAndUpdateCache(); 
            }
            else
            {
                Logger.LogInformation("Skipping Reloading from db.Cache has a copy of plan data.");
                Logger.LogInformation($"Total {PlanData.Count} plans found in the cache. ");
            }
            Logger.LogInformation("Done loading plan data.");
        }
        private PlanDefinitionAppModel _GetDefinition(PlanAppModel plan, string optionName)
        {
            PlanDefinitionAppModel ret = null;
            if (plan != null)
            {
                if (plan.Definitions.ContainsKey(optionName))
                {
                    Logger.LogInformation($"Found option '{optionName}' in plan '{plan.Name}'");
                    ret = plan.Definitions[optionName];
                }
                else Logger.LogInformation($"Could not find option '{optionName}' in plan '{plan.Name}'");
            }

            return ret;
        }
        #endregion
    }
}
