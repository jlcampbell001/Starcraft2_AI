﻿namespace Bot
{
    class ZergController : ControllerDefault
    {
        // Check and see if we can afford to make the passed structure or unit and return the costs be reference.
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
                //Logger.Info("Name = {0};  minerals = {1}; vespene = {2}", unitData.Name, unitMinerals, unitVespene);
            }

            // For some reason the mineral cost for lair includes the hatchery cost but this is already paid for so we need to take it out.
            if (unitType == Units.LAIR)
            {
                var hatcheryData = gameData.Units[(int)Units.HATCHERY];
                unitMinerals = unitMinerals - (int)hatcheryData.MineralCost;
                // Logger.Info("Name = {0};  minerals = {1}; vespene = {2}", unitData.Name, unitMinerals, unitVespene);
            }

            // For some reason the mineral cost for hive includes the lair cost but this is already paid for so we need to take it out.
            if (unitType == Units.HIVE)
            {
                var lairData = gameData.Units[(int)Units.LAIR];
                unitMinerals = unitMinerals - (int)lairData.MineralCost;
                // Logger.Info("Name = {0};  minerals = {1}; vespene = {2}", unitData.Name, unitMinerals, unitVespene);
            }

            // For some reason the mineral cost for overseer includes the overlord cost but this is already paid for so we need to take it out.
            if (unitType == Units.OVERSEER)
            {
                var overlordData = gameData.Units[(int)Units.OVERLORD];
                unitMinerals = unitMinerals - (int)overlordData.MineralCost;
                // Logger.Info("Name = {0};  minerals = {1}; vespene = {2}", unitData.Name, unitMinerals, unitVespene);
            }

            // The overlord transport cost comes out as a new overlord cost but should be 25 for both types instead.
            if (unitType == Units.OVERLORD_TRANSPORT)
            {
                unitMinerals = 25;
                unitVespene = 25;
                // Logger.Info("Name = {0};  minerals = {1}; vespene = {2}", unitData.Name, unitMinerals, unitVespene);
            }

            //Logger.Info("Name = {0};  minerals = {1}; vespene = {2}", unitData.Name, unitMinerals, unitVespene);
            return (minerals >= unitMinerals) && (vespene >= unitVespene);
        }

        // Check and see if we can construct a structure or unit.
        // Can tell this function to only check if we have the ability to construct the unit and not check the resources or supply.
        public bool CanConstruct(uint unitType, bool ignoreResourceSupply = false)
        {
            // Check if we have the structures to build the unit.
            // Do we spawning pools for the unit? 
            if (Units.NeedSpawningPool.Contains(unitType))
            {
                if (!HasUnits(Units.SPAWNING_POOL)) return false;
            }

            // Do we lairs or hives for the unit? 
            if (Units.NeedLairOrHive.Contains(unitType))
            {
                if (!HasUnits(Units.LairsAndHives)) return false;
            }

            // Do we lairs only for the unit? 
            if (Units.NeedLair.Contains(unitType))
            {
                if (!HasUnits(Units.LAIR)) return false;
            }

            // Do we hive only for the unit? 
            if (Units.NeedHive.Contains(unitType))
            {
                if (!HasUnits(Units.HIVE)) return false;
            }

            // Do we hydralisk dens for the unit? 
            if (Units.NeedHydraliskDen.Contains(unitType))
            {
                if (!HasUnits(Units.HYDRALISK_DEN)) return false;
            }

            // Do we roach warren for the unit? 
            if (Units.NeedRoachWarren.Contains(unitType))
            {
                if (!HasUnits(Units.ROACH_WARREN)) return false;
            }

            // Do we a spire or greater spire for the unit? 
            if (Units.NeedSpireOrGreaterSpire.Contains(unitType))
            {
                if (!HasUnits(Units.Spires)) return false;
            }

            // Do we an infestation pit for the unit? 
            if (Units.NeedInfestationPit.Contains(unitType))
            {
                if (!HasUnits(Units.INFESTATION_PIT)) return false;
            }

            // Do we a lurker den for the unit? 
            if (Units.NeedLurkerDen.Contains(unitType))
            {
                if (!HasUnits(Units.LURKER_DEN)) return false;
            }

            // Do we an ultralisk cavern for the unit? 
            if (Units.NeedUltraliskCavern.Contains(unitType))
            {
                if (!HasUnits(Units.ULTRALISK_CAVERN)) return false;
            }


            if (Units.Structures.Contains(unitType))
            // Preform these checks if it is a structure.
            {
                if (!ignoreResourceSupply)
                {
                    //we need worker for every structure
                    if (!HasUnits(Units.Workers)) return false;
                }

                //we need an RC for any structure
                if (!HasUnits(Units.ResourceCenters)) return false;
            }
            else
            // Preform these check if it is a unit.
            {
                if (!ignoreResourceSupply)
                {
                    // Do we have enough supply?
                    var requiredSupply = gameData.Units[(int)unitType].FoodRequired;
                    if (requiredSupply > (maxSupply - currentSupply))
                        return false;
                }
            }

            var ret = true;

            // Check and see if we can afford to make the unit.
            if (!ignoreResourceSupply)
            {
                ret = CanAfford(unitType);
            }

            return ret;
        }

        // Build an expansion base.
        public void BuildExpansion()
        {
            BuildExpansion(Units.HATCHERY);
        }
    }
}

