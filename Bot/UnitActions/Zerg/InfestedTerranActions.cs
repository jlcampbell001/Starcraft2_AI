using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg
{
    class InfestedTerranActions : ZergActions
    {
        public InfestedTerranActions(ZergController controller) : base(controller)
        {
            unitType = Units.INFESTED_TERRAN;
            burrowedUnitType = Units.INFESTED_TERRAN_BURROWED;

            burrow = Abilities.BURROW_INFESTED_TERRAN;
            unburrow = Abilities.UNBURROW_INFESTED_TERRAN;
        }
    }
}
