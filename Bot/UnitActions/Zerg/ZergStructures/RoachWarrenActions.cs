using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg.ZergStructures
{
    class RoachWarrenActions : ZergStructureActions
    {
        protected int researchGlialReconstitution = Abilities.RESEARCH_GLIAL_RECONSITUTION;
        protected int glialReconstitutionUpgrade = Abilities.GLIAL_RECONSITUTION;

        protected int researchTunningClaws = Abilities.RESEARCH_TUNNELING_CLAWS;
        protected int tunnelingClawsUpgrade = Abilities.TUNNELING_CLAWS;

        public RoachWarrenActions(ZergController controller) : base(controller)
        {
            unitType = Units.ROACH_WARREN;
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
                var result = ResearchGlialReconstitution(unit);

                if (saveFor && result == ResearchResult.CanNotAfford)
                {
                    saveUpgrade = researchGlialReconstitution;
                }
                else if (result == ResearchResult.AlreadyHas
                  || result == ResearchResult.IsResearching)
                {
                    result = ResearchTunnelingClaws(unit);

                    if (saveFor && result == ResearchResult.CanNotAfford)
                    {
                        saveUpgrade = researchTunningClaws;
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
            var randomAction = Random.Next(2);

            switch (randomAction)
            {
                case 0:
                    if (doNotUseResources) return;

                    var glialReconstitutionResult = ResearchGlialReconstitution(unit);
                    if (saveFor && glialReconstitutionResult == ResearchResult.CanNotAfford)
                    {
                        saveUpgrade = researchGlialReconstitution;
                    }
                    break;
                case 1:
                    if (doNotUseResources) return;

                    var tunnelingClawsResult = ResearchTunnelingClaws(unit);
                    if (saveFor && tunnelingClawsResult == ResearchResult.CanNotAfford)
                    {
                        saveUpgrade = researchTunningClaws;
                    }
                    break;
            }
        }

        // ********************************************************************************
        /// <summary>
        /// Will research the Glial Reconstitution upgrade.
        /// </summary>
        /// <param name="unit">The unit doing the research.</param>
        /// <returns>The research result.</returns>
        // ********************************************************************************
        public ResearchResult ResearchGlialReconstitution(Unit unit)
        {
            var result = ResearchAbility(unit, researchGlialReconstitution, glialReconstitutionUpgrade);

            return result;
        }

        // ********************************************************************************
        /// <summary>
        /// Will research the tunneling claws upgrade.
        /// </summary>
        /// <param name="unit">The unit doing the research.</param>
        /// <returns>The research result.</returns>
        // ********************************************************************************
        public ResearchResult ResearchTunnelingClaws(Unit unit)
        {
            var result = ResearchAbility(unit, researchTunningClaws, tunnelingClawsUpgrade);

            return result;
        }
    }
}
