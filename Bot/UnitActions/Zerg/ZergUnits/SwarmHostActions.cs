using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg.ZergUnits
{
    class SwarmHostActions : ZergActions
    {
        public SwarmHostActions(ZergController controller) : base(controller)
        {
            unitType = Units.SWARM_HOST;
            burrowedUnitType = Units.SWARM_HOST_BURROWED;

            burrow = Abilities.BURROW_SWARM_HOST;
            unburrow = Abilities.UNBURROW_SWARM_HOST;
        }
    }
}
