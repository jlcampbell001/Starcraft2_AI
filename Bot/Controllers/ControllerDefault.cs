using Bot.Utilities;
using SC2APIProtocol;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using Action = SC2APIProtocol.Action;

namespace Bot
{
    // --------------------------------------------------------------------------------
    /// <summary>
    /// The default for controllers to be based on.
    /// </summary>
    // --------------------------------------------------------------------------------
    class ControllerDefault
    {
        private readonly int frameDelay = 0; //too fast? increase this to e.g. 20

        private const int MIN_WORKERS_MINING_PER_ACTIVE_GAS_BUILDING = 2;
        private const int BUILD_RANGE_RADIUS = 12;
        private const int PLACEMENT_TRIES = 1000;

        private readonly static List<Action> actions = new List<Action>();
        private readonly Random random = new Random();
        private const double FRAMES_PER_SECOND = 22.4;

        private bool gatheredInitalInfo = false;

        public static ResponseGameInfo gameInfo;
        public static ResponseData gameData;
        public static ResponseObservation obs;
        public ulong frame;
        public uint currentSupply;
        public uint maxSupply;
        public uint minerals;
        public uint vespene;
        public Vector3 playerStartLocation;

        public List<Vector3> enemyLocations = new List<Vector3>();
        public List<string> chatLog = new List<string>();
        public LocationsDistanceFromList expansionPositions;

        /**********
         * Frames
         **********/

        // ********************************************************************************
        /// <summary>
        /// Converts the passed seconds to frames.
        /// </summary>
        /// <param name="seconds">The seconds to convert.</param>
        /// <returns>The number of frames.</returns>
        // ********************************************************************************
        public ulong SecsToFrames(int seconds)
        {
            return (ulong)(FRAMES_PER_SECOND * seconds);
        }

        // ********************************************************************************
        /// <summary>
        /// Close the frame.
        /// </summary>
        /// <returns>The list of actions for the frame.</returns>
        // ********************************************************************************
        public List<Action> CloseFrame()
        {
            return actions;
        }

        // ********************************************************************************
        /// <summary>
        /// Open the frame.
        /// </summary>
        // ********************************************************************************
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
            if (!gatheredInitalInfo)
            {
                gatheredInitalInfo = true;

                var resourceCenters = GetUnits(Units.ResourceCenters);
                if (resourceCenters.Count > 0)
                {
                    playerStartLocation = resourceCenters[0].position;

                    foreach (var startLocation in gameInfo.StartRaw.StartLocations)
                    {
                        var enemyLocation = new Vector3(startLocation.X, startLocation.Y, 0);
                        var distance = Vector3.Distance(enemyLocation, playerStartLocation);
                        if (distance > 30)
                            enemyLocations.Add(enemyLocation);
                    }

                    // Get the possible base expansions positions.
                    SetExpansionPositions();
                }
            }

            if (frameDelay > 0)
                Thread.Sleep(frameDelay);
        }

