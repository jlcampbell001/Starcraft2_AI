using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg
{
    class LairActions : ZergRescourceCenterActions
    {
        public LairActions(ZergController controller) : base(controller)
        {
            unitType = Units.LAIR;
        }

        // Pick a random action to preform.
        override
        public void PreformRandomActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, bool saveFor = false)
        {
            // Ask for help if being attack.
            NeedHelpAction(unit);

        }
    }
}
