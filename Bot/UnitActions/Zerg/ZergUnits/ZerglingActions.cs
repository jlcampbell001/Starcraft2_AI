using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg.ZergUnits
{
    class ZerglingActions : ZergActions
    {
        protected uint baneling = Units.BANELING;

        private int banelingPerZergling = 10;
        private int burrowUnburrowChance = 60;

        public int BanelingPerZergling { get => banelingPerZergling; set => banelingPerZergling = value; }
        public int BurrowUnburrowChance { get => burrowUnburrowChance; set => burrowUnburrowChance = value; }

        public enum BanelingResult { Success, NotUnitType, UnitBusy, CanNotConstruct, CanNotAfford };

        public ZerglingActions(ZergController controller) : base(controller)
        {
            unitType = Units.ZERGLING;
            burrowedUnitType = Units.ZERGLING_BURROWED;

            burrow = Abilities.BURROW_ZERGLING;
            unburrow = Abilities.UNBURROW_ZERGLING;
        }

        // ********************************************************************************
        /// <summary>
        /// Preform an Intelligent actions for the unit.
        /// </summary>
        /// <param name="unit">The zergling unit.</param>
        /// <param name="saveUnit">Setting to return a value to save resources for a unit.</param>
        /// <param name="saveUpgrade">Setting to return a value to save resources for an upgrade.</param>
        /// <param name="ignoreSaveRandomRoll"> Setting to return to turn off the random chance when deciding to save resources.</param>
        /// <param name="saveFor">If set true it will setup the save resource information.</param>
        /// <param name="doNotUseResources">If set true it will not run any action that requires resources.</param>
        // ********************************************************************************
        public override void PreformIntelligentActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, ref bool ignoreSaveRandomRoll, bool saveFor = false, bool doNotUseResources = false)
        {
            if (!IsUnitType(unit) && (!IsBurrowedUnitType(unit))) return;

            if (IsUnitType(unit))
            {
                var preformAction = BanelingResult.CanNotConstruct;

                if (controller.GetTotalCount(Units.Banelings) < controller.GetTotalCount(Units.Zerglings) / BanelingPerZergling)
                {
                    preformAction = MorphToBaneling(unit);
                }

                if (preformAction != BanelingResult.Success && Random.Next(100) < BurrowUnburrowChance)
                {
                    Burrow(unit);
                }
            }

            if (IsBurrowedUnitType(unit))
            {
                if (Random.Next(100) < BurrowUnburrowChance || NeedHelpAction(unit, summonHelp: false))
                {
                    Unburrow(unit);
                }
            }
        }

        // ********************************************************************************
        /// <summary>
        /// Preform a random action for the passed unit.
        /// </summary>
        /// <param name="unit">The zergling unit.</param>
        /// <param name="saveUnit">Setting to return a value to save resources for a unit.</param>
        /// <param name="saveUpgrade">Setting to return a value to save resources for an upgrade.</param>
        /// <param name="ignoreSaveRandomRoll"> Setting to return to turn off the random chance when deciding to save resources.</param>
        /// <param name="saveFor">If set true it will setup the save resource information.</param>
        /// <param name="doNotUseResources">If set true it will not run any action that requires resources.</param>
        // ********************************************************************************
        public override void PreformRandomActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, ref bool ignoreSaveRandomRoll, bool saveFor = false, bool doNotUseResources = false)
        {
            if (!IsUnitType(unit) && (!IsBurrowedUnitType(unit))) return;

            if (IsUnitType(unit))
            {
                var randomAction = Random.Next(2);

                switch (randomAction)
                {
                    case 0:
                        Burrow(unit);
                        break;

                    case 1:
                        MorphToBaneling(unit);
                        break;
                }
            }

            if (IsBurrowedUnitType(unit))
            {
                if (Random.Next(100) < BurrowUnburrowChance || NeedHelpAction(unit, summonHelp: false))
                {
                    Unburrow(unit);
                }
            }
        }

        // ********************************************************************************
        /// <summary>
        /// Morph to an baneling.
        /// </summary>
        /// <param name="unit">The zergling to morph.</param>
        /// <returns>An BanelingResult.</returns>
        // ********************************************************************************
        public BanelingResult MorphToBaneling(Unit unit)
        {
            if (!IsUnitType(unit)) return BanelingResult.NotUnitType;

            if (IsBusy(unit)) return BanelingResult.UnitBusy;

            if (!controller.CanConstruct(baneling, ignoreResourceSupply: true)) return BanelingResult.CanNotConstruct;

            if (!controller.CanAfford(baneling)) return BanelingResult.CanNotAfford;

            unit.Train(baneling);

            controller.LogIfSelectedUnit(unit, "Zergling {2} morphing to baneling @ {0} / {1}.", unit.position.X, unit.position.Y, unit.tag);

            return BanelingResult.Success;
        }
    }
}
