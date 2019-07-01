using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg.ZergStructures
{
    class SpawningPoolActions : ZergStructureActions
    {
        protected int researchAdrenalGlands = Abilities.RESEARCH_ADRENAL_GLANDS;
        protected int adrenalGlandsUpgrade = Abilities.ADRENAL_GLANDS;

        protected int researchMetabolicBoost = Abilities.RESEARCH_METABOLIC_BOOST;
        protected int metabolicBoostUpgrade = Abilities.METABOLIC_BOOST;

        public SpawningPoolActions(ZergController controller) : base(controller)
        {
            unitType = Units.SPAWNING_POOL;
        }

        // ********************************************************************************
        /// <summary>
        /// Preform an Intelligent actions for the unit.
        /// </summary>
        /// <param name="unit">The spawning pool unit.</param>
        /// <param name="saveUnit">Setting to return a value to save resources for a unit.</param>
        /// <param name="saveUpgrade">Setting to return a value to save resources for an upgrade.</param>
        /// <param name="ignoreSaveRandomRoll"> Setting to return to turn off the random chance when deciding to save resources.</param>
        /// <param name="saveFor">If set true it will setup the save resource information.</param>
        /// <param name="doNotUseResources">If set true it will not run any action that requires resources.</param>
        // ********************************************************************************
        public override void PreformIntelligentActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, ref bool ignoreSaveRandomRoll, bool saveFor = false, bool doNotUseResources = false)
        {
            base.PreformIntelligentActions(unit, ref saveUnit, ref saveUpgrade, ref ignoreSaveRandomRoll, saveFor, doNotUseResources);

            if (!doNotUseResources)
            {
                var result = ResearchMetabolicBoost(unit);

                if (saveFor && result == ResearchResult.CanNotAfford)
                {
                    saveUpgrade = researchMetabolicBoost;
                }
                else if (result == ResearchResult.AlreadyHas
                  || result == ResearchResult.IsResearching)
                {
                    result = ResearchAdrenalGlands(unit);

                    if (saveFor && result == ResearchResult.CanNotAfford)
                    {
                        saveUpgrade = researchAdrenalGlands;
                    }
                }
            }
        }

        // ********************************************************************************
        /// <summary>
        /// Preform a random action for the passed unit.
        /// </summary>
        /// <param name="unit">The spawning pool unit.</param>
        /// <param name="saveUnit">Setting to return a value to save resources for a unit.</param>
        /// <param name="saveUpgrade">Setting to return a value to save resources for an upgrade.</param>
        /// <param name="ignoreSaveRandomRoll"> Setting to return to turn off the random chance when deciding to save resources.</param>
        /// <param name="saveFor">If set true it will setup the save resource information.</param>
        /// <param name="doNotUseResources">If set true it will not run any action that requires resources.</param>
        // ********************************************************************************
        public override void PreformRandomActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, ref bool ignoreSaveRandomRoll, bool saveFor = false, bool doNotUseResources = false)
        {
            base.PreformRandomActions(unit, ref saveUnit, ref saveUpgrade, ref ignoreSaveRandomRoll, saveFor, doNotUseResources);

            var randomAction = random.Next(2);

            switch (randomAction)
            {
                case 0:
                    if (doNotUseResources) return;

                    var adrenalGlandsResult = ResearchAdrenalGlands(unit);
                    if (saveFor && adrenalGlandsResult == ResearchResult.CanNotAfford)
                    {
                        saveUpgrade = researchAdrenalGlands;
                    }
                    break;
                case 1:
                    if (doNotUseResources) return;

                    var metabolicBoostResult = ResearchMetabolicBoost(unit);
                    if (saveFor && metabolicBoostResult == ResearchResult.CanNotAfford)
                    {
                        saveUpgrade = researchMetabolicBoost;
                    }
                    break;
            }
        }

        // ********************************************************************************
        /// <summary>
        /// Research adrenal glands.
        /// </summary>
        /// <param name="unit">The spawning unit.</param>
        /// <returns>The ResearchResult.</returns>
        // ********************************************************************************
        public ResearchResult ResearchAdrenalGlands(Unit unit)
        {
            if (!controller.HasUnits(Units.HIVE)) return ResearchResult.CanNotResearch;

            var result = ResearchAbility(unit, researchAdrenalGlands, adrenalGlandsUpgrade);

            return result;
        }

        // ********************************************************************************
        /// <summary>
        /// Research metabolic boost.
        /// </summary>
        /// <param name="unit">The spawning unit.</param>
        /// <returns>The ResearchResult.</returns>
        // ********************************************************************************
        public ResearchResult ResearchMetabolicBoost(Unit unit)
        {
            var result = ResearchAbility(unit, researchMetabolicBoost, metabolicBoostUpgrade);

            return result;
        }
    }
}
