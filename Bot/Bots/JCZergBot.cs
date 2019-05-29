using System.Collections.Generic;
using SC2APIProtocol;

namespace Bot
{
    internal class JCZergBot : Bot
    {
        private readonly bool totalRandom = true;

        private const int WAIT_IN_SECONDS = 1;

        private const int MAX_SPAWNING_POOLS = 2;
        private const int MAX_HYDRALISK_DENS = 1;
        private const int MAX_SPIRES = 1;

        private const int BUILD_OVERLORD_RANGE = 2;
        private const int DRONE_PER_RC = 19;
        private const int DRONE_MIN_TO_AUTO_SAVE_RESOUCES = 10;

        private const int CHANCE_BUILD_DEF_BUILDING = 10;
        private const int SPINE_PER_RC = 10;
        private const int SPORE_PER_RC = 10;

        private const int ATTACK_ARMY_PER_MIN = 5;
        private const int ATTACK_ARMY_MAX = 50;

        private const int ZERGLINGS_PER_HYDRALISK = 4;
        private const int ZERGLINGS_PER_MUTALISKS = 3;

        private const int CHANCE_TO_SAVE_RESOURCES = 1;

        private readonly System.Random random = new System.Random();

        private int gameMin = -1;
        private ulong nextWaitFrame = 0;

        private uint saveForUnitType = 0;
        private int saveForMinerals = 0;
        private int saveForVespene = 0;

        private ZergController controller = new ZergController();

        public IEnumerable<Action> OnFrame()
        {
            controller.OpenFrame();

            if (controller.frame == 0)
            {
                Logger.Info("JCZergBot");
                Logger.Info("--------------------------------------");
                Logger.Info("Map: {0}", ZergController.gameInfo.MapName);
                Logger.Info("--------------------------------------");
            }

            /*
            if (controller.frame == 1)
            {
                foreach (var unit in Units.Zerg)
                {
                    controller.CanAfford(unit);
                }
            }
            */

            //distribute workers optimally every 10 frames
            if (controller.frame % 10 == 0)
            {
                controller.DistributeWorkers();
            }

            if (controller.frame % (22 * 60) == 0)
            {
                gameMin++;
            }

            if (controller.minerals >= saveForMinerals && controller.vespene >= saveForVespene)
            {
                // Construct / Train saved for unit.
                if (saveForUnitType != 0)
                {
                    CreateSavedForUnit();
                }
            }
            else
            {
                // There are no workers to gather.
                var workers = controller.GetUnits(Units.Workers);
                if (workers.Count == 0)
                {
                    Logger.Info("No workers to gather resources to save for {0}.", ControllerDefault.GetUnitName(saveForUnitType));
                    SetSaveResouces();
                }

                // Make sure there is enough minerals to gather.
                if (saveForMinerals > 0)
                {
                    var mineralFields = controller.GetUnits(Units.MineralFields, alliance: Alliance.Neutral, onlyVisible: true);
                    var totalMineralsLeft = 0;

                    //Logger.Info("Visible mineral fields = {0}", mineralFields.Count);

                    foreach (var mineralField in mineralFields)
                    {
                        totalMineralsLeft = totalMineralsLeft + mineralField.minerals;
                    }

                    if (totalMineralsLeft < saveForMinerals)
                    {
                        Logger.Info("Not enough minerals left: {0} left but need {1}", totalMineralsLeft, saveForMinerals);
                        SetSaveResouces();
                    }
                }

                // Make sure there is enough vespene to gather.
                if (saveForVespene > 0)
                {
                    var gasBuildings = controller.GetUnits(Units.GasGeysersStructures);
                    var totalVespeneLeft = 0;
                    foreach (var gasBuilding in gasBuildings)
                    {
                        totalVespeneLeft = totalVespeneLeft + gasBuilding.vespene;
                    }

                    if (totalVespeneLeft < saveForVespene)
                    {
                        Logger.Info("Not enough vespene left: {0} left but need {1}", totalVespeneLeft, saveForVespene);
                        SetSaveResouces();
                    }
                }
            }

            if (controller.frame > nextWaitFrame)
            {
                if (totalRandom)
                {
                    var randAction = random.Next(4);

                    switch (randAction)
                    {
                        case 0:
                            nextWaitFrame = nextWaitFrame + controller.SecsToFrames(WAIT_IN_SECONDS);
                            break;
                        case 1:
                            BuildBuildingsRandom();
                            break;
                        case 2:
                            BuildUnitsRandom();
                            break;
                        case 3:
                            UnitActionsRandom();
                            break;
                    }

                }
                else
                {
                    var randAction = random.Next(100);
                    //Logger.Info("Random = {0}", randAction);

                    if (randAction < 10)
                    {
                        nextWaitFrame = nextWaitFrame + controller.SecsToFrames(WAIT_IN_SECONDS * random.Next(1, 10));
                    }
                    else if (randAction < 70)
                    {
                        BuildBuildings();
                    }
                    else if (randAction < 90)
                    {
                        BuildUnits();
                    }
                    else
                    {
                        UnitActions();
                    }
                }
            }

            return controller.CloseFrame();
        }

