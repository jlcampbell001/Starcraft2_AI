using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg.ZergUnits.OverlordsAndOverseers
{
    class OverseerActions : OverlordActions
    {
        public OverseerActions(ZergController controller) : base(controller)
        {
            unitType = Units.OVERSEER;
        }


        public override void PreformIntelligentActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, ref bool ignoreSaveRandomRoll, bool saveFor = false,
            bool doNotUseResources = false)
        {
            if (!IsUnitType(unit)) return;

            // If under attack ask for help.
            NeedHelpAction(unit);

            // Move the overlord around.
            MoveAroundResourceCenter(unit);
        }

        public override void PreformRandomActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, ref bool ignoreSaveRandomRoll, bool saveFor = false,
           bool doNotUseResources = false)
        {
            if (!IsUnitType(unit)) return;

            // If under attack ask for help.
            NeedHelpAction(unit);

            

            // Move the overlord around.
            MoveAroundResourceCenter(unit);
        }
    }
}
