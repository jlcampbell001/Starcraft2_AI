using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg
{
    class HydraliskActions : ZergActions
    {
        public HydraliskActions(ZergController controller) : base(controller)
        {
            unitType = Units.HYDRALISK;
            burrowedUnitType = Units.HYDRALISK_BURROWED;

            burrow = Abilities.BURROW_HYDRALISK;
            unburrow = Abilities.UNBURROW_HYDRALISK;
        }
    }
}