        private void BuildBuildings()
        {
            if (controller.minerals < saveForMinerals || controller.vespene < saveForVespene)
            {
                return;
            }

            if (controller.GetTotalCount(Units.SPAWNING_POOL, inConstruction: true) < MAX_SPAWNING_POOLS)
            {
                //Logger.Info("Build SP");
                BuildBuilding(Units.SPAWNING_POOL);
            }

            if (controller.vespene < 1)
            {
                //Logger.Info("Build EX");
                BuildExtractor();
            }
            else
            {
                //Logger.Info("Build UL");
                UpgradeToLair();
            }

            if (controller.GetTotalCount(Units.HYDRALISK_DEN, inConstruction: true) < MAX_HYDRALISK_DENS)
            {
                //Logger.Info("Build HD");
                BuildBuilding(Units.HYDRALISK_DEN);
            }

            if (controller.GetTotalCount(Units.SPIRE, inConstruction: true) < MAX_SPIRES)
            {
                //Logger.Info("Build S");
                BuildBuilding(Units.SPIRE);
            }

            var randDef = random.Next(100);

            if (randDef < CHANCE_BUILD_DEF_BUILDING)
            {
                var randCrawler = random.Next(100);
                //Logger.Info("Def = {0};   Crawler = {1}", randDef, randCrawler);

                if (randCrawler < 50 && controller.GetTotalCount(Units.SPINE_CRAWLER) < SPINE_PER_RC * GetTotalRCs())
                {
                    BuildBuilding(Units.SPINE_CRAWLER);
                }
                else if (controller.GetTotalCount(Units.SPORE_CRAWLER) < SPORE_PER_RC * GetTotalRCs())
                {
                    BuildBuilding(Units.SPORE_CRAWLER);
                }
            }
        }

        private void BuildUnits()
        {
            if (controller.minerals < saveForMinerals || controller.vespene < saveForVespene)
            {
                return;
            }

            var totalZergling = controller.GetTotalCount(Units.ZERGLING);

            // So no divide by 0 and this sould make them make a zergling.
            //if (totalZergling == 0) totalZergling = 1;

            if (controller.maxSupply - controller.currentSupply <= BUILD_OVERLORD_RANGE)
            {
                BuildOverlord();
            }
            else if (controller.GetTotalCount(Units.DRONE) < (DRONE_PER_RC * GetTotalRCs()))
            {
                BuildUnit(Units.DRONE);
            }
            else if (controller.GetTotalCount(Units.HYDRALISK_DEN) > 0 && controller.GetTotalCount(Units.HYDRALISK) < totalZergling / ZERGLINGS_PER_HYDRALISK)
            {
                BuildUnit(Units.HYDRALISK);
            }
            else if (controller.GetTotalCount(Units.SPIRE) > 0 && controller.GetTotalCount(Units.MUTALISK) < totalZergling / ZERGLINGS_PER_MUTALISKS)
            {
                BuildUnit(Units.MUTALISK);
            }
            else
            {
                BuildUnit(Units.ZERGLING);
            }
        }

