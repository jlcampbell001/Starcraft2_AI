using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading;
using SC2APIProtocol;
using Action = SC2APIProtocol.Action;

namespace Bot
{
    class ControllerDefault
    {
        private readonly int frameDelay = 0; //too fast? increase this to e.g. 20

        private const int MIN_WORKERS_MINING_PER_ACTIVE_GAS_BUILDING = 2;
        private const int BUILD_RANGE_RADIUS = 12;

        private readonly static List<Action> actions = new List<Action>();
        private readonly Random random = new Random();
        private const double FRAMES_PER_SECOND = 22.4;

        public static ResponseGameInfo gameInfo;
        public static ResponseData gameData;
        public static ResponseObservation obs;
        public ulong frame;
        public uint currentSupply;
        public uint maxSupply;
        public uint minerals;
        public uint vespene;

        public List<Vector3> enemyLocations = new List<Vector3>();
        public List<string> chatLog = new List<string>();

        /**********
         * Frames
         **********/

        // Converts the passed seconds to frames.
        public ulong SecsToFrames(int seconds)
        {
            return (ulong)(FRAMES_PER_SECOND * seconds);
        }

        // Close the frame.
        public List<Action> CloseFrame()
        {
            return actions;
        }


        // Open the frame.
        public void OpenFrame()
        {
            if (gameInfo == null || gameData == null || obs == null)
            {
                if (gameInfo == null)
                    Logger.Info("GameInfo is null! The application will terminate.");
                else if (gameData == null)
                    Logger.Info("GameData is null! The application will terminate.");
                else
                    Logger.Info("ResponseObservation is null! The application will terminate.");
                Pause();
                Environment.Exit(0);
            }

            actions.Clear();

            foreach (var chat in obs.Chat)
                chatLog.Add(chat.Message);

            frame = obs.Observation.GameLoop;
            currentSupply = obs.Observation.PlayerCommon.FoodUsed;
            maxSupply = obs.Observation.PlayerCommon.FoodCap;
            minerals = obs.Observation.PlayerCommon.Minerals;
            vespene = obs.Observation.PlayerCommon.Vespene;

            //initialization
            if (frame == 0)
            {
                var resourceCenters = GetUnits(Units.ResourceCenters);
                if (resourceCenters.Count > 0)
                {
                    var rcPosition = resourceCenters[0].position;

                    foreach (var startLocation in gameInfo.StartRaw.StartLocations)
                    {
                        var enemyLocation = new Vector3(startLocation.X, startLocation.Y, 0);
                        var distance = Vector3.Distance(enemyLocation, rcPosition);
                        if (distance > 30)
                            enemyLocations.Add(enemyLocation);
                    }
                }
            }

            if (frameDelay > 0)
                Thread.Sleep(frameDelay);
        }

        /**********
         * Unit
         **********/

        // Get the unit name for the passed unit type.
        public static string GetUnitName(uint unitType)
        {
            return gameData.Units[(int)unitType].Name;
        }

        // Get a list of units based on a list of unit types.
        public List<Unit> GetUnits(HashSet<uint> hashset, Alliance alliance = Alliance.Self, bool onlyCompleted = false, bool onlyVisible = false)
        {
            // Ideally this should be cached in the future and cleared at each new frame.
            var units = new List<Unit>();
            foreach (var unit in obs.Observation.RawData.Units)
                if (hashset.Contains(unit.UnitType) && unit.Alliance == alliance)
                {
                    if (onlyCompleted && unit.BuildProgress < 1)
                        continue;

                    if (onlyVisible && (unit.DisplayType != DisplayType.Visible))
                        continue;

                    units.Add(new Unit(unit));
                }
            return units;
        }

        // Get a list of units based on a single unit type.
        public List<Unit> GetUnits(uint unitType, Alliance alliance = Alliance.Self, bool onlyCompleted = false, bool onlyVisible = false)
        {
            // Ideally this should be cached in the future and cleared at each new frame.
            var units = new List<Unit>();
            foreach (var unit in obs.Observation.RawData.Units)
                if (unit.UnitType == unitType && unit.Alliance == alliance)
                {
                    if (onlyCompleted && unit.BuildProgress < 1)
                        continue;

                    if (onlyVisible && (unit.DisplayType != DisplayType.Visible))
                        continue;

                    units.Add(new Unit(unit));
                }
            return units;
        }

