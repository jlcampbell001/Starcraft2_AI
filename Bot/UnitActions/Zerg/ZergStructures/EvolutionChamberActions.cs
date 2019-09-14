using System;

namespace Bot.UnitActions.Zerg.ZergStructures
{
    class EvolutionChamberActions : ZergStructureActions
    {
        protected int researchMeleeAttack1 = Abilities.RESEARCH_MELEE_ATTACK1_ZERG;
        protected int researchMeleeAttack2 = Abilities.RESEARCH_MELEE_ATTACK2_ZERG;
        protected int researchMeleeAttack3 = Abilities.RESEARCH_MELEE_ATTACK3_ZERG;
        protected int meleeAttack1Upgrade = Abilities.MELEE_ATTACK1_ZERG;
        protected int meleeAttack2Upgrade = Abilities.MELEE_ATTACK2_ZERG;
        protected int meleeAttack3Upgrade = Abilities.MELEE_ATTACK3_ZERG;

        protected int researchMissileAttack1 = Abilities.RESEARCH_MISSILE_ATTACK1_ZERG;
        protected int researchMissileAttack2 = Abilities.RESEARCH_MISSILE_ATTACK2_ZERG;
        protected int researchMissileAttack3 = Abilities.RESEARCH_MISSILE_ATTACK3_ZERG;
        protected int missileAttack1Upgrade = Abilities.MISSILE_ATTACK1_ZERG;
        protected int missileAttack2Upgrade = Abilities.MISSILE_ATTACK2_ZERG;
        protected int missileAttack3Upgrade = Abilities.MISSILE_ATTACK3_ZERG;

        protected int researchGoundArmor1 = Abilities.RESEARCH_GROUND_ARMOR1_ZERG;
        protected int researchGoundArmor2 = Abilities.RESEARCH_GROUND_ARMOR2_ZERG;
        protected int researchGoundArmor3 = Abilities.RESEARCH_GROUND_ARMOR3_ZERG;
        protected int groundArmor1Upgrade = Abilities.GROUND_ARMOR1_ZERG;
        protected int groundArmor2Upgrade = Abilities.GROUND_ARMOR2_ZERG;
        protected int groundArmor3Upgrade = Abilities.GROUND_ARMOR3_ZERG;

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

