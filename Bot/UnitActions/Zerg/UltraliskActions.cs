using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg
{
    class UltraliskActions : ZergActions
    {
        public UltraliskActions(ZergController controller) : base(controller)
        {
            unitType = Units.ULTRALISK;
            burrowedUnitType = Units.ULTRALISK_BURROWED;

            burrow = Abilities.BURROW_ULTRALISK;
            unburrow = Abilities.UNBURROW_ULTRALISK;
        }
    }
}