        // Get a list of idle units from the passed list of units.
        public List<Unit> GetIdleUnits(List<Unit> units)
        {
            var idleUnits = new List<Unit>();
            foreach (var unit in units)
            {
                if (unit.order.AbilityId == 0)
                {
                    idleUnits.Add(unit);
                }
            }
            return idleUnits;
        }

        // Get all the units that can not see a resource center from the passed unit list.
        public List<Unit> GetUnitsNoInSightOfRC(List<Unit> units)
        {
            var canNotSeeUnits = new List<Unit>();
            var resourceCenters = GetUnits(Units.ResourceCenters);

            foreach (var unit in units)
            {
                if (GetFirstInRange(unit.position, resourceCenters, unit.sight) == null)
                {
                    canNotSeeUnits.Add(unit);
                }
            }
            return canNotSeeUnits;
        }

        // Get all the units that have vespene from the passed unit list.
        public List<Unit> GetUnitsWithVespene(List<Unit> units)
        {
            var withVespeneUnits = new List<Unit>();

            foreach (var unit in units)
            {
                if (unit.vespene > 0)
                {
                    withVespeneUnits.Add(unit);
                }
            }
            return withVespeneUnits;
        }

        // Get all the units that a targeting a certain unit from a passed unit list.
        public List<Unit> GetUnitsTargetingTag(List<Unit> units, ulong tag)
        {
            var targetingUnits = new List<Unit>();

            foreach (var unit in units)
            {
                //  Logger.Info("Name = {0};  Target = {1}", unit.name, unit.target);
                if (unit.target == tag)
                {
                    targetingUnits.Add(unit);
                }
            }
            return targetingUnits;
        }

        // Get all the units that are gathring from the target tag from a passed unit list.
        public List<Unit> GetUnitsGatheringTag(List<Unit> units, ulong tag)
        {
            var targetingUnits = new List<Unit>();

            foreach (var unit in units)
            {
                foreach (var order in unit.orders)
                {
                    // Check if the unit has the gathering resource order.
                    if (Abilities.GatherResources.Contains((int)order.AbilityId))
                    {
                        // Logger.Info("Name = {0};  Target = {1}; Compare Tag = {2}", unit.name, order.TargetUnitTag, tag);
                        if (order.TargetUnitTag == tag)
                        {
                            targetingUnits.Add(unit);
                        }
                    }
                }
            }
            return targetingUnits;
        }

        // Check if the passed unit is in range of the target position based on the provided distance.
        public bool IsInRange(Vector3 targetPosition, List<Unit> units, float maxDistance)
        {
            return (GetFirstInRange(targetPosition, units, maxDistance) != null);
        }

        // Get the first unit that is in range of the target position based on the provided distance.
        public Unit GetFirstInRange(Vector3 targetPosition, List<Unit> units, float maxDistance)
        {
            // Squared distance is faster to calculate.
            var maxDistanceSqr = maxDistance * maxDistance;
            foreach (var unit in units)
            {
                if (Vector3.DistanceSquared(targetPosition, unit.position) <= maxDistanceSqr)
                    return unit;
            }
            return null;
        }

        // Get the next unit that is in range of the target position based on the provided distance.
        // Needs to know the last unit that was found other wise it will return the first unit it can find.
        public Unit GetNextInRange(Vector3 targetPosition, List<Unit> units, float maxDistance, Unit lastFound = null)
        {
            // Squared distance is faster to calculate.
            var maxDistanceSqr = maxDistance * maxDistance;
            var canSearch = false;

            if (lastFound == null)
            {
                canSearch = true;
            }

            foreach (var unit in units)
            {
                if (canSearch && Vector3.DistanceSquared(targetPosition, unit.position) <= maxDistanceSqr)
                    return unit;

                // Check and see if we are at the last unit found so we can start searching.
                if (unit.Equals(lastFound))
                {
                    canSearch = true;
                }
            }
            return null;
        }

        // Get a worker that is either doing nothing or just gathering minerals.
        public Unit GetAvailableWorker()
        {
            var workers = GetUnits(Units.Workers);

            // Check for idle worker first.
            foreach (var worker in workers)
            {
                if (worker.order.AbilityId != 0) continue;

                return worker;
            }

            foreach (var worker in workers)
            {
                if (!Abilities.GatherMinerals.Contains((int)worker.order.AbilityId)) continue;

                return worker;
            }

            return null;
        }

        // Check if the player can afford the unit and return by reference the cost.
        virtual
        public bool CanAfford(uint unitType, ref int unitMinerals, ref int unitVespene)
        {
            var unitData = gameData.Units[(int)unitType];
            unitMinerals = (int)unitData.MineralCost;
            unitVespene = (int)unitData.VespeneCost;
            return (minerals >= unitMinerals) && (vespene >= unitVespene);
        }

