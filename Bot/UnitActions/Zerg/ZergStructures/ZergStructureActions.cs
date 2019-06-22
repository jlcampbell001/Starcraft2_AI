using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg.ZergStructures
{
    class ZergStructureActions : ZergActions
    {
        public ZergStructureActions(ZergController controller) : base(controller)
        {
        }

        public override void PreformIntelligentActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, ref bool ignoreSaveRandomRoll, bool saveFor = false, bool doNotUseResources = false)
        {
            // If being attacked call for help.
            NeedHelpAction(unit);
        }

        public override void PreformRandomActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, ref bool ignoreSaveRandomRoll, bool saveFor = false, bool doNotUseResources = false)
        {
            // If being attacked call for help.
            NeedHelpAction(unit);
        }
    }
}
