using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg
{
    class RoachActions : ZergActions
    {
        public RoachActions(ZergController controller) : base(controller)
        {
            unitType = Units.ROACH;
            burrowedUnitType = Units.ROACH_BURROWED;

            burrow = Abilities.BURROW_ROACH;
            unburrow = Abilities.UNBURROW_ROACH;
        }
    }
}
