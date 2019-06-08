using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg
{
    class HiveActions : ZergRescourceCenterActions
    {
        public HiveActions(ZergController controller) : base(controller)
        {
            unitType = Units.HIVE;
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