            if (!doNotUseResources)
            {
                var level = 1;
                var result = ResearchGroundArmor(unit, ref level);

                if (saveFor && result == ResearchResult.CanNotAfford)
                {
                    switch (level)
                    {
                        case 1:
                            saveUpgrade = researchGoundArmor1;
                            break;
                        case 2:
                            saveUpgrade = researchGoundArmor2;
                            break;
                        case 3:
                            saveUpgrade = researchGoundArmor3;
                            break;
                    }
                }
                else if (result == ResearchResult.AlreadyHas
                  || result == ResearchResult.IsResearching
                  || result == ResearchResult.CanNotResearch)
                {
                    level = 1;
                    result = ResearchMeleeAttack(unit, ref level);

                    if (saveFor && result == ResearchResult.CanNotAfford)
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
                    else if (result == ResearchResult.AlreadyHas
                      || result == ResearchResult.IsResearching
                      || result == ResearchResult.CanNotResearch)
                    {
                        level = 1;
                        result = ResearchMissileAttack(unit, ref level);
                        if (saveFor && result == ResearchResult.CanNotAfford)
                        {
                            switch (level)
                            {
                                case 1:
                                    saveUpgrade = researchMissileAttack1;
                                    break;
                                case 2:
                                    saveUpgrade = researchMissileAttack2;
                                    break;
                                case 3:
                                    saveUpgrade = researchMissileAttack3;
                                    break;
                            }
                        }
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

            var randomAction = Random.Next(3);
            var level = 1;

            switch (randomAction)
            {
                case 0:
                    if (doNotUseResources) return;

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
                case 1:
                    if (doNotUseResources) return;

                    var missileAttackResult = ResearchMissileAttack(unit, ref level);
                    if (saveFor && missileAttackResult == ResearchResult.CanNotAfford)
                    {
                        switch (level)
                        {
                            case 1:
                                saveUpgrade = researchMissileAttack1;
                                break;
                            case 2:
                                saveUpgrade = researchMissileAttack2;
                                break;
                            case 3:
                                saveUpgrade = researchMissileAttack3;
                                break;
                        }
                    }
                    break;
                case 2:
                    if (doNotUseResources) return;

                    var groundArmorResult = ResearchGroundArmor(unit, ref level);
                    if (saveFor && groundArmorResult == ResearchResult.CanNotAfford)
                    {
                        switch (level)
                        {
                            case 1:
                                saveUpgrade = researchGoundArmor1;
                                break;
                            case 2:
                                saveUpgrade = researchGoundArmor2;
                                break;
                            case 3:
                                saveUpgrade = researchGoundArmor3;
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
            var upgrade = meleeAttack1Upgrade;
            level = 1;

            if (controller.HasUpgrade(meleeAttack1Upgrade))
            {
                if (controller.HasUpgrade(meleeAttack2Upgrade))
                {
                    research = researchMeleeAttack3;
                    upgrade = meleeAttack3Upgrade;
                    level = 3;
                }
                else
                {
                    research = researchMeleeAttack2;
                    upgrade = meleeAttack2Upgrade;
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

        // ********************************************************************************
        /// <summary>
        /// Will research the next level of missile attack.
        /// </summary>
        /// <param name="unit">The unit to preform the research.</param>
        /// <param name="level">The current level that needs to be researched.</param>
        /// <returns>The research result.</returns>
        // ********************************************************************************
        public ResearchResult ResearchMissileAttack(Unit unit, ref int level)
        {
            var research = researchMissileAttack1;
            var upgrade = missileAttack1Upgrade;
            level = 1;

            if (controller.HasUpgrade(missileAttack1Upgrade))
            {
                if (controller.HasUpgrade(missileAttack2Upgrade))
                {
                    research = researchMissileAttack3;
                    upgrade = missileAttack3Upgrade;
                    level = 3;
                }
                else
                {
                    research = researchMissileAttack2;
                    upgrade = missileAttack2Upgrade;
                    level = 2;
                }
            }

            var result = ResearchAbility(unit, research, upgrade);

            return result;
        }

        // ********************************************************************************
        /// <summary>
        /// Will research the next level of missile attack.
        /// </summary>
        /// <param name="unit">The unit to preform the research.</param>
        /// <returns>The research result.</returns>
        // ********************************************************************************
        public ResearchResult ResearchMissileAttack(Unit unit)
        {
            var level = 1;
            var result = ResearchMissileAttack(unit, ref level);

            return result;
        }

        // ********************************************************************************
        /// <summary>
        /// Will research the next level of ground armor.
        /// </summary>
        /// <param name="unit">The unit to preform the research.</param>
        /// <param name="level">The current level that needs to be researched.</param>
        /// <returns>The research result.</returns>
        // ********************************************************************************
        public ResearchResult ResearchGroundArmor(Unit unit, ref int level)
        {
            var research = researchGoundArmor1;
            var upgrade = groundArmor1Upgrade;
            level = 1;

            if (controller.HasUpgrade(groundArmor1Upgrade))
            {
                if (controller.HasUpgrade(groundArmor2Upgrade))
                {
                    research = researchGoundArmor3;
                    upgrade = groundArmor3Upgrade;
                    level = 3;
                }
                else
                {
                    research = researchGoundArmor2;
                    upgrade = groundArmor2Upgrade;
                    level = 2;
                }
            }

            var result = ResearchAbility(unit, research, upgrade);

            return result;
        }

        // ********************************************************************************
        /// <summary>
        /// Will research the next level of ground armor.
        /// </summary>
        /// <param name="unit">The unit to preform the research.</param>
        /// <returns>The research result.</returns>
        // ********************************************************************************
        public ResearchResult ResearchGroundArmor(Unit unit)
        {
            var level = 1;
            var result = ResearchGroundArmor(unit, ref level);

            return result;
        }
    }
}
