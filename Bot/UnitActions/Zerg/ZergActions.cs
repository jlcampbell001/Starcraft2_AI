using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg
{
    class ZergActions : UnitActions
    {
        protected uint burrowedUnitType = 0;

        // Upgrades
        private int burrowUpgrade = Abilities.BURROW;

        // Abilities
        protected int burrow = 0;
        protected int unburrow = 0;

        public ZergActions(ControllerDefault controller) : base(controller)
        {
        }

        // Checks to see if the passed unit is of the burrowed unit type that is action controller will deal with.
        public bool IsBurrowedUnitType(Unit unit)
        {
            var isUnitType = false;

            if (unit.unitType == burrowedUnitType)
            {
                isUnitType = true;
            }

            return isUnitType;
        }

        // Used the units burrow command if it has it.
        public void Burrow(Unit unit)
        {
            if (burrow == 0) return;

            if (!IsUnitType(unit)) return;

            if (!controller.HasUpgrade(burrowUpgrade)) return;

            unit.UseAbility(burrow);
        }

        // Used the units unburrow command if it has it.
        public void Unburrow(Unit unit)
        {
            if (burrow == 0) return;

            if (!IsBurrowedUnitType(unit)) return;

            if (!controller.HasUpgrade(burrowUpgrade)) return;

            unit.UseAbility(unburrow);
        }
    }
}
