using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg.ZergUnits
{
    class LurkerActions : ZergActions
    {
        public LurkerActions(ZergController controller) : base(controller)
        {
            unitType = Units.LURKER;
            burrowedUnitType = Units.LURKER_BURROWED;

            burrow = Abilities.BURROW_LURKER;
            unburrow = Abilities.UNBURROW_LURKER;
        }
    }
}