        // ********************************************************************************
        /// <summary>
        /// Figure out the best place for base expansion points.
        /// </summary>
        /// <remarks>
        /// Note: This is really a rework of pysc2 version.
        /// </remarks>
        // ********************************************************************************
        private void SetExpansionPositions()
        {
            Logger.Info("Setting up expansion points...Please wait.");

            // Figure out the bot race's base resource center for later.
            var botRace = getBotRace();

            uint baseResourceCenter;
            if (botRace == Race.Terran)
            {
                baseResourceCenter = Units.COMMAND_CENTER;
            }
            else if (botRace == Race.Protoss)
            {
                baseResourceCenter = Units.NEXUS;
            }
            else
            {
                baseResourceCenter = Units.HATCHERY;
            }

            // Add the players start location as an expansion position in case their base there gets destroyed.
            expansionPositions = new LocationsDistanceFromList(playerStartLocation);

            var resources = GetUnits(Units.Resources, alliance: Alliance.Neutral);
            var resourceCenters = GetUnits(Units.ResourceCenters);

            // Get a list of resources that a near each other.
            List<ResourceCluster> initialResourceClusters = new List<ResourceCluster>();
            List<ResourceCluster> resourceClusters = new List<ResourceCluster>();

            foreach (var resource in resources)
            {
                var addedMineral = false;

                foreach (var resourceCluster in initialResourceClusters)
                {
                    // Maps do not have more then 10 mineral together.
                    if (resourceCluster.resources.Count == 10) continue;

                    // Note: This was the original code but the z axis is not the object sitting on the terrain so it is different for each object.
                    //if (Vector3.DistanceSquared(resource.position, resourceCluster.resources[0].position) < 225
                    //&& resource.position.Z == resourceCluster.resources[0].position.Z)

                    if (Vector3.DistanceSquared(resource.position, resourceCluster.resources[0].position) < 225)
                    {
                        resourceCluster.resources.Add(resource);
                        addedMineral = true;
                        break;
                    }
                }

                if (!addedMineral)
                {
                    ResourceCluster resourceCluster = new ResourceCluster();
                    resourceCluster.resources.Add(resource);
                    initialResourceClusters.Add(resourceCluster);
                }
            }

            // Remove all the single resource clusters.
            foreach (var resourceCluster in initialResourceClusters)
            {
                if (resourceCluster.resources.Count != 1)
                {
                    resourceClusters.Add(resourceCluster);
                }
            }

            // Setup the offsets.
            List<Vector2> offsets = new List<Vector2>();

            for (var x = -9; x < 11; x++)
            {
                for (var y = -9; y < 11; y++)
                {
                    var value = Math.Pow(x, 2) + Math.Pow(y, 2);
                    if (75 >= value && value >= 49)
                    {
                        Vector2 vector = new Vector2(x, y);
                        offsets.Add(vector);
                    }
                }
            }

            // Figure out the closest points to place a resource center to each cluster.
            foreach (var resourceCluster in resourceClusters)
            {
                List<LocationDistance> possiblePoints = new List<LocationDistance>();

                foreach (var resource in resourceCluster.resources)
                {
                    // Set up the minimum distance you can build to a resource.
                    var minDistance = 6.0;
                    if (Units.GasGeysers.Contains(resource.unitType))
                    {
                        minDistance = 7.0;
                    }

                    LocationDistance closestPoint = new LocationDistance();
                    closestPoint.location = Vector3.Zero;
                    closestPoint.distance = float.MaxValue;

                    // If there is a resource center, the resource this is most likely the starting position for the map so use it for the cluster.
                    var resourceCenter = GetFirstInRange(resource.position, resourceCenters, 12);

                    if (resourceCenter == null)
                    {
                        // Figure out the closest point to the resource based on the offsets setup above.
                        foreach (var offset in offsets)
                        {
                            Vector3 vector = new Vector3(offset.X + resource.position.X, offset.Y + resource.position.Y, resource.position.Z);


                            var distance = Vector3.Distance(resource.position, vector);
                            // Note: Need to figure a better way of doing this.  It takes 30 - 60 seconds total to check the canplace.
                            if (distance >= minDistance && closestPoint.distance >= distance && CanPlace(baseResourceCenter, vector))
                            {
                                closestPoint.location = vector;
                                closestPoint.distance = distance;
                            }
                        }
                    }
                    else
                    {
                        closestPoint.location = resourceCenter.position;
                        closestPoint.distance = resourceCenter.GetDistance(resource);
                    }

                    if (closestPoint.location != Vector3.Zero)
                    {
                        possiblePoints.Add(closestPoint);
                    }
                }

                // Figure out the 3 closest points to a resource cluster and record them for placement.
                if (possiblePoints.Count > 0)
                {
                    LocationDistance center = new LocationDistance();
                    center.location = Vector3.Zero;
                    center.distance = float.MaxValue;

                    LocationDistance center2 = new LocationDistance();
                    center2.location = Vector3.Zero;
                    center2.distance = float.MaxValue;

                    LocationDistance center3 = new LocationDistance();
                    center3.location = Vector3.Zero;
                    center3.distance = float.MaxValue;


                    foreach (var possiblePoint in possiblePoints)
                    {
                        if (center.distance > possiblePoint.distance)
                        {
                            center3 = center2;
                            center2 = center;
                            center = possiblePoint;

                        }
                    }

                    // Add a possible 3 locations per cluster in case something gets built in its spot.
                    if (center.location != Vector3.Zero)
                    {
                        expansionPositions.AddLocation(center.location, sortAfter: false);
                    }

                    if (center2.location != Vector3.Zero)
                    {
                        expansionPositions.AddLocation(center2.location, sortAfter: false);
                    }

                    if (center3.location != Vector3.Zero)
                    {
                        expansionPositions.AddLocation(center3.location, sortAfter: false);
                    }
                }
            }

            expansionPositions.UpdateDistances();
            Logger.Info("Finished setup expansion points.");
        }

        /**********
         * Player
         **********/

        // ********************************************************************************
        /// <summary>
        /// Get the race the bots is setup to use in game.
        /// </summary>
        /// <returns>The race of the bot.</returns>
        // ********************************************************************************
        public Race getBotRace()
        {
            var botRace = Race.NoRace;
            foreach (var player in gameInfo.PlayerInfo)
            {
                if (player.Type == PlayerType.Participant)
                {
                    botRace = player.RaceActual;
                }
            }
            return botRace;
        }

        /**********
         * Unit
         **********/

        // ********************************************************************************
        /// <summary>
        /// Get the unit name for the passed unit type.
        /// </summary>
        /// <param name="unitType">The unit type to look up.</param>
        /// <returns>The unit name.</returns>
        // ********************************************************************************
        public static string GetUnitName(uint unitType)
        {
            var unitName = "";

            if (unitType == Units.EXPANSION_BASE)
            {
                unitName = "Base Expansion";
            }
            else
            {
                unitName = gameData.Units[(int)unitType].Name;
            }

            return unitName;
        }

        // ********************************************************************************
        /// <summary>
        /// Get the unit that has the passed tag.
        /// </summary>
        /// <param name="tag">The tag of th unit to look for.</param>
        /// <returns>The unit if found or null if not found.</returns>
        // ********************************************************************************
        public Unit GetUnitByTag(ulong tag)
        {
            Unit foundUnit = null;

            foreach(var unit in obs.Observation.RawData.Units)
            {
                if (unit.Tag == tag)
                {
                    foundUnit = new Unit(unit);
                }
            }

            return foundUnit;
        }

