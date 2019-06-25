using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg.ZergStructures.ZergResourceCenters
{
    class HatcheryActions : ZergRescourceCenterActions
    {
        private readonly uint lair = Units.LAIR;

        public enum LairResult { Success, NotUnitType, UnitBusy, CanNotConstruct };

        public HatcheryActions(ZergController controller, QueenToResourceCenterManager queenToResourceCenterManager) : base(controller, queenToResourceCenterManager)
        {
            unitType = Units.HATCHERY;
        }

        // Upgrade the hatchery to a lair.
        public LairResult UpgradeToLair(Unit unit)
        {
            if (!IsUnitType(unit)) return LairResult.NotUnitType;

            if (IsBusy(unit)) return LairResult.UnitBusy;

            if (!controller.CanConstruct(lair)) return LairResult.CanNotConstruct;

            unit.Train(lair);
            Logger.Info("Upgrade to Lair @ {0} / {1}", unit.position.X, unit.position.Y);

            return LairResult.Success;
        }

        // Try an preform intelligent actions for the unit.
        public override void PreformIntelligentActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, ref bool ignoreSaveRandomRoll,
            bool saveFor = false, bool doNotUseResources = false)
        {
            base.PreformIntelligentActions(unit, ref saveUnit, ref saveUpgrade, ref ignoreSaveRandomRoll, saveFor, doNotUseResources);

            // Set the rally points.
            SetUnitRally(unit);
            SetWorkerRally(unit);

            if (!doNotUseResources)
            {
                // If there is no queen near by create one.
                if (random.Next(100) < chanceOfExtraQueens || GetAssignedQueen(unit) == null)
                {
                    var queenResult = BirthQueen(unit);
                    if (saveFor && queenResult == BirthQueenResult.CanNotConstruct)
                    {
                        saveUnit = queen;
                    }
                }
                else if (random.Next(100) < researchBurrowChance)
                {
                    var burrowResult = ResearchBurrow(unit);
                    if (saveFor && burrowResult == ResearchResult.CanNotAfford)
                    {
                        saveUpgrade = researchBurrow;
                    }
                }
                else if (random.Next(100) < researchPneumatizedCarapaceChance)
                {
                    var pneumatizedCarapaceResult = ResearchPneumatizedCarapace(unit);
                    if (saveFor && pneumatizedCarapaceResult == ResearchResult.CanNotAfford)
                    {
                        saveUpgrade = researchPneumatizedCarapace;
                    }
                }
                else
                {
                    var lairResult = UpgradeToLair(unit);
                    if (saveFor && lairResult == LairResult.CanNotConstruct)
                    {
                        saveUnit = lair;
                    }
                }
            }
        }

        // Pick a random action to preform.
        public override void PreformRandomActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, ref bool ignoreSaveRandomRoll,
            bool saveFor = false, bool doNotUseResources = false)
        {
            base.PreformRandomActions(unit, ref saveUnit, ref saveUpgrade, ref ignoreSaveRandomRoll, saveFor, doNotUseResources);

            var randomAction = random.Next(6);

            switch (randomAction)
            {
                case 0:
                    if (doNotUseResources) return;

                    var lairResult = UpgradeToLair(unit);
                    if (saveFor && lairResult == LairResult.CanNotConstruct)
                    {
                        saveUnit = lair;
                    }
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
