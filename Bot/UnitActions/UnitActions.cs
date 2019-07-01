using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SC2APIProtocol;

namespace Bot.UnitActions
{
    // --------------------------------------------------------------------------------
    /// <summary>
    /// Object to control a units actions.
    /// </summary>
    // --------------------------------------------------------------------------------
    class UnitActions : IConvertible
    {
        protected ControllerDefault controller;
        protected static Random random = new Random();

        protected uint unitType = 0;

        protected double workerHelpDistance = 12.0;

        public enum ResearchResult { Success, NotUnitType, AlreadyHas, IsResearching, CanNotAfford, UnitBusy, NoGasGysersStructures, CanNotResearch };

        public UnitActions(ControllerDefault controller)
        {
            this.controller = controller ?? throw new ArgumentNullException(nameof(controller));
        }


        // ********************************************************************************
        /// <summary>
        /// Checks to see if the passed unit is of the unit type that is action controller will deal with.
        /// </summary>
        /// <param name="unit">The unit to check.</param>
        /// <returns>True if the unit is the set unit type.</returns>
        // ********************************************************************************
        public bool IsUnitType(Unit unit)
        {
            var isUnitType = false;

            if (unit.unitType == unitType)
            {
                isUnitType = true;
            }

            return isUnitType;
        }

        // ********************************************************************************
        /// <summary>
        /// Add this unit action to the passed unit actions list.
        /// </summary>
        /// <param name="unitActionsList">The actions list to add this object to.</param>
        // ********************************************************************************
        public virtual void SetupUnitActionsList(ref UnitActionsList unitActionsList)
        {
            unitActionsList.addUnitAction(this, unitType);
        }

        // ********************************************************************************
        /// <summary>
        /// Check and see if the unit is busy.
        /// </summary>
        /// <param name="unit">The unit to check.</param>
        /// <returns>True if the unit is busy.</returns>
        // ********************************************************************************
        public virtual bool IsBusy(Unit unit)
        {
            if (unit.buildProgress != 1) return true;

            if (unit.order.AbilityId != 0) return true;

            return false;
        }

        // ********************************************************************************
        /// <summary>
        /// Try and command actions intelligently.
        /// This is meant to be overridden.
        /// </summary>
        /// <param name="unit">The unit to preform the action.</param>
        /// <param name="saveUnit">Setting to return a value to save resources for a unit.</param>
        /// <param name="saveUpgrade">Setting to return a value to save resources for an upgrade.</param>
        /// <param name="ignoreSaveRandomRoll"> Setting to return to turn off the random chance when deciding to save resources.</param>
        /// <param name="saveFor">If set true it will setup the save resource information.</param>
        /// <param name="doNotUseResources">If set true it will not run any action that requires resources.</param>
        // ********************************************************************************
        public virtual void PreformIntelligentActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, ref bool ignoreSaveRandomRoll, bool saveFor = false,
            bool doNotUseResources = false)
        {

        }

        // ********************************************************************************
        /// <summary>
        /// Command random actions.
        /// This is meant to be overridden.
        /// </summary>
        /// <param name="unit">The unit to preform the action.</param>
        /// <param name="saveUnit">Setting to return a value to save resources for a unit.</param>
        /// <param name="saveUpgrade">Setting to return a value to save resources for an upgrade.</param>
        /// <param name="ignoreSaveRandomRoll"> Setting to return to turn off the random chance when deciding to save resources.</param>
        /// <param name="saveFor">If set true it will setup the save resource information.</param>
        /// <param name="doNotUseResources">If set true it will not run any action that requires resources.</param>
        // ********************************************************************************
        public virtual void PreformRandomActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, ref bool ignoreSaveRandomRoll, bool saveFor = false,
            bool doNotUseResources = false)
        {

        }

        // ********************************************************************************
        /// <summary>
        /// Summon army help to the unit.
        /// </summary>
        /// <param name="unit">The unit to summon help to.</param>
        /// <param name="attacker">The attacker to attack.  If null the army comes to the units position.</param>
        /// <param name="idleArmyOnly">If true only idle army units come to help.</param>
        /// <param name="includeNearByWorkers">If true workers with 12.0 distance will come to help also.</param>
        // ********************************************************************************
        public void SummonHelp(Unit unit, Unit attacker = null, bool idleArmyOnly = true, bool includeNearByWorkers = false)
        {
            var army = controller.GetUnits(Units.ArmyUnits);

            if (idleArmyOnly)
            {
                army = controller.GetIdleUnits(army);
            }

            var targetPos = unit.position;

            if (attacker != null)
            {
                targetPos = attacker.position;
            }

            controller.Attack(army, targetPos);

            if (includeNearByWorkers)
            {
                var workers = controller.GetUnits(Units.Workers);

                List<Unit> attackWorkers = new List<Unit>();

                foreach (var worker in workers)
                {
                    if (worker.GetDistance(unit) <= workerHelpDistance)
                    {
                        attackWorkers.Add(worker);
                    }
                }

                controller.Attack(attackWorkers, targetPos);
            }
        }

        // ********************************************************************************
        /// <summary>
        /// Summons help from idle army if under attack. <para/>
        /// It is really actually just reacting to enemy units in sight, not actually being attacked.
        /// </summary>
        /// <param name="unit">The unit that will need help.</param>
        /// <param name="summonHelp">If true it will summon help.</param>
        /// <returns>true if under attack.</returns>
        // ********************************************************************************
        public virtual bool NeedHelpAction(Unit unit, bool summonHelp = true)
        {
            var enemyAttackers = controller.GetPotentialAttackers(unit);
            var underAttack = false;

            if (enemyAttackers.Count > 0)
            {
                if (summonHelp)
                {
                    SummonHelp(unit, enemyAttackers[0]);
                    controller.LogIfSelectedUnit(unit, "{0} is under attack by {1} and summons help.", unit.name, enemyAttackers[0].name);
                }

                underAttack = true;
            }

            return underAttack;
        }

        // ********************************************************************************
        /// <summary>
        /// Research the passed ability if possible.
        /// </summary>
        /// <param name="unit">The unit doing the research.</param>
        /// <param name="researchID">The research ID.</param>
        /// <param name="upgradeID">The ID to look up to see if it is done already.</param>
        /// <returns>A ReserachResult.</returns>
        // ********************************************************************************
        protected virtual ResearchResult ResearchAbility(Unit unit, int researchID, int upgradeID, bool needVespene = true)
        {
            if (!IsUnitType(unit)) return ResearchResult.NotUnitType;

            if (controller.HasUpgrade(upgradeID)) return ResearchResult.AlreadyHas;

            if (controller.IsResearchingUpgrade(researchID, unitType)) return ResearchResult.IsResearching;

            if (needVespene && controller.GetTotalCount(Units.GasGeysersStructures) == 0) return ResearchResult.NoGasGysersStructures;

            if (!controller.CanAffordUpgrade(researchID)) return ResearchResult.CanNotAfford;

            if (IsBusy(unit)) return ResearchResult.UnitBusy;

            unit.Research(researchID);

            return ResearchResult.Success;
        }


        TypeCode IConvertible.GetTypeCode()
        {
            return TypeCode.Object;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            // This is not the best way to do this but it is working because 
            // it is being used on subclasses of UnitActions.
            return this;
        }
    }
}