        // ********************************************************************************
        /// <summary>
        /// Get a list of units based on a list of unit types.
        /// </summary>
        /// <param name="hashset">The hashset to search against.</param>
        /// <param name="alliance">The alliance the units are with.</param>
        /// <param name="onlyCompleted">Only units that have be completely built.</param>
        /// <param name="displayType">The visibly the units can be seen in.</param>
        /// <param name="hasVespene">Gas geysers that contain vespene.</param>
        /// <returns>The list of units.</returns>
        // ********************************************************************************
        public List<Unit> GetUnits(HashSet<uint> hashset, Alliance alliance = Alliance.Self, bool onlyCompleted = false, 
            DisplayType displayType = DisplayType.Unset, bool hasVespene = false)
        {
            // Ideally this should be cached in the future and cleared at each new frame.
            var units = new List<Unit>();
            foreach (var unit in obs.Observation.RawData.Units)
                if (hashset.Contains(unit.UnitType) && unit.Alliance == alliance)
                {
                    if (onlyCompleted && unit.BuildProgress < 1)
                        continue;

                    if (displayType != DisplayType.Unset && (unit.DisplayType != displayType))
                        continue;

                    if (hasVespene && Units.GasGeysers.Contains(unit.UnitType) && (unit.VespeneContents < 1))
                        continue;

                    units.Add(new Unit(unit));
                }
            return units;
        }

        // ********************************************************************************
        /// <summary>
        /// Get a list of units based on a single unit type.
        /// </summary>
        /// <param name="unitType">The unit type to look for.</param>
        /// <param name="alliance">The alliance the units are with.</param>
        /// <param name="onlyCompleted">Only units that have be completely built.</param>
        /// <param name="displayType">The visibly the units can be seen in.</param>
        /// <param name="hasVespene">Gas geysers that contain vespene.</param>
        /// <returns>The list of units.</returns>
        // ********************************************************************************
        public List<Unit> GetUnits(uint unitType, Alliance alliance = Alliance.Self, bool onlyCompleted = false,
            DisplayType displayType = DisplayType.Unset, bool hasVespene = false)
        {
            // Ideally this should be cached in the future and cleared at each new frame.
            var units = new List<Unit>();
            foreach (var unit in obs.Observation.RawData.Units)
                if (unit.UnitType == unitType && unit.Alliance == alliance)
                {
                    if (onlyCompleted && unit.BuildProgress < 1)
                        continue;

                    if (displayType != DisplayType.Unset && (unit.DisplayType != displayType))
                        continue;

                    if (hasVespene && (unit.VespeneContents < 1))
                        continue;
                    units.Add(new Unit(unit));
                }
            return units;
        }

        // ********************************************************************************
        /// <summary>
        /// Get a list of idle units from the passed list of units.
        /// </summary>
        /// <param name="units">The list of units to search against.</param>
        /// <returns>A list of idle units.</returns>
        // ********************************************************************************
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

        // ********************************************************************************
        /// <summary>
        /// Get all the units that can not see a resource center from the passed unit list.
        /// </summary>
        /// <param name="units">The list of units to search against.</param>
        /// <returns>A list of units that can not see the resource center.</returns>
        // ********************************************************************************
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

        // ********************************************************************************
        /// <summary>
        /// Get all the units that have vespene from the passed unit list.
        /// </summary>
        /// <param name="units">The list of units to search against.</param>
        /// <returns>The list of units with vespene.</returns>
        // ********************************************************************************
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

        // ********************************************************************************
        /// <summary>
        /// Get all the units that a targeting a certain unit from a passed unit list.
        /// </summary>
        /// <param name="units">The list of units search against.</param>
        /// <param name="tag">The target tag.</param>
        /// <returns>The list of units targeting the tag.</returns>
        // ********************************************************************************
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

        // ********************************************************************************
        /// <summary>
        /// Get a list of units that maybe attacking the passed unit. <para/>
        /// This is done by getting the attackers that the target is in sight of.
        /// </summary>
        /// <param name="target">The unit that might be a target.</param>
        /// <returns>A list of enemy units.</returns>
        // ********************************************************************************
        public List<Unit> GetPotentialAttackers(Unit target)
        {
            var enemyArmy = GetUnits(Units.ArmyUnits, alliance: Alliance.Enemy);

            List<Unit> enemies = new List<Unit>();

            foreach(var enemy in enemyArmy)
            {
                if (enemy.GetDistance(target) <= enemy.sight)
                {
                    enemies.Add(enemy);
                }
            }

            return enemies;
        }

        // ********************************************************************************
        /// <summary>
        /// Get a list of units that maybe attacking the passed unit list. <para/>
        /// This is done by getting the attackers that the target is in sight of.
        /// </summary>
        /// <param name="targets">A list of target units.</param>
        /// <returns>A list of enemy units.</returns>
        // ********************************************************************************
        public List<Unit> GetPotentialAttackers(List<Unit> targets)
        {
            List<Unit> enemies = new List<Unit>();

            foreach (var target in targets)
            {
                var targetEnemies = GetPotentialAttackers(target);

                if (targetEnemies.Count > 0)
                {
                    enemies.AddRange(targetEnemies);
                }
            }

            return enemies;
        }

