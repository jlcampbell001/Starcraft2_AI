using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg
{
    // --------------------------------------------------------------------------------
    /// <summary>
    /// Object to control Zerg actions.
    /// </summary>
    // --------------------------------------------------------------------------------
    class ZergActions : UnitActions
    {
        protected new ZergController controller;

        protected uint burrowedUnitType = 0;

        // Upgrades
        protected int burrowUpgrade = Abilities.BURROW;

        // Abilities
        protected int burrow = 0;
        protected int unburrow = 0;

        public ZergActions(ZergController controller) : base(controller)
        {
            this.controller = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        // ********************************************************************************
        /// <summary>
        /// Add this object to a unit actions list.
        /// </summary>
        /// <param name="unitActionsList">Unit actions list to add object to.</param>
        // ********************************************************************************
        public override void SetupUnitActionsList(ref UnitActionsList unitActionsList)
        {
            unitActionsList.addUnitAction(this, unitType);

            if (burrowedUnitType != 0)
            {
                unitActionsList.addUnitAction(this, burrowedUnitType);
            }
        }

        // ********************************************************************************
        /// <summary>
        /// Checks to see if the passed unit is of the burrowed unit type that is action controller will deal with.
        /// </summary>
        /// <param name="unit">The burrow unit to check.</param>
        /// <returns>True if the unit is the burrow version type.</returns>
        // ********************************************************************************
        public bool IsBurrowedUnitType(Unit unit)
        {
            var isUnitType = false;

            if (unit.unitType == burrowedUnitType)
            {
                isUnitType = true;
            }

            return isUnitType;
        }

        // ********************************************************************************
        /// <summary>
        /// Used the units burrow command if it has it.
        /// </summary>
        /// <param name="unit">The unit to burrow.</param>
        // ********************************************************************************
        public void Burrow(Unit unit)
        {
            if (burrow == 0) return;

            if (!IsUnitType(unit)) return;

            if (!controller.HasUpgrade(burrowUpgrade)) return;

            unit.UseAbility(burrow);

            controller.LogIfSelectedUnit(unit, "Burrow {0} {1} @ {2} / {3}", unit.name, unit.tag, unit.position.X, unit.position.Y);
        }

        // ********************************************************************************
        /// <summary>
        /// Used the units unburrow command if it has it.
        /// </summary>
        /// <param name="unit">The unit to unborrow.</param>
        // ********************************************************************************
        public void Unburrow(Unit unit)
        {
            if (burrow == 0) return;

            if (!IsBurrowedUnitType(unit)) return;

            if (!controller.HasUpgrade(burrowUpgrade)) return;

            unit.UseAbility(unburrow);

            controller.LogIfSelectedUnit(unit, "Unburrow {0} {1} @ {2} / {3}", unit.name, unit.tag, unit.position.X, unit.position.Y);
        }
    }
}
