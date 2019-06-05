using Bot.UnitActions.Zerg;
using Bot.Utilities;
using SC2APIProtocol;
using System.Collections.Generic;

namespace Bot
{
    internal class JCZergBot : Bot
    {
        private readonly bool totalRandom = true;

        private const int WAIT_IN_SECONDS = 1;


        private const int BUILD_OVERLORD_RANGE = 2;
        private const int DRONE_PER_RC = 19;
        private const int DRONE_MIN = 10;
        private const int DRONE_MAX = 100;
        private const int ZERGLINGS_PER_HYDRALISK = 4;
        private const int ZERGLINGS_PER_MUTALISKS = 3;

        private const int MAX_SPAWNING_POOLS = 2;
        private const int MAX_HYDRALISK_DENS = 1;
        private const int MAX_SPIRES = 1;
        private const int CHANCE_TO_SAVE_RESOURCES = 1;
        private const int CHANCE_BUILD_DEF_BUILDING = 10;
        private const int SPINE_PER_RC = 10;
        private const int SPORE_PER_RC = 10;
        private const int EXPAND_BASE_INTERVAL_MINS = 5;

        private const int ATTACK_ARMY_PER_MIN = 5;
        private const int ATTACK_ARMY_MAX = 50;

        private const int MIN_VESPENE = 50;

        private readonly System.Random random = new System.Random();

        private int gameMin = -1;
        private ulong nextWaitFrame = 0;
        private bool burrowedUnits = false;

        private uint saveForUnitType = 0;
        private int saveForUpgrade = 0;
        private int saveForMinerals = 0;
        private int saveForVespene = 0;
        private string saveForName = "";

        private int expandBaseMinute = EXPAND_BASE_INTERVAL_MINS;

        private ZergController controller = new ZergController();

        private DroneActions drone;
        private ZerglingActions zergling;
        private QueenActions queen;
        private BanelingActions baneling;
        private RoachActions roach;
        private RavagerActions ravager;
        private HydraliskActions hydralisk;
        private LurkerActions lurker;

        public JCZergBot()
        {
            drone = new DroneActions(controller);
            zergling = new ZerglingActions(controller);
            queen = new QueenActions(controller);
            baneling = new BanelingActions(controller);
            roach = new RoachActions(controller);
            ravager = new RavagerActions(controller);
            hydralisk = new HydraliskActions(controller);
            lurker = new LurkerActions(controller);
        }

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

            // Distribute workers optimally every 10 frames.
            if (controller.frame % 10 == 0)
            {
                controller.DistributeWorkers();
            }

            // Setup the game minute.
            if (controller.frame % (22 * 60) == 0)
            {
                gameMin++;
            }

