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
        public void PreformRandomActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, bool saveFor = false, bool doNotUseResources = false)
        {
            // Ask for help if being attack.
            NeedHelpAction(unit);

            var randomAction = random.Next(6);

            switch (randomAction)
            {
                case 0:
                    if (doNotUseResources) return;

                    /*
                    var laiResult = UpgradeToLair(unit);
                    if (saveFor && laiResult == LairResult.CanNotConstruct)
                    {
                        saveUnit = lair;
                    }*/
                    break;
                case 1:
                    if (doNotUseResources) return;

                    var burrowResult = ResearchBurrow(unit);
                    if (saveFor && burrowResult == ResearchResult.CanNotAfford)
                    {
                        saveUpgrade = researchBurrow;
                    }
                    break;
                case 2:
                    if (doNotUseResources) return;

                    var queenResult = BirthQueen(unit);
                    if (saveFor && queenResult == BirthQueenResult.CanNotConstruct)
                    {
                        saveUnit = queen;
                    }
                    break;
                case 3:
                    if (doNotUseResources) return;

                    var pneumatizedCarapaceResult = ResearchPneumatizedCarapace(unit);
                    if (saveFor && pneumatizedCarapaceResult == ResearchResult.CanNotAfford)
                    {
                        saveUpgrade = researchPneumatizedCarapace;
                    }
                    break;
                case 4:
                    SetUnitRally(unit);
                    break;
                case 5:
                    SetWorkerRally(unit);
                    break;
            }
        }

    }
}