        // ********************************************************************************
        /// <summary>
        /// Get all the units that are gathering from the target tag from a passed unit list.
        /// </summary>
        /// <param name="units">A list of units to search.</param>
        /// <param name="tag">The target tag to look for.</param>
        /// <returns>A list of units found.</returns>
        // ********************************************************************************
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

        // ********************************************************************************
        /// <summary>
        /// Check if the passed unit is in range of the target position based on the provided distance.
        /// </summary>
        /// <param name="targetPosition">The target position.</param>
        /// <param name="units">A list of units to search.</param>
        /// <param name="maxDistance">The farthest distance the unit can be.</param>
        /// <returns>True if the unit is within range.</returns>
        // ********************************************************************************
        public bool IsInRange(Vector3 targetPosition, List<Unit> units, float maxDistance)
        {
            return (GetFirstInRange(targetPosition, units, maxDistance) != null);
        }

        // ********************************************************************************
        /// <summary>
        /// Get the first unit that is in range of the target position based on the provided distance.
        /// </summary>
        /// <param name="targetPosition">The target position.</param>
        /// <param name="units">A list of units to search.</param>
        /// <param name="maxDistance">The farthest distance the unit can be.</param>
        /// <returns>The first unit found or null if none are found.</returns>
        // ********************************************************************************
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

        // ********************************************************************************
        /// <summary>
        /// Get the next unit that is in range of the target position based on the provided distance. <para/>
        /// Needs to know the last unit that was found other wise it will return the first unit it can find.
        /// </summary>
        /// <param name="targetPosition">The target position.</param>
        /// <param name="units">A list of units to search.</param>
        /// <param name="maxDistance">The farthest distance the unit can be.</param>
        /// <param name="lastFound">The last unit that was found.</param>
        /// <returns>The next unit found or null if none was found.</returns>
        // ********************************************************************************
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

        // ********************************************************************************
        /// <summary>
        /// Get a worker that is either doing nothing or just gathering minerals.
        /// </summary>
        /// <returns>The worker unit found or null if none was found.</returns>
        // ********************************************************************************
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

        // ********************************************************************************
        /// <summary>
        /// Check if the player can afford the unit and return by reference the cost.
        /// </summary>
        /// <param name="unitType">The unit type to look up.</param>
        /// <param name="unitMinerals">The minerals required for the unit.</param>
        /// <param name="unitVespene">The vespene required for the unit.</param>
        /// <returns>True if the bot has enough resources for the unit.</returns>
        // ********************************************************************************
        public virtual bool CanAfford(uint unitType, ref int unitMinerals, ref int unitVespene)
        {
            var unitData = gameData.Units[(int)unitType];
            unitMinerals = (int)unitData.MineralCost;
            unitVespene = (int)unitData.VespeneCost;
            return (minerals >= unitMinerals) && (vespene >= unitVespene);
        }

        // ********************************************************************************
        /// <summary>
        /// Check if the player can afford the unit.
        /// </summary>
        /// <param name="unitType">The unit type to look up.</param>
        /// <returns>True if the bot has enough resources for the unit.</returns>
        // ********************************************************************************
        public virtual bool CanAfford(uint unitType)
        {
            var mineralCost = 0;
            var vespeneCost = 0;
            return CanAfford(unitType, ref mineralCost, ref vespeneCost);
        }

        // ********************************************************************************
        /// <summary>
        /// Check and see if the unit can be placed in the target position.
        /// </summary>
        /// <param name="unitType">The unit type that is going to be placed.</param>
        /// <param name="targetPos">The target position to check.</param>
        /// <returns>True if the unit can be placed in the target position.</returns>
        // ********************************************************************************
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

        // ********************************************************************************
        /// <summary>
        /// Get the total number of units in game for a passed unit type.
        /// </summary>
        /// <param name="unitType">The unit type to count.</param>
        /// <param name="inConstruction">Add units that are being built.</param>
        /// <returns>The total units found.</returns>
        // ********************************************************************************
        public int GetTotalCount(uint unitType, bool inConstruction = false)
        {
            var pendingCount = GetPendingCount(unitType, inConstruction);
            var constructionCount = GetUnits(unitType).Count;
            return pendingCount + constructionCount;
        }

        // ********************************************************************************
        /// <summary>
        /// Get the total number of units in game for a passed list of unit type.
        /// </summary>
        /// <param name="unitType">A hashset of unit types to look up.</param>
        /// <param name="inConstruction">Add units that are being built.</param>
        /// <returns>The total units found.</returns>
        // ********************************************************************************
        public int GetTotalCount(HashSet<uint> unitType, bool inConstruction = false)
        {
            var pendingCount = GetPendingCount(unitType, inConstruction);
            var constructionCount = GetUnits(unitType).Count;
            return pendingCount + constructionCount;
        }

        // ********************************************************************************
        /// <summary>
        /// Get the total number of units pending in game for a passed unit type.
        /// </summary>
        /// <param name="unitType">The unit type to look for.</param>
        /// <param name="inConstruction">Only units that are being built.</param>
        /// <returns>The total units found.</returns>
        // ********************************************************************************
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

