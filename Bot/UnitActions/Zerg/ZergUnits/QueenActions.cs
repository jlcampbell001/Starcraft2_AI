using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg.ZergUnits
{
    class QueenActions : ZergActions
    {
        public QueenActions(ZergController controller) : base(controller)
        {
            unitType = Units.QUEEN;
            burrowedUnitType = Units.QUEEN_BURROWED;

            burrow = Abilities.BURROW_QUEEN;
            unburrow = Abilities.UNBURROW_QUEEN;
        }
    }
}
