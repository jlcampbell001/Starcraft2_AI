using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg.ZergUnits
{
    class RavagerActions : ZergActions
    {
        public RavagerActions(ZergController controller) : base(controller)
        {
            unitType = Units.RAVAGER;
            burrowedUnitType = Units.RAVAGER_BURROWED;

            burrow = Abilities.BURROW_RAVAGER;
            unburrow = Abilities.UNBURROW_RAVAGER;
        }
    }
}