        // ********************************************************************************
        /// <summary>
        /// Get the total number of units pending in game for a passed list of unit type.
        /// </summary>
        /// <param name="unitTypes">A hashset of unit types to look for.</param>
        /// <param name="inConstruction">Only units that are being built.</param>
        /// <returns>The total units found.</returns>
        // ********************************************************************************
        public int GetPendingCount(HashSet<uint> unitTypes, bool inConstruction = true)
        {
            var workers = GetUnits(Units.Workers);
            var counter = 0;

            foreach (var unitType in unitTypes)
            {
                var abilityID = Abilities.GetID(unitType);


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
            }

            return counter;
        }

        // ********************************************************************************
        /// <summary>
        /// Get the total resource centers.
        /// </summary>
        /// <returns>The total resource centers found.</returns>
        // ********************************************************************************
        public int GetTotalRCs()
        {
            var resourceCenters = GetUnits(Units.ResourceCenters, onlyCompleted: true);
            return resourceCenters.Count;
        }

        // ********************************************************************************
        /// <summary>
        /// Get the closest unit to the target unit from the passed unit list.
        /// </summary>
        /// <param name="target"> The unit to check distances to.</param>
        /// <param name="units">List of units to check for.</param>
        /// <param name="withInDistance">The max distance allowed. 0 means any distance.</param>
        /// <param name="isNotBurrowed">If true make sure it is not burrowed.</param>
        /// <returns>The closest unit of null if none is found.</returns>
        // ********************************************************************************
        public Unit GetClosestUnit(Unit target, List<Unit> units, double withInDistance = 0, bool isNotBurrowed = false)
        {
            Unit closestUnit = null;

            if (units.Count > 0)
            {
                UnitsDistanceFromList unitsDistanceFromList = new UnitsDistanceFromList(target.position);
                unitsDistanceFromList.AddUnits(units);

                foreach (var unit in unitsDistanceFromList.toUnits)
                {
                    var foundUnit = false;

                    if (withInDistance > 0)
                    {
                        if (unit.distance <= withInDistance)
                        {
                            foundUnit = true;
                        }
                        else
                        {
                            // No units are withing range.
                            break;
                        }
                    }
                    else
                    {
                        foundUnit = true;
                    }

                    if (foundUnit && isNotBurrowed && unit.unit.isBurrowed)
                    {
                        foundUnit = false;
                    }

                    // Found the closest unit get out.
                    if (foundUnit)
                    {
                        closestUnit = unit.unit;
                        break;
                    }
                }
            }

            return closestUnit;
        }

        // ********************************************************************************
        /// <summary>
        /// Get the closest unit to the target unit from the passed hashset.
        /// </summary>
        /// <param name="target">The unit to check distances to.</param>
        /// <param name="unitTypes">The unit hashset to check for.</param>
        /// <param name="withInDistance">The max distance allowed. 0 means any distance.</param>
        /// <param name="isNotBurrowed">If true make sure it is not burrowed.</param>
        /// <returns>The closest unit of null if none is found.</returns>
        // ********************************************************************************
        public Unit GetClosestUnit(Unit target, HashSet<uint> unitTypes, double withInDistance = 0, bool isNotBurrowed = false)
        {
            var units = GetUnits(unitTypes);

            return GetClosestUnit(target, units, withInDistance, isNotBurrowed);
        }

        // ********************************************************************************
        /// <summary>
        /// Get the closest unit to the target unit from the passed unit type.
        /// </summary>
        /// <param name="target">The unit to check distances to.</param>
        /// <param name="unitTypes">The unit type to check for.</param>
        /// <param name="withInDistance">The max distance allowed. 0 means any distance.</param>
        /// <param name="isNotBurrowed">If true make sure it is not burrowed.</param>
        /// <returns>The closest unit of null if none is found.</returns>
        // ********************************************************************************
        public Unit GetClosestUnit(Unit target, uint unitType, double withInDistance = 0, bool isNotBurrowed = false)
        {
            var units = GetUnits(unitType);

            return GetClosestUnit(target, units, withInDistance, isNotBurrowed);
        }

        // ********************************************************************************
        /// <summary>
        /// Check and see if the bot has any of the passed completed units.
        /// </summary>
        /// <param name="unitType">The unit type to look up.</param>
        /// <returns>True if units are found.</returns>
        // ********************************************************************************
        public bool HasUnits(uint unitType)
        {
            var foundUnits = GetUnits(unitType, onlyCompleted: true);

            return (foundUnits.Count > 0);
        }

        // ********************************************************************************
        /// <summary>
        /// Check and see if the bot has any of the passed completed units.
        /// </summary>
        /// <param name="unitTypes">A hashset of unit types to look for.</param>
        /// <returns>True if units are found.</returns>
        // ********************************************************************************
        public bool HasUnits(HashSet<uint> unitTypes)
        {
            var foundUnits = GetUnits(unitTypes, onlyCompleted: true);

            return (foundUnits.Count > 0);
        }

        // ********************************************************************************
        /// <summary>
        /// Check and see if a unit is gathering vespene.
        /// </summary>
        /// <param name="minWorkersGathering"> The minimum number of units need to be gathering vespene.</param>
        /// <returns>True if the number of workers gathering vespene is matched.</returns>
        // ********************************************************************************
        public bool IsGatheringVespene(int minWorkersGathering = 1)
        {
            var gasBuildings = GetUnits(Units.GasGeysersStructures);
            var totalWorkersAssigned = 0;
            foreach (var gasBuilding in gasBuildings)
            {
                totalWorkersAssigned = totalWorkersAssigned + gasBuilding.assignedWorkers;
            }

            return (totalWorkersAssigned >= minWorkersGathering);
        }

