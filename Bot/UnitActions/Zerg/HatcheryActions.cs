using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg
{
    class HatcheryActions : ZergRescourceCenterActions
    {
        private readonly uint lair = Units.LAIR;

        public enum LairResult { Success, NotUnitType, UnitBusy, CanNotConstruct };

        public HatcheryActions(ZergController controller) : base(controller)
        {
            unitType = Units.HATCHERY;
        }

        // Upgrade the hatchery to a lair.
        public LairResult UpgradeToLair(Unit unit)
        {
            if (!IsUnitType(unit)) return LairResult.NotUnitType;

            if (IsBusy(unit)) return LairResult.UnitBusy;

            if (!controller.CanConstruct(lair)) return LairResult.CanNotConstruct;

            unit.Train(Units.LAIR);
            Logger.Info("Upgrade to Lair @ {0} / {1}", unit.position.X, unit.position.Y);

            return LairResult.Success;
        }

        // Pick a random action to preform.
        override
        public void PreformRandomActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, bool saveFor = false)
        {
            // Ask for help if being attack.
            NeedHelpAction(unit);
            
            var randomAction = random.Next(3);

            switch (randomAction)
            {
                case 0:
                    var laiResult = UpgradeToLair(unit);
                    if (saveFor && laiResult == LairResult.CanNotConstruct)
                    {
                        saveUnit = lair;
                    }
                    break;
                case 1:
                    var burrowResult = ResearchBurrow(unit);
                    if (saveFor && burrowResult == ResearchBurrowResult.CanNotAfford)
                    {
                        saveUpgrade = researchBurrow;
                    }
                    break;
                case 2:
                    var queenResult = BirthQueen(unit);
                    if (saveFor && queenResult == BirthQueenResult.CanNotConstruct)
                    {
                        saveUnit = queen;
                    }
                    break;
            }
        }
    }
}
