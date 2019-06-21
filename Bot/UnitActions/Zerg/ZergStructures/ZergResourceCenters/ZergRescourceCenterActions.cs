using Bot.UnitActions.Zerg;
using Bot.Utilities;
using SC2APIProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg.ZergStructures.ZergResourceCenters
{
    class ZergRescourceCenterActions : ZergActions
    {
        protected int researchBurrow = Abilities.RESEARCH_BURROW;

        protected int researchPneumatizedCarapace = Abilities.RESEARCH_PNEUMATIZED_CARAPACE;
        protected int pneumatizedCarapceUpgrade = Abilities.PNEUMATIZED_CARAPACE;

        protected int unitRally = Abilities.HATCHERY_UNIT_RALLY;
        protected int workerRally = Abilities.HATCHERY_WORKER_RALLY;

        protected uint queen = Units.QUEEN;

        protected List<ulong> unitRallySet = new List<ulong>();
        protected List<ulong> workerRallySet = new List<ulong>();

        // Usually there is only 1 queen for each resource center.
        // This is a random chance to get more.
        // Note: If the queens wander out of sight of a resource center then it will try and birth another queen.
        public int chanceOfExtraQueens = 2;

        public int researchBurrowChance = 30;
        public int researchPneumatizedCarapaceChance = 40;

        public enum BirthQueenResult { Success, NotUnitType, UnitBusy, CanNotConstruct };

        public ZergRescourceCenterActions(ZergController controller) : base(controller)
        {
        }

        // Check and see if the unit is busy.
        override
            public bool IsBusy(Unit unit)
        {
            if (unit.buildProgress != 1) return true;

            if (unit.order.AbilityId != 0) return true;

            return false;
        }

        // Researches an ability.
        private ResearchResult ResearchAbility(Unit unit, int researchID, int upgradeID)
        {
            if (!IsUnitType(unit)) return ResearchResult.NotUnitType;

            if (controller.HasUpgrade(upgradeID)) return ResearchResult.AlreadyHas;

            if (controller.IsResearchingUpgrade(researchID, Units.ResourceCenters)) return ResearchResult.IsResearching;

            if (controller.GetTotalCount(Units.GasGeysersStructures) == 0) return ResearchResult.NoGasGysersStructures;

            if (!controller.CanAffordUpgrade(researchID)) return ResearchResult.CanNotAfford;

            if (IsBusy(unit)) return ResearchResult.UnitBusy;

            unit.Research(researchID);

            return ResearchResult.Success;
        }


        // Researches the burrow ability.
        public ResearchResult ResearchBurrow(Unit unit)
        {
            var result = ResearchAbility(unit, researchBurrow, burrowUpgrade);

            return result;
        }

        // Researches the pneumatized carapace upgrade.
        public ResearchResult ResearchPneumatizedCarapace(Unit unit)
        {
            var result = ResearchAbility(unit, researchPneumatizedCarapace, pneumatizedCarapceUpgrade);

            return result;
        }

        // Create a queen.
        public BirthQueenResult BirthQueen(Unit unit)
        {
            if (!IsUnitType(unit)) return BirthQueenResult.NotUnitType;

            if (IsBusy(unit)) return BirthQueenResult.UnitBusy;

            if (!controller.CanConstruct(queen)) return BirthQueenResult.CanNotConstruct;

            unit.Train(Units.QUEEN);
            return BirthQueenResult.Success;
        }

        // Ask for help if being attack.
        override
            public void NeedHelpAction(Unit unit)
        {
            var enemyAttackers = controller.GetPotentialAttackers(unit);

            if (enemyAttackers.Count > 0)
            {
                SummonHelp(unit, enemyAttackers[0], idleArmyOnly: false);
                Logger.Info("{0} is under attack by {1} and summons help.", unit.name, enemyAttackers[0].name);
            }
        }

        // Set a unit rally point opposite the resources near by if any.
        public void SetUnitRally(Unit unit)
        {
            // If the rally point is already set only reset it rarely.
            if (unitRallySet.Contains(unit.tag) && random.Next(100) >= 1) return;

            var unitSight = (int)unit.sight;

            var resources = controller.GetUnits(Units.Resources, alliance: Alliance.Neutral);

            LocationsDistanceFromList resourceDistance = new LocationsDistanceFromList(unit.position);
            resourceDistance.AddLocation(resources);

            var xMin = -unitSight;
            var xMax = unitSight;
            var yMin = -unitSight;
            var yMax = unitSight;

            // Set the units to rally somewhere opposite the resources.
            if (resourceDistance.toLocations.Count > 0)
            {
                if (resourceDistance.toLocations[0].distance <= unitSight)
                {
                    var x = resourceDistance.toLocations[0].location.X;
                    var y = resourceDistance.toLocations[0].location.Y;

                    if (x <= unit.position.X)
                    {
                        xMin = 1;
                    }
                    else
                    {
                        xMax = -1;
                    }

                    if (y <= unit.position.Y)
                    {
                        yMin = 1;
                    }
                    else
                    {
                        yMax = -1;
                    }
                }
            }

            var rallySpot = controller.GetRandomLocation(unit.position, xMin, xMax, yMin, yMax);

            var constructAction = ControllerDefault.CreateRawUnitCommand(unitRally);
            constructAction.ActionRaw.UnitCommand.UnitTags.Add(unit.tag);
            constructAction.ActionRaw.UnitCommand.TargetWorldSpacePos = new Point2D();
            constructAction.ActionRaw.UnitCommand.TargetWorldSpacePos.X = rallySpot.X;
            constructAction.ActionRaw.UnitCommand.TargetWorldSpacePos.Y = rallySpot.Y;
            ControllerDefault.AddAction(constructAction);

            if (!unitRallySet.Contains(unit.tag))
            {
                unitRallySet.Add(unit.tag);
            }

            Logger.Info("Set {0} unit rally point @ {1}, {2}", unit.name, rallySpot.X, rallySpot.Y);
        }

        // Set a worker rally point to the closest resource.
        public void SetWorkerRally(Unit unit)
        {
            // If the rally point is already set only reset it rarely.
            if (workerRallySet.Contains(unit.tag) && random.Next(100) >= 1) return;

            var unitSight = (int)unit.sight;

            var resources = controller.GetUnits(Units.Resources, alliance: Alliance.Neutral);

            LocationsDistanceFromList resourceDistance = new LocationsDistanceFromList(unit.position);
            resourceDistance.AddLocation(resources);

            if (resourceDistance.toLocations.Count > 0)
            {
                if (resourceDistance.toLocations[0].distance <= unitSight)
                {
                    var rallySpot = resourceDistance.toLocations[0].location;

                    var constructAction = ControllerDefault.CreateRawUnitCommand(workerRally);
                    constructAction.ActionRaw.UnitCommand.UnitTags.Add(unit.tag);
                    constructAction.ActionRaw.UnitCommand.TargetWorldSpacePos = new Point2D();
                    constructAction.ActionRaw.UnitCommand.TargetWorldSpacePos.X = rallySpot.X;
                    constructAction.ActionRaw.UnitCommand.TargetWorldSpacePos.Y = rallySpot.Y;
                    ControllerDefault.AddAction(constructAction);

                    if (!workerRallySet.Contains(unit.tag))
                    {
                        workerRallySet.Add(unit.tag);
                    }

                    Logger.Info("Set {0} worker rally point @ {1}, {2}", unit.name, rallySpot.X, rallySpot.Y);
                }
            }
        }

        // Get the closest queen that can see the resource center.
        protected Unit GetNearestQueen(Unit unit)
        {
            var queens = controller.GetUnits(queen);

            Unit nearestQueen = null;

            if (queens.Count > 0)
            {
                nearestQueen = controller.GetClosestUnit(unit, queens, queens[0].sight);
            }

            return nearestQueen;
        }
    }
}