        /**********
         * Abilities
         **********/

        // ********************************************************************************
        /// <summary>
        /// Get the ability name for the passed ability id.
        /// </summary>
        /// <param name="abilityID">The ability ID to look for.</param>
        /// <returns>The name of the ability.</returns>
        // ********************************************************************************
        public static string GetAbilityName(int abilityID)
        {
            var abilityName = "";

            abilityName = gameData.Abilities[abilityID].FriendlyName;
            
            return abilityName;
        }

        // ********************************************************************************
        /// <summary>
        /// Checks to see if the passed unit is ordered to preform an ability.
        /// </summary>
        /// <param name="unit">The unit to check.</param>
        /// <param name="abilityID">The ability to check for.</param>
        /// <returns>True if the unit is ordered to do the ability.</returns>
        // ********************************************************************************
        public bool IsOrderedTo(Unit unit, int abilityID)
        {
            var isOrderedTo = false;

            foreach(var order in unit.orders)
            {
                if (order.AbilityId == abilityID)
                {
                    isOrderedTo = true;
                    break;
                }
            }

            return isOrderedTo;
        }

        // ********************************************************************************
        /// <summary>
        /// Check and see if we have the resources for the passed upgrade and also return the resources costs.
        /// </summary>
        /// <param name="abilityID">The upgrade id.</param>
        /// <param name="mineralCost">The mineral cost for the upgrade.</param>
        /// <param name="vespeneCost">The vespene cost for the upgrade.</param>
        /// <returns>True if they can afford the upgrade.</returns>
        // ********************************************************************************
        public virtual bool CanAffordUpgrade(int abilityID, ref int mineralCost, ref int vespeneCost)
        {
            var canAfford = false;

            foreach (var upgrade in gameData.Upgrades)
            {
                if (upgrade.AbilityId == abilityID)
                {
                    mineralCost = (int)upgrade.MineralCost;
                    vespeneCost = (int)upgrade.VespeneCost;

                    if (minerals >= mineralCost && vespene >= vespeneCost)
                    {
                        canAfford = true;
                        break;
                    }
                }
            }

            return canAfford;
        }

        // ********************************************************************************
        /// <summary>
        /// Check and see if we have the resources for the passed upgrade.
        /// </summary>
        /// <param name="abilityID">The upgrade id.</param>
        /// <returns>True if they can afford the upgrade.</returns>
        // ********************************************************************************
        public bool CanAffordUpgrade(int abilityID)
        {
            var mineralCost = 0;
            var vespenCost = 0;

            return CanAffordUpgrade(abilityID, ref mineralCost, ref vespenCost);
        }

        // ********************************************************************************
        /// <summary>
        /// Check and see if the upgrade has been researched.
        /// </summary>
        /// <param name="abilityID">The upgrade id.</param>
        /// <returns>True if the upgrade has already been researched.</returns>
        // ********************************************************************************
        public bool HasUpgrade(int abilityID)
        {
            var hasAbility = false;
            if (obs.Observation.RawData.Player.UpgradeIds.Contains((uint)abilityID))
            {
                hasAbility = true;
            }
            return hasAbility;
        }

        // ********************************************************************************
        /// <summary>
        /// Check and see if any of the passed units are researching an upgrade.
        /// </summary>
        /// <param name="abilityID">The upgrade id.</param>
        /// <param name="units">The list of units to search.</param>
        /// <returns>True if a unit is researching the upgrade.</returns>
        // ********************************************************************************
        public bool IsResearchingUpgrade(int abilityID, List<Unit> units)
        {
            var isResearching = false;

            foreach(var unit in units)
            {
                if (IsOrderedTo(unit, abilityID)) {
                    isResearching = true;
                    break;
                }
            }

            return isResearching;
        }

        // ********************************************************************************
        /// <summary>
        /// Check and see if any of the passed unit types are researching an upgrade.
        /// </summary>
        /// <param name="abilityID">The upgrade id.</param>
        /// <param name="unitTypes">A hashset of unit types to search.</param>
        /// <returns>True if a unit is researching the upgrade.</returns>
        // ********************************************************************************
        public bool IsResearchingUpgrade(int abilityID, HashSet<uint> unitTypes)
        {
            var units = GetUnits(unitTypes);

            return IsResearchingUpgrade(abilityID, units);
        }

        // ********************************************************************************
        /// <summary>
        /// Check and see if any of the passed unit type are researching an upgrade.
        /// </summary>
        /// <param name="abilityID">The upgrade id.</param>
        /// <param name="unitType">A unit type to search.</param>
        /// <returns>True if a unit is researching the upgrade.</returns>
        // ********************************************************************************
        public bool IsResearchingUpgrade(int abilityID, uint unitType)
        {
            var units = GetUnits(unitType);

            return IsResearchingUpgrade(abilityID, units);
        }

