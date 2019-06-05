using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg
{
    class BanelingActions : ZergActions
    {
        public BanelingActions(ControllerDefault controller) : base(controller)
        {
            unitType = Units.BANELING;
            burrowedUnitType = Units.BANELING_BURROWED;

            burrow = Abilities.BURROW_BANELING;
            unburrow = Abilities.UNBURROW_BANELING;
        }
    }
}
