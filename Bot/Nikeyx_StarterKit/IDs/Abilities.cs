﻿using System.Collections.Generic;

namespace Bot
{
    internal static class Abilities
    {
        // You can get all these values from the stableid.json file (just search for it on your PC).

        public static int RESEARCH_BANSHEE_CLOAK = 790;
        public static int RESEARCH_INFERNAL_PREIGNITER = 761;
        public static int RESEARCH_UPGRADE_MECH_AIR = 3699;
        public static int RESEARCH_UPGRADE_MECH_ARMOR = 3700;
        public static int RESEARCH_UPGRADE_MECH_GROUND = 3701;

        public static int RESEARCH_BURROW = 1225;
        public static int BURROW = 64;
        public static int BURROW_DRONE = 1378;
        public static int UNBURROW_DRONE = 1380;
        public static int BURROW_ZERGLING = 1390;
        public static int UNBURROW_ZERGLING = 1392;
        public static int BURROW_QUEEN = 1433;
        public static int UNBURROW_QUEEN = 1435;
        public static int BURROW_BANELING = 1374;
        public static int UNBURROW_BANELING = 1376;
        public static int BURROW_ROACH = 1386;
        public static int UNBURROW_ROACH = 1388;
        public static int BURROW_RAVAGER = 2340;
        public static int UNBURROW_RAVAGER = 2342;
        public static int BURROW_HYDRALISK = 1382;
        public static int UNBURROW_HYDRALISK = 1384;
        public static int BURROW_LURKER = 2108;
        public static int UNBURROW_LURKER = 2110;
        public static int BURROW_INFESTOR = 1444;
        public static int UNBURROW_INFESTOR = 1446;
        public static int BURROW_SWARM_HOST = 2014;
        public static int UNBURROW_SWARM_HOST = 2016;
        public static int BURROW_ULTRALISK = 1512;
        public static int UNBURROW_ULTRALISK = 1514;
        public static int BURROW_INFESTED_TERRAN = 1394;
        public static int UNBURROW_INFESTED_TERRAN = 1396;

        public static int RESEARCH_PNEUMATIZED_CARAPACE = 1223;
        public static int PNEUMATIZED_CARAPACE = 62;

        public static int HATCHERY_UNIT_RALLY = 211;
        public static int HATCHERY_WORKER_RALLY = 212;

        public static int GENERATE_CREEP_ON = 1692;
        public static int GENERATE_CREEP_OFF = 1693;

        public static int MUTATE_VENTRAL_SACS = 1224;
        public static int MORPH_OVERLORD_TRANSPORT = 2708;

        public static int LOAD_OVERLORD = 1406;
        public static int UNLOADAll_OVERLORD = 1408;
        public static int UNLOADUNIT_OVERLORD = 1409;

        public static int SPAWN_LARVA = 251;
        public static int SPAWN_CREEP_TUMOR_QUEEN = 1694;
        public static int TRANSFUSION = 1664;

        public static int SPAWN_CREEP_TUMOR = 1733;

        public static int RESEARCH_ADRENAL_GLANDS = 1252;
        public static int ADRENAL_GLANDS = 65;

        public static int RESEARCH_METABOLIC_BOOST = 1253;
        public static int METABOLIC_BOOST = 66;

        public static int RESEARCH_MELEE_ATTACK1_ZERG = 1186;
        public static int RESEARCH_MELEE_ATTACK2_ZERG = 1187;
        public static int RESEARCH_MELEE_ATTACK3_ZERG = 1188;
        public static int MELEE_ATTACK1_ZERG = 53;
        public static int MELEE_ATTACK2_ZERG = 54;
        public static int MELEE_ATTACK3_ZERG = 55;

        public static int RESEARCH_MISSILE_ATTACK1_ZERG = 1192;
        public static int RESEARCH_MISSILE_ATTACK2_ZERG = 1193;
        public static int RESEARCH_MISSILE_ATTACK3_ZERG = 1194;
        public static int MISSILE_ATTACK1_ZERG = 59;
        public static int MISSILE_ATTACK2_ZERG = 60;
        public static int MISSILE_ATTACK3_ZERG = 61;

        public static int RESEARCH_GROUND_ARMOR1_ZERG = 1189;
        public static int RESEARCH_GROUND_ARMOR2_ZERG = 1190;
        public static int RESEARCH_GROUND_ARMOR3_ZERG = 1191;
        public static int GROUND_ARMOR1_ZERG = 56;
        public static int GROUND_ARMOR2_ZERG = 57;
        public static int GROUND_ARMOR3_ZERG = 58;

        public static int RESEARCH_GLIAL_RECONSITUTION = 216;
        public static int GLIAL_RECONSITUTION = 2;

        public static int RESEARCH_TUNNELING_CLAWS = 217;
        public static int TUNNELING_CLAWS = 3;

        public static int CANCEL_CONSTRUCTION = 314;
        public static int CANCEL = 3659;
        public static int CANCEL_LAST = 3671;
        public static int LIFT = 3679;
        public static int LAND = 3678;

        public static int SMART = 1;
        public static int STOP = 4;
        public static int ATTACK = 23;
        public static int MOVE = 16;
        public static int PATROL = 17;
        public static int RALLY = 3673;
        public static int REPAIR = 316;

