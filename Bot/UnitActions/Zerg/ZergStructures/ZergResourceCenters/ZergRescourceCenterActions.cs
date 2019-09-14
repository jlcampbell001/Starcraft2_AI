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
    class ZergRescourceCenterActions : ZergStructureActions
    {
        protected QueenToResourceCenterManager queenToResourceCenterManager;

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

        public ZergRescourceCenterActions(ZergController controller, QueenToResourceCenterManager queenToResourceCenterManager) : base(controller)
        {
            this.queenToResourceCenterManager = queenToResourceCenterManager ?? throw new ArgumentNullException(nameof(queenToResourceCenterManager));
        }

        // ********************************************************************************
        /// <summary>
        /// Preform an Intelligent actions for the unit.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <param name="saveUnit">Setting to return a value to save resources for a unit.</param>
        /// <param name="saveUpgrade">Setting to return a value to save resources for an upgrade.</param>
        /// <param name="ignoreSaveRandomRoll"> Setting to return to turn off the random chance when deciding to save resources.</param>
        /// <param name="saveFor">If set true it will setup the save resource information.</param>
        /// <param name="doNotUseResources">If set true it will not run any action that requires resources.</param>
        // ********************************************************************************
        public override void PreformIntelligentActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, ref bool ignoreSaveRandomRoll, bool saveFor = false, bool doNotUseResources = false)
        {
            base.PreformIntelligentActions(unit, ref saveUnit, ref saveUpgrade, ref ignoreSaveRandomRoll, saveFor, doNotUseResources);
        }

        // ********************************************************************************
        /// <summary>
        /// Preform a random action for the passed unit.
        /// </summary>
        /// <param name="unit">The unit.</param>
        /// <param name="saveUnit">Setting to return a value to save resources for a unit.</param>
        /// <param name="saveUpgrade">Setting to return a value to save resources for an upgrade.</param>
        /// <param name="ignoreSaveRandomRoll"> Setting to return to turn off the random chance when deciding to save resources.</param>
        /// <param name="saveFor">If set true it will setup the save resource information.</param>
        /// <param name="doNotUseResources">If set true it will not run any action that requires resources.</param>
        // ********************************************************************************
        public override void PreformRandomActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, ref bool ignoreSaveRandomRoll, bool saveFor = false, bool doNotUseResources = false)
        {
            base.PreformRandomActions(unit, ref saveUnit, ref saveUpgrade, ref ignoreSaveRandomRoll, saveFor, doNotUseResources);
        }

        // ********************************************************************************
        /// <summary>
        /// Research the passed ability if possible.
        /// </summary>
        /// <param name="unit">The unit doing the research.</param>
        /// <param name="researchID">The research ID.</param>
        /// <param name="upgradeID">The ID to look up to see if it is done already.</param>
        /// <returns>A ReserachResult.</returns>
        // ********************************************************************************
        protected override ResearchResult ResearchAbility(Unit unit, int researchID, int upgradeID, bool needVespene = true)
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

        // ********************************************************************************
        /// <summary>
        /// Researches the burrow ability.
        /// </summary>
        /// <param name="unit">The unit to do the research.</param>
        /// <returns>The ResearchResult.</returns>
        // ********************************************************************************
        public ResearchResult ResearchBurrow(Unit unit)
        {
            var result = ResearchAbility(unit, researchBurrow, burrowUpgrade);

            return result;
        }

        // ********************************************************************************
        /// <summary>
        /// Researches the pneumatized carapace upgrade.
        /// </summary>
        /// <param name="unit">The unit to do the research.</param>
        /// <returns>The ResearchResult.</returns>
        // ********************************************************************************
        public ResearchResult ResearchPneumatizedCarapace(Unit unit)
        {
            var result = ResearchAbility(unit, researchPneumatizedCarapace, pneumatizedCarapceUpgrade);

            return result;
        }

        // ********************************************************************************
        /// <summary>
        /// Create a queen.
        /// </summary>
        /// <param name="unit">The unit to create the queen.</param>
        /// <returns>The BirthQueenResult.</returns>
        // ********************************************************************************
        public BirthQueenResult BirthQueen(Unit unit)
        {
            if (!IsUnitType(unit)) return BirthQueenResult.NotUnitType;

            if (IsBusy(unit)) return BirthQueenResult.UnitBusy;

            if (!controller.CanConstruct(queen)) return BirthQueenResult.CanNotConstruct;

            unit.Train(Units.QUEEN);
            return BirthQueenResult.Success;
        }


        // ********************************************************************************
        /// <summary>
        /// Summons all army units if under attack.
        /// </summary>
        /// <param name="unit">The unit under attack.</param>
        /// <param name="summonHelp">If true summon help.</param>
        /// <returns>true if under attack.</returns>
        // ********************************************************************************
        public override bool NeedHelpAction(Unit unit, bool summonHelp = true)
        {
            var enemyAttackers = controller.GetPotentialAttackers(unit);
            var underAttack = false;

            if (enemyAttackers.Count > 0)
            {
                if (summonHelp)
                {
                    SummonHelp(unit, enemyAttackers[0], idleArmyOnly: false);
                    controller.LogIfSelectedUnit(unit, "{0} is under attack by {1} and summons help.", unit.name, enemyAttackers[0].name);
                }

                underAttack = true;
            }

            return underAttack;
        }

        // ********************************************************************************
        /// <summary>
        /// Set a unit rally point opposite the resources near by if any.
        /// </summary>
        /// <param name="unit">The unit to set the rally for.</param>
        // ********************************************************************************
        public void SetUnitRally(Unit unit)
        {
            // If the rally point is already set only reset it rarely.
            if (unitRallySet.Contains(unit.tag) && Random.Next(100) >= 1) return;

            var unitSight = (int)unit.sight;

            var resources = controller.GetUnits(Units.Resources, alliance: Alliance.Neutral);

            var resourceDistance = new LocationsDistanceFromList(unit.position);
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

            controller.LogIfSelectedUnit(unit, "Set {0} unit rally point @ {1}, {2}", unit.name, rallySpot.X, rallySpot.Y);
        }

        // ********************************************************************************
        /// <summary>
        /// Set a worker rally point to the closest resource.
        /// </summary>
        /// <param name="unit">The unit to set the work rally point.</param>
        // ********************************************************************************
        public void SetWorkerRally(Unit unit)
        {
            // If the rally point is already set only reset it rarely.
            if (workerRallySet.Contains(unit.tag) && Random.Next(100) >= 1) return;

            var unitSight = (int)unit.sight;

            var resources = controller.GetUnits(Units.Resources, alliance: Alliance.Neutral);

            var resourceDistance = new LocationsDistanceFromList(unit.position);
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

                    controller.LogIfSelectedUnit(unit, "Set {0} worker rally point @ {1}, {2}", unit.name, rallySpot.X, rallySpot.Y);
                }
            }
        }

        // ********************************************************************************
        /// <summary>
        /// Get the queen that is assigned the resource center.
        /// </summary>
        /// <param name="unit">The resource center.</param>
        /// <returns>The found assigned queen or null if none is assigned.</returns>
        // ********************************************************************************
        protected Unit GetAssignedQueen(Unit unit)
        {
            var queenLink = queenToResourceCenterManager.FindLinkByResourceCenter(unit.tag, true);

            Unit foundQueen = null;

            if (queenLink != null)
            {
                foundQueen = controller.GetUnitByTag(queenLink.Tag1);
            }

            return foundQueen;
        }
    }
}