            if (controller.minerals >= saveForMinerals && controller.vespene >= saveForVespene)
            {
                // Construct / Train saved for unit.
                if (saveForUnitType != 0 || saveForUpgrade != 0)
                {
                    CreateSavedFor();
                }
            }
            else
            {
                // There are no workers to gather.
                var workers = controller.GetUnits(Units.Workers);
                if (workers.Count == 0)
                {
                    Logger.Info("No workers to gather resources to save for {0}.", saveForName);
                    SetSaveResouces();

                    BuildUnit(Units.DRONE);
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
                // Note:  Need to add code to make sure vespene is being gathered.
                if (saveForVespene > 0)
                {
                    var gasBuildings = controller.GetUnits(Units.GasGeysersStructures);
                    var totalVespeneLeft = 0;
                    var totalWorkersAssigned = 0;
                    foreach (var gasBuilding in gasBuildings)
                    {
                        totalVespeneLeft = totalVespeneLeft + gasBuilding.vespene;
                        totalWorkersAssigned = totalWorkersAssigned + gasBuilding.assignedWorkers;
                    }

                    if (totalVespeneLeft < saveForVespene)
                    {
                        Logger.Info("Not enough vespene left: {0} left but need {1}", totalVespeneLeft, saveForVespene);
                        SetSaveResouces();
                    }
                    else if (totalWorkersAssigned == 0)
                    {
                        // This is most likely happening because there are a lot of extractors and not enough drones,
                        // so the distribution code is not assigning any drones to the extractors.
                        // I need to re-worked that but for now make more drones.
                        Logger.Info("No Workers are assigned to collect vespene.");
                        SetSaveResouces();

                        BuildUnit(Units.DRONE);
                    }
                }
            }

            if (controller.frame > nextWaitFrame)
            {
                // If there are no resource centers build one.
                if (controller.GetTotalCount(Units.ResourceCenters) == 0)
                {
                    BuildExpansionBase();
                }

                // This is for a bot that randomly dose all its actions.
                if (totalRandom)
                {
                    var randAction = random.Next(5);

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
                        case 4:
                            ResearchRandom();
                            break;
                    }

                }
                else
                {
                    // Bot will try and do actions more intelligently.
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

        // Try and create buildings intelligently.
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

            // Note: Need to figure a better way to decide when to create extractors.
            var gasGeysers = controller.GetUnits(Units.GasGeysersAvail, alliance: Alliance.Neutral, onlyVisible: true, hasVespene: true);
            if (controller.vespene < MIN_VESPENE && gasGeysers.Count > 0)
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

            if (gameMin >= expandBaseMinute)
            {
                BuildExpansionBase();
            }

            var randDef = random.Next(100);

            if (randDef < CHANCE_BUILD_DEF_BUILDING)
            {
                var randCrawler = random.Next(100);
                //Logger.Info("Def = {0};   Crawler = {1}", randDef, randCrawler);

                if (randCrawler < 50 && controller.GetTotalCount(Units.SPINE_CRAWLER) < SPINE_PER_RC * controller.GetTotalRCs())
                {
                    BuildBuilding(Units.SPINE_CRAWLER);
                }
                else if (controller.GetTotalCount(Units.SPORE_CRAWLER) < SPORE_PER_RC * controller.GetTotalRCs())
                {
                    BuildBuilding(Units.SPORE_CRAWLER);
                }
            }
        }

        // Try and create units intelligently.
        private void BuildUnits()
        {
            if (controller.minerals < saveForMinerals || controller.vespene < saveForVespene)
            {
                return;
            }

            var totalZergling = controller.GetTotalCount(Units.ZERGLING);
            var totalWorkers = controller.GetTotalCount(Units.Workers);

            if (controller.maxSupply - controller.currentSupply <= BUILD_OVERLORD_RANGE)
            {
                BuildOverlord();
            }
            else if (totalWorkers < DRONE_MAX && totalWorkers < (DRONE_PER_RC * controller.GetTotalRCs()))
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

        // Try and preform actions intelligently.
        private void UnitActions()
        {
            var army = controller.GetIdleUnits(controller.GetUnits(Units.ArmyUnits));
            var enemyArmy = controller.GetUnits(Units.ArmyUnits, alliance: Alliance.Enemy, onlyVisible: true);
            var enemyBuildings = controller.GetUnits(Units.Structures, alliance: Alliance.Enemy, onlyVisible: true);
            var resourceCenters = controller.GetUnits(Units.ResourceCenters);

            var canNotSeeRCArmy = controller.GetUnitsNoInSightOfRC(army);

            if (canNotSeeRCArmy.Count > 0 && enemyBuildings.Count == 0 && enemyArmy.Count == 0)
            {
                //Logger.Info("Unit = {0};  Sight = {1}", canNotSeeRCArmy[0].name, canNotSeeRCArmy[0].sight);
                RecallIdleUnits();
            }
            else if (army.Count > ATTACK_ARMY_MAX || army.Count > ATTACK_ARMY_PER_MIN * gameMin)
            {
                if (enemyArmy.Count > 0)
                {
                    AttackEnemyUnits();
                }
                else if (enemyBuildings.Count > 0)
                {
                    AttackEnemyStructure();
                }
                else
                {
                    AttackEnemyBase();
                }
            }
        }

        /**********
         * BUILDINGS
         **********/

        // Basic code to create a building.
        private void BuildBuilding(uint unitType, bool saveFor = true)
        {
            // Do not construct buildings if there are less then a certain amount of drones.
            if (controller.GetUnits(Units.Workers).Count <= DRONE_MIN)
            {
                return;
            }

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
                    SaveResourcesForUnit(unitType);
                }
            }
        }

        // Build an extractor.
        private void BuildExtractor(bool saveFor = true)
        {
            if (controller.CanConstruct(Units.EXTRACTOR))
            {
                controller.ConstructOnGasGeyser(Units.EXTRACTOR);
            }
            if (saveFor)
            {
                SaveResourcesForUnit(Units.EXTRACTOR);
            }
        }