        private void UnitActions()
        {
            var army = controller.GetIdleUnits(controller.GetUnits(Units.ArmyUnits));
            var enemyBuildings = controller.GetUnits(Units.Structures, alliance: Alliance.Enemy, onlyVisible: true);
            var resourceCenters = controller.GetUnits(Units.ResourceCenters);

            var canNotSeeRCArmy = controller.GetUnitsNoInSightOfRC(army);

            if (canNotSeeRCArmy.Count > 0 && enemyBuildings.Count == 0)
            {
                //Logger.Info("Unit = {0};  Sight = {1}", canNotSeeRCArmy[0].name, canNotSeeRCArmy[0].sight);
                RecallIdleUnits();
            }
            else if (army.Count > ATTACK_ARMY_MAX || army.Count > ATTACK_ARMY_PER_MIN * gameMin)
            {
                if (enemyBuildings.Count > 0)
                {
                    AttackEnemyStructure();
                }
                else
                {
                    AttackEnemyBase();
                }
            }
        }

        // -----------------------------------------------BUILDINGS---------------------------------------------

        private void BuildBuilding(uint unitType, bool saveFor = true)
        {
            //Logger.Info("Trying to build {0}.", ControllerDefault.GetUnitName(unitType));

            if (controller.CanConstruct(unitType))
            {
                //Logger.Info("Can build {0}.", ControllerDefault.GetUnitName(unitType));
                controller.Construct(unitType);
            }
            else
            {
                if (saveFor)
                {
                    SaveResourcesFor(unitType);
                }
            }
        }

        private void BuildExtractor(bool saveFor = true)
        {
            if (controller.CanConstruct(Units.EXTRACTOR))
            {
                controller.ConstructOnGasGeyser(Units.EXTRACTOR);
            }
            if (saveFor)
            {
                SaveResourcesFor(Units.EXTRACTOR);
            }
        }

        private void UpgradeToLair(bool saveFor = true)
        {
            var hatcheries = controller.GetUnits(Units.HATCHERY, onlyCompleted: true);

            if (controller.CanConstruct(Units.LAIR))
            {
                foreach (var hatchery in hatcheries)
                {
                    if (hatchery.order.AbilityId != 0) continue;

                    hatchery.Train(Units.LAIR);
                    Logger.Info("Upgrade to Lair @ {0} / {1}", hatchery.position.X, hatchery.position.Y);
                    break;
                }
            }
            if (saveFor && hatcheries.Count > 0)
            {
                foreach (var hatchery in hatcheries)
                {
                    if (hatchery.order.AbilityId != 0) continue;

                    SaveResourcesFor(Units.LAIR);
                    break;
                }
            }
        }

        private void UpgradeToHive(bool saveFor = true)
        {
            var lairs = controller.GetUnits(Units.LAIR, onlyCompleted: true);

            if (controller.CanConstruct(Units.HIVE))
            {
                foreach (var lair in lairs)
                {
                    if (lair.order.AbilityId != 0) continue;

                    lair.Train(Units.HIVE);
                    Logger.Info("Upgrade to Hive @ {0} / {1}", lair.position.X, lair.position.Y);
                    break;
                }
            }
            if (saveFor && lairs.Count > 0)
            {
                foreach (var hive in lairs)
                {
                    if (hive.order.AbilityId != 0) continue;

                    SaveResourcesFor(Units.HIVE);
                    break;
                }
            }

        }

        // -----------------------------------------------UNITS---------------------------------------------
        private void BuildUnit(uint unitType, bool saveFor = true)
        {
            if (controller.CanConstruct(unitType))
            {
                var larvas = controller.GetUnits(Units.LARVA);
                if (larvas.Count > 0)
                {
                    larvas[0].Train(unitType);
                }
            }
            else
            {
                if (saveFor)
                {
                    SaveResourcesFor(unitType);
                }
            }

        }

        private void BuildOverlord(bool saveFor = true)
        {
            if (controller.maxSupply < 200)
            {
                BuildUnit(Units.OVERLORD, saveFor);
            }
        }

