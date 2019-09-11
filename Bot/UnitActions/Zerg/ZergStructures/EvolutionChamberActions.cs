using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg.ZergStructures
{
    class EvolutionChamberActions : ZergStructureActions
    {
        protected int researchMeleeAttack1 = Abilities.RESEARCH_MELEE_ATTACK1;
        protected int researchMeleeAttack2 = Abilities.RESEARCH_MELEE_ATTACK2;
        protected int researchMeleeAttack3 = Abilities.RESEARCH_MELEE_ATTACK3;
        protected int MeleeAttack1Upgrade = Abilities.MELEE_ATTACK1;
        protected int MeleeAttack2Upgrade = Abilities.MELEE_ATTACK2;
        protected int MeleeAttack3Upgrade = Abilities.MELEE_ATTACK3;

        public EvolutionChamberActions(ZergController controller) : base(controller)
        {
            unitType = Units.EVOLUTION_CHAMBER;
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

            var randomAction = random.Next(1);

            switch (randomAction)
            {
                case 0:
                    if (doNotUseResources) return;

                    var level = 1;
                    var meleeAttackResult = ResearchMeleeAttack(unit, ref level);
                    if (saveFor && meleeAttackResult == ResearchResult.CanNotAfford)
                    {
                        switch (level)
                        {
                            case 1:
                                saveUpgrade = researchMeleeAttack1;
                                break;
                            case 2:
                                saveUpgrade = researchMeleeAttack2;
                                break;
                            case 3:
                                saveUpgrade = researchMeleeAttack3;
                                break;
                        }
                    }
                    break;
            }
        }

        // ********************************************************************************
        /// <summary>
        /// Will research the next level of melee attack.
        /// </summary>
        /// <param name="unit">The unit to preform the research.</param>
        /// <param name="level">The current level that needs to be researched.</param>
        /// <returns>The research result.</returns>
        // ********************************************************************************
        public ResearchResult ResearchMeleeAttack(Unit unit, ref int level)
        {
            var research = researchMeleeAttack1;
            var upgrade = MeleeAttack1Upgrade;
            level = 1;

            if (controller.HasUpgrade(MeleeAttack1Upgrade))
            {
                if (controller.HasUpgrade(MeleeAttack2Upgrade))
                {
                    research = researchMeleeAttack3;
                    upgrade = MeleeAttack3Upgrade;
                    level = 3;
                }
                else
                {
                    research = researchMeleeAttack2;
                    upgrade = MeleeAttack2Upgrade;
                    level = 2;
                }
            }

            var result = ResearchAbility(unit, research, upgrade);

            return result;
        }

        // ********************************************************************************
        /// <summary>
        /// Will research the next level of melee attack.
        /// </summary>
        /// <param name="unit">The unit to preform the research.</param>
        /// <returns>The research result.</returns>
        // ********************************************************************************
        public ResearchResult ResearchMeleeAttack(Unit unit)
        {
            var level = 1;
            var result = ResearchMeleeAttack(unit, ref level);

            return result;
        }
    }
}
