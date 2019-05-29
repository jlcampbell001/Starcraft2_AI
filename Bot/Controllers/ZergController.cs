using SC2APIProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    class ZergController : ControllerDefault
    {
        override
        public bool CanAfford(uint unitType, ref int unitMinerals, ref int unitVespene)
        {
            var unitData = gameData.Units[(int)unitType];
            unitMinerals = (int)unitData.MineralCost;
            unitVespene = (int)unitData.VespeneCost;

            // For some reason the mineral cost for these structures includes the drone cost but this is already paid for so we need to take it out.
            if (Units.IncludesDroneCost.Contains(unitType))
            {
                var droneData = gameData.Units[(int)Units.DRONE];
                unitMinerals = unitMinerals - (int)droneData.MineralCost;
                //Logger.Info("Name = {0};  minerals = {1}; vespen = {2}", unitData.Name, unitMinerals, unitVespene);
            }

            // For some reason the mineral cost for lair includes the hatchery cost but this is already paid for so we need to take it out.
            if (unitType == Units.LAIR)
            {
                var hatcheryData = gameData.Units[(int)Units.HATCHERY];
                unitMinerals = unitMinerals - (int)hatcheryData.MineralCost;
                // Logger.Info("Name = {0};  minerals = {1}; vespen = {2}", unitData.Name, unitMinerals, unitVespene);
            }

            // For some reason the mineral cost for hive includes the lair cost but this is already paid for so we need to take it out.
            if (unitType == Units.HIVE)
            {
                var lairData = gameData.Units[(int)Units.LAIR];
                unitMinerals = unitMinerals - (int)lairData.MineralCost;
                // Logger.Info("Name = {0};  minerals = {1}; vespen = {2}", unitData.Name, unitMinerals, unitVespene);
            }

            //Logger.Info("Name = {0};  minerals = {1}; vespen = {2}", unitData.Name, unitMinerals, unitVespene);
            return (minerals >= unitMinerals) && (vespene >= unitVespene);
        }

        public bool CanConstruct(uint unitType, bool ignoreResourceSupply = false)
        {
            // Check if we have the structures to build the unit.
            //do we spawning pools for the unit? 
            if (Units.NeedSpawningPool.Contains(unitType))
            {
                var spawningPools = GetUnits(Units.SPAWNING_POOL, onlyCompleted: true);
                if (spawningPools.Count == 0) return false;
            }

            //do we lairs or hives for the unit? 
            if (Units.NeedLairOrHive.Contains(unitType))
            {
                var lairs = GetUnits(Units.LAIR, onlyCompleted: true);
                var hives = GetUnits(Units.HIVE, onlyCompleted: true);
                if (lairs.Count == 0 && hives.Count == 0) return false;
            }

            //do we lairs only for the unit? 
            if (Units.NeedLair.Contains(unitType))
            {
                var lairs = GetUnits(Units.LAIR, onlyCompleted: true);
                if (lairs.Count == 0) return false;
            }

            //do we hive only for the unit? 
            if (Units.NeedHive.Contains(unitType))
            {
                var hives = GetUnits(Units.HIVE, onlyCompleted: true);
                if (hives.Count == 0) return false;
            }

            //do we hydralisk dens for the unit? 
            if (Units.NeedHydraliskDen.Contains(unitType))
            {
                var hydraliskDens = GetUnits(Units.HYDRALISK_DEN, onlyCompleted: true);
                if (hydraliskDens.Count == 0) return false;
            }

            //do we roach warren for the unit? 
            if (Units.NeedRoachWarren.Contains(unitType))
            {
                var roachWarrens = GetUnits(Units.ROACH_WARREN, onlyCompleted: true);
                if (roachWarrens.Count == 0) return false;
            }

            //do we a spire or greater spire for the unit? 
            if (Units.NeedSpireOrGreaterSpire.Contains(unitType))
            {
                var spires = GetUnits(Units.SPIRE, onlyCompleted: true);
                var greaterSpires = GetUnits(Units.GREATER_SPIRE, onlyCompleted: true);
                if (spires.Count == 0 && greaterSpires.Count == 0) return false;
            }

            //do we an infestation pit for the unit? 
            if (Units.NeedInfestationPit.Contains(unitType))
            {
                var infestationPits = GetUnits(Units.INFESTATION_PIT, onlyCompleted: true);
                if (infestationPits.Count == 0) return false;
            }

            //do we a luker den for the unit? 
            if (Units.NeedLurkerDen.Contains(unitType))
            {
                var lurkerDens = GetUnits(Units.LURKER_DEN, onlyCompleted: true);
                if (lurkerDens.Count == 0) return false;
            }

            //do we an ultralisk cavern for the unit? 
            if (Units.NeedUltraliskCavern.Contains(unitType))
            {
                var ultraliskCaverns = GetUnits(Units.ULTRALISK_CAVERN, onlyCompleted: true);
                if (ultraliskCaverns.Count == 0) return false;
            }

            //is it a structure?
            if (Units.Structures.Contains(unitType))
            {
                if (!ignoreResourceSupply)
                {
                    //we need worker for every structure
                    if (GetUnits(Units.Workers).Count == 0) return false;
                }

                //we need an RC for any structure
                var resourceCenters = GetUnits(Units.ResourceCenters, onlyCompleted: true);
                if (resourceCenters.Count == 0) return false;
            }

            //it's an actual unit
            else
            {
                if (!ignoreResourceSupply)
                {
                    //do we have enough supply?
                    var requiredSupply = gameData.Units[(int)unitType].FoodRequired;
                    if (requiredSupply > (maxSupply - currentSupply))
                        return false;
                }
            }
            var ret = true;

            if (!ignoreResourceSupply)
            {
                ret = CanAfford(unitType);
            }

            return ret;
        }

    }
}