        // Check if the player can afford the unit.
        virtual
        public bool CanAfford(uint unitType)
        {
            var mineralCost = 0;
            var vespeneCost = 0;
            return CanAfford(unitType, ref mineralCost, ref vespeneCost);
        }

        // Check and see if the unit can be placed in the target position.
        public bool CanPlace(uint unitType, Vector3 targetPos)
        {
            //Note: this is a blocking call! Use it sparingly, or you will slow down your execution significantly!
            var abilityID = Abilities.GetID(unitType);

            RequestQueryBuildingPlacement queryBuildingPlacement = new RequestQueryBuildingPlacement();
            queryBuildingPlacement.AbilityId = abilityID;
            queryBuildingPlacement.TargetPos = new Point2D();
            queryBuildingPlacement.TargetPos.X = targetPos.X;
            queryBuildingPlacement.TargetPos.Y = targetPos.Y;

            Request requestQuery = new Request();
            requestQuery.Query = new RequestQuery();
            requestQuery.Query.Placements.Add(queryBuildingPlacement);

            var result = Program.gc.SendQuery(requestQuery.Query);

            if (result.Result.Placements.Count > 0)
                return (result.Result.Placements[0].Result == ActionResult.Success);
            return false;
        }

        // Get the total number of units in game for a passed unit type.
        public int GetTotalCount(uint unitType, bool inConstruction = false)
        {
            var pendingCount = GetPendingCount(unitType, inConstruction);
            var constructionCount = GetUnits(unitType).Count;
            return pendingCount + constructionCount;
        }

        // Get the total number of units pending in game for a passed unit type.
        public int GetPendingCount(uint unitType, bool inConstruction = true)
        {
            var workers = GetUnits(Units.Workers);
            var abilityID = Abilities.GetID(unitType);

            var counter = 0;

            //count workers that have been sent to build this structure
            foreach (var worker in workers)
            {
                if (worker.order.AbilityId == abilityID)
                    counter += 1;
            }

            //count buildings that are already in construction
            if (inConstruction)
            {
                foreach (var unit in GetUnits(unitType))
                    if (unit.buildProgress < 1)
                        counter += 1;
            }

            return counter;
        }

        /**********
         * Actions
         **********/

        // Crate a raw unit command action for the passed ability.
        public static Action CreateRawUnitCommand(int ability)
        {
            var action = new Action();
            action.ActionRaw = new ActionRaw();
            action.ActionRaw.UnitCommand = new ActionRawUnitCommand();
            action.ActionRaw.UnitCommand.AbilityId = ability;
            return action;
        }

        // Add an action to the action list.
        public static void AddAction(Action action)
        {
            actions.Add(action);
        }

