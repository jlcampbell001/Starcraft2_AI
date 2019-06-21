using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg.ZergUnits
{
    // This is not handling the unit morphs.  Morphs are handled in the bot.
    class LarvaActions : ZergActions
    {
        protected double distanceToResourceCenter = 5.0;

        protected uint droneTrain = Units.DRONE;
        protected uint overlordTrain = Units.OVERLORD;
        protected uint zerglingTrain = Units.ZERGLING;

        public LarvaActions(ZergController controller) : base(controller)
        {
            unitType = Units.LARVA;
        }

        public override void PreformIntelligentActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, ref bool ignoreSaveRandomRoll,
            bool saveFor = false, bool doNotUseResources = false)
        {
            if (!IsUnitType(unit)) return;

            NotNearResourceCenterActions(unit);
        }

        public override void PreformRandomActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, ref bool ignoreSaveRandomRoll,
            bool saveFor = false, bool doNotUseResources = false)
        {
            if (!IsUnitType(unit)) return;

            NotNearResourceCenterActions(unit);
        }

        // If a larva is not near a resource center then the creep will most likely be gone soon and it will die so morph in to something.
        public void NotNearResourceCenterActions(Unit unit)
        {
            if (!IsUnitType(unit)) return;

            var resourceCenter = controller.GetClosestUnit(unit, Units.ResourceCenters, distanceToResourceCenter);

            if (resourceCenter == null)
            {
                var rollRange = 3;

                if (controller.GetTotalCount(Units.SPAWNING_POOL) == 0)
                {
                    rollRange = 2;
                }
                var randomMorph = random.Next(rollRange);

                switch (randomMorph)
                {
                    case 0:
                        {
                            unit.Train(droneTrain);
                            break;
                        }
                    case 1:
                        {
                            unit.Train(overlordTrain);
                            break;
                        }
                    case 2:
                        {
                            unit.Train(zerglingTrain);
                            break;
                        }
                }
            }
        }
    }
}