        // ********************************************************************************
        /// <summary>
        /// Check and see if any unit type is researching an upgrade.
        /// </summary>
        /// <param name="abilityID">The upgrade id.</param>
        /// <returns>True if a unit is researching the upgrade.</returns>
        // ********************************************************************************
        public bool IsResearchingUpgrade(int abilityID)
        {
            var units = GetUnits(Units.All);

            return IsResearchingUpgrade(abilityID, units);
        }


        /**********
         * Actions
         **********/

        // ********************************************************************************
        /// <summary>
        /// Create a raw unit command action for the passed ability.
        /// </summary>
        /// <param name="ability">The ability to create a command for.</param>
        /// <returns>The created action.</returns>
        // ********************************************************************************
        public static Action CreateRawUnitCommand(int ability)
        {
            var action = new Action();
            action.ActionRaw = new ActionRaw();
            action.ActionRaw.UnitCommand = new ActionRawUnitCommand();
            action.ActionRaw.UnitCommand.AbilityId = ability;
            return action;
        }

        // ********************************************************************************
        /// <summary>
        /// Create a toggle auto cast action for the passed ability.
        /// </summary>
        /// <param name="ability">The ability to auto cast.</param>
        /// <returns>The auto cast action for the ability.</returns>
        // ********************************************************************************
        public static Action CreateToggleAutoCast(int ability)
        {
            var action = new Action();
            action.ActionRaw = new ActionRaw();
            action.ActionRaw.ToggleAutocast = new ActionRawToggleAutocast();
            action.ActionRaw.ToggleAutocast.AbilityId = ability;
            return action;
        }

        // ********************************************************************************
        /// <summary>
        /// Add an action to the action list.
        /// </summary>
        /// <param name="action">The actions to add.</param>
        // ********************************************************************************
        public static void AddAction(Action action)
        {
            actions.Add(action);
        }

