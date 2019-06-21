using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg.ZergUnits.OverlordsAndOverseers
{
    class OverlordTransportActions : OverlordActions
    {
        public OverlordTransportActions(ZergController controller) : base(controller)
        {
            unitType = Units.OVERLORD_TRANSPORT;
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

            var generatingCreep = false;

            var randomAction = random.Next(2);

            switch (randomAction)
            {
                case 0:
                    generatingCreep = GenerateCreep(unit);
                    break;
                case 1:
                    GenerateCreepStop(unit);
                    break;
            }

            // Move the overlord around.
            if (!generatingCreep)
            {
                MoveAroundResourceCenter(unit);
            }
        }
    }
}
