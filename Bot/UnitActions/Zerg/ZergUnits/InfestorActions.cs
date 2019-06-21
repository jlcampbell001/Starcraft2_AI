using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg.ZergUnits
{
    class InfestorActions : ZergActions
    {
        public InfestorActions(ZergController controller) : base(controller)
        {
            unitType = Units.INFESTOR;
            burrowedUnitType = Units.INFESTOR_BURROWED;

            burrow = Abilities.BURROW_INFESTOR;
            unburrow = Abilities.UNBURROW_INFESTOR;
        }
    }
}