        // Upgrade a hatchery to a lair.
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

                    SaveResourcesForUnit(Units.LAIR);
                    break;
                }
            }
        }

        // Upgrade a lair to a hive.
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

                    SaveResourcesForUnit(Units.HIVE);
                    break;
                }
            }

        }


        private void BuildExpansionBase(bool saveFor = true)
        {
            if (controller.CanConstruct(Units.HATCHERY))
            {
                controller.BuildExpansion();
                expandBaseMinute = gameMin + EXPAND_BASE_INTERVAL_MINS;
            }
            else
            {
                if (saveFor)
                {
                    SaveResourcesForUnit(Units.EXPANSION_BASE);
                }
            }
        }

        /**********
         * UNITS
         **********/

        // Basic code to build a unit that requires a larva.
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
                    SaveResourcesForUnit(unitType);
                }
            }

        }

        // Code to build an overlord.
        // Since you can not have more then 200 supply do not create more overlords at that point.
        private void BuildOverlord(bool saveFor = true)
        {
            if (controller.maxSupply < 200)
            {
                BuildUnit(Units.OVERLORD, saveFor);
            }
        }

        // Create a queen.
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
                    SaveResourcesForUnit(Units.QUEEN);
                }
            }
        }

        /**********
         * Research
         **********/

        // Research the burrow upgrade.
        private void ResearchBurrow(bool saveFor = true)
        {
            if (controller.HasUpgrade(Abilities.BURROW)) return;

            if (controller.GetTotalCount(Units.GasGeysersStructures) == 0) return;

            var resourceCenters = controller.GetUnits(Units.ResourceCenters);

            if (resourceCenters.Count > 0)
            {
                if (controller.IsResearchingUpgrade(Abilities.RESEARCH_BURROW, resourceCenters)) return;


                if (controller.CanAffordUpgrade(Abilities.RESEARCH_BURROW))
                {

                    foreach (var resourceCenter in resourceCenters)
                    {
                        if (resourceCenter.buildProgress != 1) continue;

                        if (resourceCenter.order.AbilityId != 0) continue;

                        resourceCenter.Research(Abilities.RESEARCH_BURROW);
                    }
                }
                else
                {
                    if (saveFor)
                    {
                        SaveResourcesForUpgrade(Abilities.RESEARCH_BURROW);
                    }
                }
            }
        }

        /**********
         * Abilities
         **********/

        // Burrow the passed unit if able.
        private void BurrowUnit(Unit unit)
        {
            if (!controller.HasUpgrade(Abilities.BURROW)) return;

            burrowedUnits = true;

            if (unit.unitType == Units.DRONE)
            {
                drone.Burrow(unit);
            }
            else if (unit.unitType == Units.ZERGLING)
            {
                zergling.Burrow(unit);
            }
            else if (unit.unitType == Units.QUEEN)
            {
                queen.Burrow(unit);
            }
            else if (unit.unitType == Units.BANELING)
            {
                baneling.Burrow(unit);
            }
            else if (unit.unitType == Units.ROACH)
            {
                roach.Burrow(unit);
            }
            else if (unit.unitType == Units.RAVAGER)
            {
                ravager.Burrow(unit);
            }
            else if (unit.unitType == Units.HYDRALISK)
            {
                hydralisk.Burrow(unit);
            }
            else if (unit.unitType == Units.LURKER)
            {
                lurker.Burrow(unit);
            }
            else
            {
                burrowedUnits = false;
            }
        }

        // Burrow the passed unit if able.
        private void UnburrowUnit(Unit unit, bool setAutoCastOn = false)
        {
            if (!controller.HasUpgrade(Abilities.BURROW)) return;
            //Logger.Info("Unburrow {0}, {1}", unit.name, unit.unitType);

            if (unit.unitType == Units.DRONE_BURROWED)
            {
                drone.Unburrow(unit);
            }
            else if (unit.unitType == Units.ZERGLING_BURROWED)
            {
                zergling.Unburrow(unit);
            }
            else if (unit.unitType == Units.QUEEN_BURROWED)
            {
                queen.Unburrow(unit);
            }
            else if (unit.unitType == Units.BANELING_BURROWED)
            {
                baneling.Unburrow(unit);
            }
            else if (unit.unitType == Units.ROACH_BURROWED)
            {
                roach.Unburrow(unit);
            }
            else if (unit.unitType == Units.RAVAGER_BURROWED)
            {
                ravager.Unburrow(unit);
            }
            else if (unit.unitType == Units.HYDRALISK_BURROWED)
            {
                hydralisk.Unburrow(unit);
            }
            else if (unit.unitType == Units.LURKER_BURROWED)
            {
                lurker.Unburrow(unit);
            }
        }


        /**********
         * ACTIONS
         **********/

        // If there are idle units in the army send them to attack the enemy base at its starting location.
        private void AttackEnemyBase()
        {
            var army = controller.GetIdleUnits(controller.GetUnits(Units.ArmyUnits));
            if (army.Count > 0)
            {
                // Logger.Info("Army = {0};  gameMin = {1}", army.Count, gameMin);
                if (controller.enemyLocations.Count > 0)
                {
                    UnburrowIdleUnits();
                    // Logger.Info("Locations = {0}", controller.enemyLocations.Count);
                    var enemyLocation = random.Next(controller.enemyLocations.Count);
                    controller.Attack(army, controller.enemyLocations[enemyLocation]);
                }
            }
        }

        // If there are known enemy structures send any idle units to attack it.
        private void AttackEnemyStructure()
        {
            var enemyStructures = controller.GetUnits(Units.Structures, alliance: Alliance.Enemy, onlyVisible: true);
            if (enemyStructures.Count > 0)
            {
                var army = controller.GetIdleUnits(controller.GetUnits(Units.ArmyUnits));
                if (army.Count > 0)
                {
                    UnburrowIdleUnits();
                    Logger.Info("Attacking: {0} @ {1} / {2}", enemyStructures[0].name, enemyStructures[0].position.X, enemyStructures[0].position.Y);
                    controller.Attack(army, enemyStructures[0].position);
                }
            }
        }

        // If there are known enemy units send any idle units to attack it.
        private void AttackEnemyUnits()
        {
            var enemyArmy = controller.GetUnits(Units.ArmyUnits, alliance: Alliance.Enemy, onlyVisible: true);
            if (enemyArmy.Count > 0)
            {
                var army = controller.GetIdleUnits(controller.GetUnits(Units.ArmyUnits));
                if (army.Count > 0)
                {
                    UnburrowIdleUnits();

                    Logger.Info("Attacking: {0} @ {1} / {2}", enemyArmy[0].name, enemyArmy[0].position.X, enemyArmy[0].position.Y);
                    controller.Attack(army, enemyArmy[0].position);
                }
            }
        }

        // Burrow idle units.
        private void BurrowIdleUnits()
        {
            if (!controller.HasUpgrade(Abilities.BURROW)) return;

            var army = controller.GetIdleUnits(controller.GetUnits(Units.ArmyUnits));
            foreach (var unit in army)
            {
                if (!unit.isBurrowed)
                {
                    BurrowUnit(unit);
                    //Logger.Info("Burrowing: {0}", unit.name);
                }
            }
        }

        // Unburrow idle units.
        private void UnburrowIdleUnits()
        {
            if (!controller.HasUpgrade(Abilities.BURROW)) return;

            var army = controller.GetIdleUnits(controller.GetUnits(Units.BurrowedUnits));
            foreach (var unit in army)
            {
                if (unit.isBurrowed)
                {
                    UnburrowUnit(unit);
                    //Logger.Info("Unburrowing: {0}", unit.name);
                }
            }
        }

        // Recall idle units back to a resource center.
        // Note: Maybe I should change it to be the closest resource center.
        private void RecallIdleUnits()
        {
            var army = controller.GetIdleUnits(controller.GetUnits(Units.ArmyUnits));
            var resourceCenters = controller.GetUnits(Units.ResourceCenters);
            if (army.Count > 0 && resourceCenters.Count > 0)
            {
                controller.Attack(army, resourceCenters[0].position);
            }
        }

        /**********
         * UTILITIES
         **********/

        // Save resources for the passed unit type.
        private void SaveResourcesForUnit(uint unitType = 0)
        {
            var rollToSaveFor = random.Next(100);
            var showNotFoundMessage = false;

            var saveUnitType = unitType;

            // In expanding the base we are really saving resources for the hatchery.
            if (unitType == Units.EXPANSION_BASE)
            {
                saveUnitType = Units.HATCHERY;
            }

            // If there is no structure save to make one.
            if (Units.Structures.Contains(saveUnitType) && controller.GetTotalCount(saveUnitType, inConstruction: true) == 0 && controller.GetTotalCount(Units.DRONE) > DRONE_MIN)
            {
                rollToSaveFor = 0;
                showNotFoundMessage = true;
            }

            if (rollToSaveFor <= CHANCE_TO_SAVE_RESOURCES && controller.CanConstruct(saveUnitType, ignoreResourceSupply: true))
            {
                if (showNotFoundMessage)
                {
                    Logger.Info("Could not find a {0}, save resources for it.", ControllerDefault.GetUnitName(saveUnitType));
                }

                var mineralCost = 0;
                var vespeneCost = 0;
                controller.CanAfford(saveUnitType, ref mineralCost, ref vespeneCost);

                SetSaveResouces(unitType, upgradeId: 0, mineralCost, vespeneCost);
            }
        }
        // Save resources for the passed upgrade.
        private void SaveResourcesForUpgrade(int saveUpgradeId = 0)
        {
            var mineralCost = 0;
            var vespeneCost = 0;
            controller.CanAffordUpgrade(saveUpgradeId, ref mineralCost, ref vespeneCost);

            SetSaveResouces(unitType: 0, saveUpgradeId, mineralCost, vespeneCost);
        }

        // Set the resources that need to be saved to for a unit type or upgrade.
        // Not sending in any data to the method will reset it to not saving for a unit and upgrade.
        private void SetSaveResouces(uint unitType = 0, int upgradeId = 0, int minerals = 0, int vespene = 0)
        {
            saveForName = "";

            // Do not try and save for vespene is no one is gathering it.
            if (vespene > 0)
            {
                if (!controller.isGatheringVespene()) return;
            }

            if (unitType != 0)
            {
                saveForName = ControllerDefault.GetUnitName(unitType);
                saveForUnitType = unitType;
            }
            else if (upgradeId != 0)
            {
                saveForName = ControllerDefault.GetAbilityName(upgradeId);
                saveForUpgrade = upgradeId;
            }

            saveForMinerals = minerals;
            saveForVespene = vespene;

            if (saveForName != "")
            {
                Logger.Info("Save resources for {0}:  minerals = {1}  vespene = {2}", saveForName, minerals, vespene);
            }
        }

        //  Create a unit that resources were saved for.
        private void CreateSavedFor()
        {
            if (saveForUnitType != 0)
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
                else if (saveForUnitType == Units.EXPANSION_BASE)
                {
                    BuildExpansionBase(saveFor: false);
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
            }
            else
            {
                if (saveForUpgrade == Abilities.RESEARCH_BURROW)
                {
                    ResearchBurrow(saveFor: false);
                }
            }
            SetSaveResouces();
        }

        /**********
         * Total Random Functions
         **********/

        // Randomly build building.
        private void BuildBuildingsRandom()
        {
            if (controller.minerals < saveForMinerals || controller.vespene < saveForVespene)
            {
                return;
            }

            var randBuilding = random.Next(17);

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
                    //BuildBuilding(Units.EVOLUTION_CHAMBER);
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
                case 16:
                    BuildExpansionBase();
                    break;
            }
        }

        // Randomly build units.
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

        // Randomly preform research.
        private void ResearchRandom()
        {
            if (controller.minerals < saveForMinerals || controller.vespene < saveForVespene)
            {
                return;
            }

            var randResearch = random.Next(1);

            switch (randResearch)
            {
                case 0:
                    ResearchBurrow();
                    break;
            }
        }

        // Randomly preform actions.
        private void UnitActionsRandom()
        {
            var randAction = random.Next(6);
            var army = controller.GetUnits(Units.ArmyUnits);

            switch (randAction)
            {
                case 0:
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
                case 3:
                    if (army.Count > random.Next(ATTACK_ARMY_PER_MIN, ATTACK_ARMY_MAX))
                    {
                        AttackEnemyUnits();
                    }
                    break;
                case 4:
                    BurrowIdleUnits();
                    break;
                case 5:
                    UnburrowIdleUnits();
                    break;
            }
        }
    }
}