        // ********************************************************************************
        /// <summary>
        /// Distribute the workers as needed.
        /// </summary>
        // ********************************************************************************
        public void DistributeWorkers()
        {
            var resourceRange = 12;
            var gasBuildings = GetUnits(Units.GasGeysersStructures, onlyCompleted: true, hasVespene: true);
            var resourceCenters = GetUnits(Units.ResourceCenters, onlyCompleted: true);
            var mineralFields = GetUnits(Units.MineralFields, displayType: DisplayType.Visible, alliance: Alliance.Neutral);
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

                // Check to see if there are any gas geysers structures not full of workers that still have gas (see declaring gas building list).
                // This will set the max workers gather gas based on how many should be mining.
                // I.E.: If the min is set at 2 (default) and there are 2 gas buildings and there are 16 workers the answer should be 2 workers to assign to each gas building.
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

                    // If the workers per gas building is 0, then check if the are at least 3 more workers then builds and if this is so set it to 1.
                    // This is in case there are a lot of buildings that were made.
                    if (workersPerGB == 0)
                    {
                        if (workers.Count - gasBuildings.Count > 2)
                        {
                            workersPerGB = 1;
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
                                // Remove all the workers it can find.  If is lowers it to below needed the next step will fill it back up.
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

        // ********************************************************************************
        /// <summary>
        /// Construct a structure based on a location not a target. <para/>
        /// It is possible to pass the starting position.
        /// </summary>
        /// <param name="unitType">The unit type to construct.</param>
        /// <param name="startingLocation">If passed a staring location to construct near.</param>
        /// <param name="buildRangeRadius">The build range radius to work within.</param>
        // ********************************************************************************
        public void Construct(uint unitType, Vector3 startingLocation, int buildRangeRadius = BUILD_RANGE_RADIUS)
        {
            Vector3 startingSpot;

            if (startingLocation != Vector3.Zero)
            {
                startingSpot = startingLocation;
            }
            else
            {
                // Use the first resource centers as a starting point.

                bool completed = false;

                // Zerg need completed resource centers so there is creep to build on.
                if (getBotRace() == Race.Zerg)
                {
                    completed = true;
                }
                // Note: Need to re-work this to handle multiple resource centers and to get a starting point if there are no resource centers.
                var resourceCenters = GetUnits(Units.ResourceCenters, onlyCompleted: completed);
                if (resourceCenters.Count > 0)
                    startingSpot = resourceCenters[0].position;
                else
                {
                    if (Units.ResourceCenters.Contains(unitType))
                    {
                        startingSpot = playerStartLocation;
                    }
                    else
                    {
                        Logger.Error("Unable to construct: {0}. No resource center was found.", GetUnitName(unitType));
                        return;
                    }
                }
            }
            // Trying to find a valid construction spot
            var mineralFields = GetUnits(Units.MineralFields, displayType: DisplayType.Visible, alliance: Alliance.Neutral);
            Vector3 constructionSpot;

            var i = 0;
            while (true)
            {
                i++;

                if (i == PLACEMENT_TRIES)
                {
                    // There might be something wrong or there is no place left to place the structure.
                    Logger.Error("After {0} tries we could not find a spot to place {1}.", PLACEMENT_TRIES, GetUnitName(unitType));
                    return;
                }

                //constructionSpot = new Vector3(startingSpot.X + random.Next(-buildRangeRadius, buildRangeRadius + 1),
                //startingSpot.Y + random.Next(-buildRangeRadius, buildRangeRadius + 1), 0);

                constructionSpot = GetRandomLocation(startingSpot, -buildRangeRadius, buildRangeRadius, -buildRangeRadius, buildRangeRadius);

                // Avoid building in the mineral line.
                // Note:  Need to re-work this as it still happens and I need it for gas structures.
                if (IsInRange(constructionSpot, mineralFields, 5)) continue;

                // Check if the building fits.
                if (!CanPlace(unitType, constructionSpot)) continue;

                // OK, we found a spot.
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

        // ********************************************************************************
        /// <summary>
        /// Construct a structure based on a location not a target.
        /// </summary>
        /// <param name="unitType">The unit type to construct.</param>
        // ********************************************************************************
        public void Construct(uint unitType)
        {
            Construct(unitType, Vector3.Zero);
        }

        // ********************************************************************************
        /// <summary>
        /// Construct a structure on a gas geyser. <para/>
        /// This is needed because to can not use the gas geyser location but need to target the gas geyser to build on it.
        /// </summary>
        /// <remarks>
        /// Note: This is similar to construct so see its notes for possible changes.
        /// </remarks>
        /// <param name="unitType">The unit type to build.</param>
        // ********************************************************************************
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
            var gasGeysers = GetUnits(Units.GasGeysersAvail, displayType: DisplayType.Visible, alliance: Alliance.Neutral);
            foreach (var gasGeyser in gasGeysers)
            {
                //Logger.Info("Gas Geyser Position = {0}", gasGeyser.position);

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

        // ********************************************************************************
        /// <summary>
        /// Build an expansion at the next available mineral field. <para/>
        /// Pass in the recourse center to build.
        /// </summary>
        /// <param name="unitType">The unit type to build at the expansion spot.</param>
        // ********************************************************************************
        public void BuildExpansion(uint unitType)
        {
            var resources = GetUnits(Units.Resources, alliance: Alliance.Neutral, hasVespene: true);

            var resourceCenters = GetUnits(Units.ResourceCenters);

            // Find a mineral field that is not within sight of the basic resource center.
            var basicResourceSight = gameData.Units[(int)unitType].SightRange;

            foreach (var expansionPosition in expansionPositions.toLocations)
            {
                var resouceCenter = GetFirstInRange(expansionPosition.location, resourceCenters, basicResourceSight);

                if (resouceCenter != null) continue;

                var resource = GetFirstInRange(expansionPosition.location, resources, basicResourceSight);

                if (resource == null) continue;

                if (!CanPlace(unitType, expansionPosition.location)) continue;

                var overlords = GetUnits(Units.OverLordsAndOverseers);

                overlords[0].Move(expansionPosition.location);

                Logger.Info("Placing Expansion @ {0}.", expansionPosition.location);

                //Construct(unitType, expansionPosition.location, (int)basicResourceSight);
                Construct(unitType, expansionPosition.location, 1);

                break;
            }
        }

        // ********************************************************************************
        /// <summary>
        /// Tell the passed units to attack at target location.
        /// </summary>
        /// <param name="units">A list of units to attack with.</param>
        /// <param name="target">The target position to attack.</param>
        // ********************************************************************************
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
         * Utilities
         ***********/

        // ********************************************************************************
        /// <summary>
        /// Pause the game.
        /// </summary>
        // ********************************************************************************
        public void Pause()
        {
            Console.WriteLine("Press enter key to continue...");
            while (Console.ReadKey().Key != ConsoleKey.Enter)
            {
                //do nothing
            }
        }

        // ********************************************************************************
        /// <summary>
        /// Write to the log if the passed unit is currently selected.
        /// </summary>
        /// <param name="unit">The unit to check.</param>
        /// <param name="line">The line to write in the log.</param>
        /// <param name="parameters">Additional parameters for the log.</param>
        // ********************************************************************************
        public void LogIfSelectedUnit(Unit unit, string line, params object[] parameters)
        {
            if (unit != null && unit.isSelected)
            {
                Logger.Info(line, parameters);
            }
        }

        // ********************************************************************************
        /// <summary>
        /// Get a random spot around a starting position.
        /// </summary>
        /// <remarks>
        /// Note: Need some error checking to make sure the min are lower then the maxes.
        /// </remarks>
        /// <param name="startingLocation">The starting location to work with.</param>
        /// <param name="xMinRange">Minimum X range.</param>
        /// <param name="xMaxRange">Maximum X range.</param>
        /// <param name="yMinRange">Minimum Y range.</param>
        /// <param name="yMaxRange">Maximum Y range.</param>
        /// <returns>A random position around the start location with in the ranges.</returns>
        // ********************************************************************************
        public Vector3 GetRandomLocation(Vector3 startingLocation, int xMinRange = 0, int xMaxRange = 0, int yMinRange = 0, int yMaxRange = 0)
        {
            var randomLocation = new Vector3(startingLocation.X + random.Next(xMinRange, xMaxRange + 1),
                    startingLocation.Y + random.Next(yMinRange, yMaxRange + 1), 0);

            return randomLocation;
        }

        // ********************************************************************************
        /// <summary>
        /// A list of actions for the frame.
        /// </summary>
        /// <returns>A list of actions.</returns>
        // ********************************************************************************
        public List<Action> GetActions()
        {
            return actions;
        }
    }
}

