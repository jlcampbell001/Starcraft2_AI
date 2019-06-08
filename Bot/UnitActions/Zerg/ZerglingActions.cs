using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg
{
    class ZerglingActions : ZergActions
    {
        public ZerglingActions(ZergController controller) : base(controller)
        {
            unitType = Units.ZERGLING;
            burrowedUnitType = Units.ZERGLING_BURROWED;

            burrow = Abilities.BURROW_ZERGLING;
            unburrow = Abilities.UNBURROW_ZERGLING;
        }


    }
}