        private void BirthQueen(bool saveFor = true)
        {
            if (controller.CanConstruct(Units.QUEEN))
            {
                var resourceCenters = controller.GetUnits(Units.ResourceCenters, onlyCompleted: true);
                foreach (var resourceCenter in resourceCenters)
                {
                    if (resourceCenter.order.AbilityId != 0) continue;
                    
                    resourceCenter.Train(Units.QUEEN);
                }
            }
            else
            {
                if (saveFor)
                {
                    SaveResourcesFor(Units.QUEEN);
                }
            }
        }

        // -----------------------------------------------ACTIONS---------------------------------------------
        private void AttackEnemyBase()
        {
            var army = controller.GetIdleUnits(controller.GetUnits(Units.ArmyUnits));
            if (army.Count > 0)
            {
                // Logger.Info("Army = {0};  gameMin = {1}", army.Count, gameMin);
                if (controller.enemyLocations.Count > 0)
                {
                    // Logger.Info("Locations = {0}", controller.enemyLocations.Count);
                    var enemyLocation = random.Next(controller.enemyLocations.Count);
                    controller.Attack(army, controller.enemyLocations[enemyLocation]);
                }
            }
        }

        private void AttackEnemyStructure()
        {
            var enemyStructures = controller.GetUnits(Units.Structures, alliance: Alliance.Enemy, onlyVisible: true);
            if (enemyStructures.Count > 0)
            {
                var army = controller.GetIdleUnits(controller.GetUnits(Units.ArmyUnits));
                if (army.Count > 0)
                {
                    Logger.Info("Attacking: {0} @ {1} / {2}", enemyStructures[0].name, enemyStructures[0].position.X, enemyStructures[0].position.Y);
                    controller.Attack(army, enemyStructures[0].position);
                }
            }
        }

        private void RecallIdleUnits()
        {
            var army = controller.GetIdleUnits(controller.GetUnits(Units.ArmyUnits));
            var resourceCenters = controller.GetUnits(Units.ResourceCenters);
            if (army.Count > 0 && resourceCenters.Count > 0)
            {
                controller.Attack(army, resourceCenters[0].position);
            }
        }

        // -----------------------------------------------UTILITIES---------------------------------------------

        private int GetTotalRCs()
        {
            var resourceCenters = controller.GetUnits(Units.ResourceCenters, onlyCompleted: true);
            return resourceCenters.Count;
        }

        private void SaveResourcesFor(uint unitType)
        {
            var rollToSaveFor = random.Next(100);
            var showNotFoundMessage = false;

            // If there is no structure save to make one.
            if (Units.Structures.Contains(unitType) && controller.GetTotalCount(unitType, inConstruction: true) == 0 && controller.GetTotalCount(Units.DRONE) > DRONE_MIN_TO_AUTO_SAVE_RESOUCES)
            {
                rollToSaveFor = 0;
                showNotFoundMessage = true;
            }

            if (rollToSaveFor <= CHANCE_TO_SAVE_RESOURCES && controller.CanConstruct(unitType, ignoreResourceSupply: true))
            {
                if (showNotFoundMessage)
                {
                    Logger.Info("Could not find a {0}, save resouces for it.", ControllerDefault.GetUnitName(unitType));
                }

                var mineralCost = 0;
                var vespeneCost = 0;
                controller.CanAfford(unitType, ref mineralCost, ref vespeneCost);

                SetSaveResouces(unitType, mineralCost, vespeneCost);
            }
        }

        private void SetSaveResouces(uint unitType = 0, int minerals = 0, int vespene = 0)
        {
            if (unitType != 0)
            {
                Logger.Info("Save resources for {0}:  minerals = {1}  vespene = {2}", ControllerDefault.GetUnitName(unitType), minerals, vespene);
            }
            saveForUnitType = unitType;
            saveForMinerals = minerals;
            saveForVespene = vespene;
        }

