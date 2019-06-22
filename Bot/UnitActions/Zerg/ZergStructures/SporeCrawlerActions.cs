using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg.ZergStructures
{
    class SporeCrawlerActions : ZergStructureActions
    {
        public SporeCrawlerActions(ZergController controller) : base(controller)
        {
            unitType = Units.SPORE_CRAWLER;
        }

        public override void PreformIntelligentActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, ref bool ignoreSaveRandomRoll, bool saveFor = false, bool doNotUseResources = false)
        {
            //base.PreformIntelligentActions(unit, ref saveUnit, ref saveUpgrade, ref ignoreSaveRandomRoll, saveFor, doNotUseResources);
        }

        public override void PreformRandomActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, ref bool ignoreSaveRandomRoll, bool saveFor = false, bool doNotUseResources = false)
        {
            //base.PreformRandomActions(unit, ref saveUnit, ref saveUpgrade, ref ignoreSaveRandomRoll, saveFor, doNotUseResources);
        }
    }
}