        // Distriubte the workers as needed.
        public void DistributeWorkers()
        {
            var resourceRange = 12;
            var gasBuildings = GetUnitsWithVespene(GetUnits(Units.GasGeysersStructures, onlyCompleted: true));
            var resourceCenters = GetUnits(Units.ResourceCenters, onlyCompleted: true);
            var mineralFields = GetUnits(Units.MineralFields, onlyVisible: true, alliance: Alliance.Neutral);
            var workers = GetUnits(Units.Workers);

            // Get a list of idle workers.
            List<Unit> idleWorkers = new List<Unit>();
            foreach (var worker in workers)
            {
                if (worker.order.AbilityId != 0) continue;
                idleWorkers.Add(worker);
            }

            if (idleWorkers.Count > 0)
            // If there are any idle works assign then gathering work.
            {
                // Logger.Info("Workers = {0};   Count = {1}; First = {2}", workers.Count, idleWorkers.Count, idleWorkers[0].tag);

                foreach (var resourceCenter in resourceCenters)
                {
                    // Get one of the closer resources.
                    var mineralField = GetFirstInRange(resourceCenter.position, mineralFields, resourceRange);
                    var gasBuilding = GetFirstInRange(resourceCenter.position, gasBuildings, resourceRange);

                    // Make sure the gas is being used with the proper number of workers.
                    while (gasBuilding != null && gasBuilding.assignedWorkers >= gasBuilding.idealWorkers)
                    {
                        gasBuilding = GetNextInRange(resourceCenter.position, gasBuildings, resourceRange, gasBuilding);
                    }

                    if (mineralField == null && gasBuilding == null) continue;

                    // Dealing with workers 1 at a time.
                    // Note: I need to play with this and see what happens when I deal with all of them.
                    if (idleWorkers[0].order.AbilityId == 0)
                    {
                        Logger.Info("Distributing idle worker: {0}", idleWorkers[0].tag);
                    }

                    if (random.Next(2) == 0 && mineralField != null)
                    {
                        idleWorkers[0].Smart(mineralField);
                    }
                    else if (gasBuilding != null)
                    {
                        idleWorkers[0].Smart(gasBuilding);
                    }
                    break;
                }
            }
            else
            {
                // Assign workers to gather gas.

                // Check to see if there are any gas geysers structures not full of workers that still have gas (see declaring gas builing list).
                // This will set the max workers gather gas based on how many should be mining.
                // I.E.: If the min is set at 2 (defualt) and there are 2 gas buildings and there are 16 workers the answer should be 2 workers to assign to each gas building.
                if (gasBuildings.Count > 0)
                {
                    var workersPerGB = 0;

                    for (var i = gasBuildings[0].idealWorkers; i > 0; i--)
                    {
                        if (workers.Count - i * gasBuildings.Count > MIN_WORKERS_MINING_PER_ACTIVE_GAS_BUILDING * i * gasBuildings.Count)
                        {
                            workersPerGB = i;
                            break;
                        }
                    }

                    // Logger.Info("GBs = {0};  Workers = {1}; Workers / GB = {2}", gasBuildings.Count, workers.Count, workersPerGB);

                    // Get the closest mineral field.
                    Unit mineralField = null;

                    foreach (var resourceCenter in resourceCenters)
                    {
                        mineralField = GetFirstInRange(resourceCenter.position, mineralFields, resourceRange);

                        if (mineralField != null)
                        {
                            break;
                        }
                    }

                    var workerId = 0;
                    foreach (var gasBuilding in gasBuildings)
                    {
                        if (gasBuilding.assignedWorkers == workersPerGB) continue;

                        // Too many works assign them to minerals.
                        if (mineralField != null && gasBuilding.assignedWorkers > workersPerGB)
                        {
                            var assignedWorkers = GetUnitsGatheringTag(workers, gasBuilding.tag);
                            //Logger.Info("AW.Count = {0};  GB.AW = {1}", assignedWorkers.Count, gasBuilding.assignedWorkers);
                            if (assignedWorkers.Count > 0)
                            {
                                // Remove all the workers it can find.  If is lowers it to below neede the next step will fill it back up.
                                // Need to do this because workers could be targeting the hatchery instead at the moment so we do not get the true count.
                                foreach (var worker in assignedWorkers)
                                {
                                    worker.Smart(mineralField);
                                }
                            }
                        }

                        // Not enough workers so assign some.
                        if (gasBuilding.assignedWorkers < workersPerGB)
                        {
                            for (var i = gasBuilding.assignedWorkers; i < workersPerGB; i++)
                            {
                                workers[workerId].Smart(gasBuilding);
                                workerId++;
                            }
                        }
                    }
                }
            }
        }

        // Construct a structure based on a loction not a target.
        public void Construct(uint unitType)
        {
            Vector3 startingSpot;

            // Use the first resouce centers as a starting point.
            // Note: Need to re-work this to handle multiple resouce centers and to get a starting point if there are no resource centers.
            var resourceCenters = GetUnits(Units.ResourceCenters);
            if (resourceCenters.Count > 0)
                startingSpot = resourceCenters[0].position;
            else
            {
                Logger.Error("Unable to construct: {0}. No resource center was found.", GetUnitName(unitType));
                return;
            }

            // Trying to find a valid construction spot
            var mineralFields = GetUnits(Units.MineralFields, onlyVisible: true, alliance: Alliance.Neutral);
            Vector3 constructionSpot;

            var i = 0;
            while (true)
            {
                i++;

                if (i == 1000)
                {
                    // There might be something wrong or there is no place left to place the structure.
                    Logger.Error("After 1000 trys we could not find a spot to place {0}.", GetUnitName(unitType));
                    return;
                }

                constructionSpot = new Vector3(startingSpot.X + random.Next(-BUILD_RANGE_RADIUS, BUILD_RANGE_RADIUS + 1), 
                    startingSpot.Y + random.Next(-BUILD_RANGE_RADIUS, BUILD_RANGE_RADIUS + 1), 0);

                // Avoid building in the mineral line.
                // Note:  Need to re-work this as it still happens and I need it for gas structures.
                if (IsInRange(constructionSpot, mineralFields, 5)) continue;

                // Check if the building fits.
                if (!CanPlace(unitType, constructionSpot)) continue;

                // Ok, we found a spot.
                break;
            }

            // Get the worker to create the structure.
            // Note:  Need to re-worker this so it gets a close worker.
            var worker = GetAvailableWorker();
            if (worker == null)
            {
                Logger.Error("Unable to find worker to construct: {0}", GetUnitName(unitType));
                return;
            }

            // Set up the construction command.
            var abilityID = Abilities.GetID(unitType);
            //Logger.Info("Ability ID: {0}", abilityID);
            var constructAction = CreateRawUnitCommand(abilityID);
            constructAction.ActionRaw.UnitCommand.UnitTags.Add(worker.tag);
            constructAction.ActionRaw.UnitCommand.TargetWorldSpacePos = new Point2D();
            constructAction.ActionRaw.UnitCommand.TargetWorldSpacePos.X = constructionSpot.X;
            constructAction.ActionRaw.UnitCommand.TargetWorldSpacePos.Y = constructionSpot.Y;
            AddAction(constructAction);

            Logger.Info("Constructing: {0} @ {1} / {2}", GetUnitName(unitType), constructionSpot.X, constructionSpot.Y);
        }