        private void CreateSavedForUnit()
        {

            if (saveForUnitType == Units.OVERLORD)
            {
                BuildOverlord(saveFor: false);
            }
            else if (saveForUnitType == Units.EXTRACTOR)
            {
                BuildExtractor(saveFor: false);
            }
            else if (saveForUnitType == Units.QUEEN)
            {
                BirthQueen(saveFor: false);
            }
            else if (saveForUnitType == Units.LAIR)
            {
                UpgradeToLair(saveFor: false);
            }
            else if (saveForUnitType == Units.HIVE)
            {
                UpgradeToHive(saveFor: false);
            }
            else if (Units.Structures.Contains(saveForUnitType))
            {
                // Build structure.
                BuildBuilding(saveForUnitType, saveFor: false);
            }
            else if ((Units.ArmyUnits.Contains(saveForUnitType)))
            {
                // Build unit.
                BuildUnit(saveForUnitType, saveFor: false);
            }

            SetSaveResouces();
        }

        // -----------------------------------------------Total Random Functions---------------------------------------------

        private void BuildBuildingsRandom()
        {
            if (controller.minerals < saveForMinerals || controller.vespene < saveForVespene)
            {
                return;
            }

            var randBuilding = random.Next(16);

            switch (randBuilding)
            {
                case 0:
                    BuildBuilding(Units.HATCHERY);
                    break;
                case 1:
                    BuildBuilding(Units.SPAWNING_POOL);
                    break;
                case 2:
                    BuildBuilding(Units.SPORE_CRAWLER);
                    break;
                case 3:
                    BuildBuilding(Units.SPINE_CRAWLER);
                    break;
                case 4:
                    BuildExtractor();
                    break;
                case 5:
                    UpgradeToLair();
                    break;
                case 6:
                    BuildBuilding(Units.HYDRALISK_DEN);
                    break;
                case 7:
                    BuildBuilding(Units.ROACH_WARREN);
                    break;
                case 8:
                    BuildBuilding(Units.BANELING_NEST);
                    break;
                case 9:
                    BuildBuilding(Units.EVOLUTION_CHAMBER);
                    break;
                case 10:
                    BuildBuilding(Units.SPIRE);
                    break;
                case 11:
                    BuildBuilding(Units.NYDUS_NETWORK);
                    break;
                case 12:
                    BuildBuilding(Units.INFESTATION_PIT);
                    break;
                case 13:
                    UpgradeToHive();
                    break;
                case 14:
                    // This will fail and get stuck because it can not run the query in CanPlace.
                    // It can not get an ability ID for it and I can not find it in the stableid.json.
                    // NOTE: I fixed this by using the Lurker Den MP ID.
                    BuildBuilding(Units.LURKER_DEN);
                    break;
                case 15:
                    BuildBuilding(Units.ULTRALISK_CAVERN);
                    break;
            }
        }

        private void BuildUnitsRandom()
        {
            if (controller.minerals < saveForMinerals || controller.vespene < saveForVespene)
            {
                return;
            }

            var randUnit = random.Next(12);

            switch (randUnit)
            {
                case 0:
                    BuildOverlord();
                    break;
                case 1:
                    BuildUnit(Units.DRONE);
                    break;
                case 2:
                    BuildUnit(Units.ZERGLING);
                    break;
                case 3:
                    BuildUnit(Units.HYDRALISK);
                    break;
                case 4:
                    BirthQueen();
                    break;
                case 5:
                    BuildUnit(Units.ROACH);
                    break;
                case 6:
                    BuildUnit(Units.MUTALISK);
                    break;
                case 7:
                    BuildUnit(Units.CORRUPTOR);
                    break;
                case 8:
                    BuildUnit(Units.INFESTOR);
                    break;
                case 9:
                    BuildUnit(Units.VIPER);
                    break;
                case 10:
                    BuildUnit(Units.SWARM_HOST);
                    break;
                case 11:
                    BuildUnit(Units.ULTRALISK);
                    break;
            }
        }

        private void UnitActionsRandom()
        {
            var randAction = random.Next(3);

            switch (randAction)
            {
                case 0:
                    var army = controller.GetUnits(Units.ArmyUnits);
                    if (army.Count > random.Next(ATTACK_ARMY_PER_MIN, ATTACK_ARMY_MAX))
                    {
                        AttackEnemyBase();
                    }
                    break;
                case 1:
                    RecallIdleUnits();
                    break;
                case 2:
                    AttackEnemyStructure();
                    break;
            }
        }


    }
}