        public static int THOR_SWITCH_AP = 2362;
        public static int THOR_SWITCH_NORMAL = 2364;
        public static int SCANNER_SWEEP = 399;
        public static int YAMATO = 401;
        public static int CALL_DOWN_MULE = 171;
        public static int CLOAK = 3676;
        public static int REAPER_GRENADE = 2588;
        public static int DEPOT_RAISE = 558;
        public static int DEPOT_LOWER = 556;
        public static int SIEGE_TANK = 388;
        public static int UNSIEGE_TANK = 390;
        public static int TRANSFORM_TO_HELLBAT = 1998;
        public static int TRANSFORM_TO_HELLION = 1978;
        public static int UNLOAD_BUNKER = 408;
        public static int SALVAGE_BUNKER = 32;

        public static int LARVA_TRAIN_ZERGLING = 1343;
        public static int BIRTH_QUEEN = 1632;

        public static int UPGRADE_TO_LAIR = 1216;

        //gathering/returning minerals
        public static int GATHER_RESOURCES = 295;
        public static int RETURN_RESOURCES = 296;
        public static int PROBE_GATHER_RESOURCES = 298;
        public static int PROBE_RETURN_RESOURCES = 299;
        public static int DRONE_GATHER_RESOURCES = 1183;
        public static int DRONE_RETURN_RESOURCES = 1184;

        //gathering/returning minerals
        public static int GATHER_MINERALS = 295;
        public static int RETURN_MINERALS = 296;
        public static int PROBE_GATHER_MINERALS = 298;
        public static int PROBE_RETURN_MINERALS = 299;
        public static int DRONE_GATHER_MINERALS = 1183;
        public static int DRONE_RETURN_MINERALS = 1184;

        public static readonly HashSet<int> GatherReturnResources = new HashSet<int> {
            GATHER_RESOURCES,
            RETURN_RESOURCES,
            PROBE_GATHER_RESOURCES,
            PROBE_RETURN_RESOURCES,
            DRONE_GATHER_RESOURCES,
            DRONE_RETURN_RESOURCES
        };

        public static readonly HashSet<int> GatherMinerals = new HashSet<int> {
            GATHER_MINERALS,
            PROBE_GATHER_MINERALS,
            DRONE_GATHER_MINERALS,
        };

        public static readonly HashSet<int> GatherResources = new HashSet<int> {
            GATHER_RESOURCES,
            PROBE_GATHER_RESOURCES,
            DRONE_GATHER_RESOURCES,
        };

        public static readonly HashSet<int> ZergResourceCenterResearch = new HashSet<int>
        {
            RESEARCH_PNEUMATIZED_CARAPACE,
            RESEARCH_BURROW
        };

        public static readonly HashSet<int> SpawingPoolResearch = new HashSet<int>
        {
           RESEARCH_ADRENAL_GLANDS,
           RESEARCH_METABOLIC_BOOST
        };

        public static readonly HashSet<int> EvolutionChamberResearch = new HashSet<int>
        {
            RESEARCH_MELEE_ATTACK1_ZERG,
            RESEARCH_MELEE_ATTACK2_ZERG,
            RESEARCH_MELEE_ATTACK3_ZERG,
            RESEARCH_MISSILE_ATTACK1_ZERG,
            RESEARCH_MISSILE_ATTACK2_ZERG,
            RESEARCH_MISSILE_ATTACK3_ZERG,
            RESEARCH_GROUND_ARMOR1_ZERG,
            RESEARCH_GROUND_ARMOR2_ZERG,
            RESEARCH_GROUND_ARMOR3_ZERG
        };

        public static readonly HashSet<int> RoachWarrenResearch = new HashSet<int>
        {
           RESEARCH_GLIAL_RECONSITUTION,
           RESEARCH_TUNNELING_CLAWS
        };

        public static readonly HashSet<int> ZergMeleeAttackResearch = new HashSet<int>
        {
            RESEARCH_MELEE_ATTACK1_ZERG,
            RESEARCH_MELEE_ATTACK2_ZERG,
            RESEARCH_MELEE_ATTACK3_ZERG
        };

        public static readonly HashSet<int> ZergMissileAttackResearch = new HashSet<int>
        {
            RESEARCH_MISSILE_ATTACK1_ZERG,
            RESEARCH_MISSILE_ATTACK2_ZERG,
            RESEARCH_MISSILE_ATTACK3_ZERG
        };

        public static readonly HashSet<int> ZergGroundArmorResearch = new HashSet<int>
        {
            RESEARCH_GROUND_ARMOR1_ZERG,
            RESEARCH_GROUND_ARMOR2_ZERG,
            RESEARCH_GROUND_ARMOR3_ZERG
        };

        public static readonly HashSet<int> RequiresLair = new HashSet<int>
        {
            RESEARCH_MELEE_ATTACK2_ZERG,
            RESEARCH_MISSILE_ATTACK2_ZERG,
            RESEARCH_GROUND_ARMOR2_ZERG,
            RESEARCH_GLIAL_RECONSITUTION,
            RESEARCH_TUNNELING_CLAWS
        };

        public static readonly HashSet<int> RequiresHive = new HashSet<int>
        {
            RESEARCH_MELEE_ATTACK3_ZERG,
            RESEARCH_MISSILE_ATTACK3_ZERG,
            RESEARCH_GROUND_ARMOR3_ZERG
        };

        // Get the id of a unit.
        public static int GetID(uint unit)
        {
            return (int)ControllerDefault.GameData.Units[(int)unit].AbilityId;
        }

    }
}
