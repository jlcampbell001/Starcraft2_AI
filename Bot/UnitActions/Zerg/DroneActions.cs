using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg
{
    class DroneActions : ZergActions
    {
        public DroneActions(ControllerDefault controller) : base(controller)
        {
            unitType = Units.DRONE;
            burrowedUnitType = Units.DRONE_BURROWED;

            burrow = Abilities.BURROW_DRONE;
            unburrow = Abilities.UNBURROW_DRONE;
        }
    }
}