        // Construct a structure on a gas geyser.
        // This is needed because to can not use the gas geyser loction but need to target the gas geyser to build on it.
        // Note: This is similar to construct so see its notes for possible changes.
        public void ConstructOnGasGeyser(uint unitType)
        {
            // Gas buildings need to build on the target tag for the geyser not its position like other builds do.

            var resourceCenters = GetUnits(Units.ResourceCenters);
            if (resourceCenters.Count < 1)
            {
                Logger.Error("Unable to construct: {0}. No resource center was found.", GetUnitName(unitType));
                return;
            }

            // Trying to find a valid construction gas geyser.
            var gasGeysers = GetUnits(Units.GasGeysersAvail, onlyVisible: true, alliance: Alliance.Neutral);
            foreach (var gasGeyser in gasGeysers)
            {
                //Logger.Info("Gas Geyser Pos = {0}", gasGeyser.position);

                // Must have vespene.
                if (gasGeyser.vespene < 1)
                {
                    continue;
                }

                // Make sure it is in range of a resource center.
                if (!IsInRange(gasGeyser.position, resourceCenters, BUILD_RANGE_RADIUS))
                {
                    continue;
                }

                // Make sure it is not in use already.
                if (!CanPlace(unitType, gasGeyser.position))
                {
                    continue;
                }

                // Note: Might need this information later when dealing with the closest worker.
                Vector3 constructionSpot;
                constructionSpot = new Vector3(gasGeyser.position.X, gasGeyser.position.Y, 0);

                // Get the worker to create the structure.
                var worker = GetAvailableWorker();
                if (worker == null)
                {
                    Logger.Error("Unable to find worker to construct: {0}", GetUnitName(unitType));
                    return;
                }

                // Setup the construction command.
                var abilityID = Abilities.GetID(unitType);
                //Logger.Info("Ability ID: {0}", abilityID);
                var constructAction = CreateRawUnitCommand(abilityID);
                constructAction.ActionRaw.UnitCommand.UnitTags.Add(worker.tag);
                constructAction.ActionRaw.UnitCommand.TargetUnitTag = gasGeysers[0].tag;

                AddAction(constructAction);

                Logger.Info("Constructing: {0} @ {1} / {2}", GetUnitName(unitType), constructionSpot.X, constructionSpot.Y);
                return;
            }

            //Logger.Error("Unable to find gas geyser to build on.");
            return;
        }

        // Tell the passed units to attack at target loction.
        public void Attack(List<Unit> units, Vector3 target)
        {
            var action = CreateRawUnitCommand(Abilities.ATTACK);
            action.ActionRaw.UnitCommand.TargetWorldSpacePos = new Point2D();
            action.ActionRaw.UnitCommand.TargetWorldSpacePos.X = target.X;
            action.ActionRaw.UnitCommand.TargetWorldSpacePos.Y = target.Y;
            foreach (var unit in units)
                action.ActionRaw.UnitCommand.UnitTags.Add(unit.tag);
            AddAction(action);
        }

        /***********
         * Utilites
         ***********/

        // Pause the game.
        public void Pause()
        {
            Console.WriteLine("Press enter key to continue...");
            while (Console.ReadKey().Key != ConsoleKey.Enter)
            {
                //do nothing
            }
        }

    }
}

