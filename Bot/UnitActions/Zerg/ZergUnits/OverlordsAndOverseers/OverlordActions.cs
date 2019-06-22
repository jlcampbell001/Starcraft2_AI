using Bot.UnitActions.Zerg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg.ZergUnits.OverlordsAndOverseers
{
    class OverlordActions : ZergActions
    {
        protected uint overseer = Units.OVERSEER;

        protected int generateCreepOn = Abilities.GENERATE_CREEP_ON;
        protected int generateCreepOff = Abilities.GENERATE_CREEP_OFF;

        protected int morphToTransport = Abilities.MORPH_OVERLORD_TRANSPORT;
        protected uint overlordTransport = Units.OVERLORD_TRANSPORT;

        public enum OverseerResult { Success, NotUnitType, UnitBusy, CanNotConstruct, CanNotAfford, HasCargo };
        public enum MorphToTransportResult { Success, NotUnitType, UnitBusy, CanNotConstruct, CanNotAfford };

        public OverlordActions(ZergController controller) : base(controller)
        {
            unitType = Units.OVERLORD;
        }

        public override void PreformIntelligentActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, ref bool ignoreSaveRandomRoll, bool saveFor = false,
            bool doNotUseResources = false)
        {
            if (!IsUnitType(unit)) return;

            // If under attack ask for help.
            NeedHelpAction(unit);

            // Move the overlord around.
            MoveAroundResourceCenter(unit);
        }

        public override void PreformRandomActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, ref bool ignoreSaveRandomRoll, bool saveFor = false,
            bool doNotUseResources = false)
        {
            if (!IsUnitType(unit)) return;

            // If under attack ask for help.
            NeedHelpAction(unit);

            var generatingCreep = false;

            var randomAction = random.Next(4);

            switch (randomAction)
            {
                case 0:
                    if (doNotUseResources) return;

                    var overseerResult = MorphToOverseer(unit);
                    if (saveFor && overseerResult == OverseerResult.CanNotAfford)
                    {
                        saveUnit = overseer;
                        ignoreSaveRandomRoll = true;
                    }
                    break;
                case 1:
                    generatingCreep = GenerateCreep(unit);
                    break;
                case 2:
                    GenerateCreepStop(unit);
                    break;
                case 3:
                    if (doNotUseResources) return;

                    var mutateResult = MorphToOverlordTransport(unit);
                    if (saveFor && mutateResult == MorphToTransportResult.CanNotAfford)
                    {
                        saveUnit = overlordTransport;
                        ignoreSaveRandomRoll = true;
                    }
                    break;
            }

            // Move the overlord around.
            if (!generatingCreep)
            {
                MoveAroundResourceCenter(unit);
            }
        }

        // Move randomly around a resource center.
        public void MoveAroundResourceCenter(Unit unit)
        {
            if (!IsUnitType(unit)) return;

            if (IsBusy(unit)) return;

            // Get the closest resource center.
            var resourceCenter = controller.GetClosestUnit(unit, Units.ResourceCenters);

            if (resourceCenter != null)
            {
                var sight = (int)unit.sight;
                var moveTo = controller.GetRandomLocation(resourceCenter.position, -sight, sight, -sight, sight);

                unit.Move(moveTo);
            }
        }

        // Morph to an overseer.
        public OverseerResult MorphToOverseer(Unit unit)
        {
            if (!IsUnitType(unit)) return OverseerResult.NotUnitType;

            if (IsBusy(unit)) return OverseerResult.UnitBusy;

            if (unit.cargoUsed != 0) return OverseerResult.HasCargo;

            if (!controller.CanConstruct(overseer, ignoreResourceSupply: true)) return OverseerResult.CanNotConstruct;

            if (!controller.CanAfford(overseer)) return OverseerResult.CanNotAfford;

            unit.Train(overseer);

            Logger.Info("Overlord {2} morphing to overseer @ {0} / {1}.", unit.position.X, unit.position.Y, unit.tag);

            return OverseerResult.Success;
        }

        // Start generating creep.
        // Note: Need to figure out a way to tell if the unit is generating creep.  It is not in the list of orders while they are doing that.
        public bool GenerateCreep(Unit unit)
        {
            if (!IsUnitType(unit)) return false;

            if (IsBusy(unit)) return false;

            if (!controller.HasUnits(Units.LairsAndHives)) return false;

            unit.UseAbility(generateCreepOn);

            controller.LogIfSelectedUnit(unit, "Overseer {0} generating creep @ {1} / {2}.", unit.tag, unit.position.X, unit.position.Y);
            return true;
        }

        // Stop generating creep.
        // Note: Need to figure out a way to tell if the unit is generating creep.  It is not in the list of orders while they are doing that.
        public bool GenerateCreepStop(Unit unit)
        {
            if (!IsUnitType(unit)) return false;

            if (IsBusy(unit)) return false;

            if (!controller.HasUnits(Units.LairsAndHives)) return false;

            unit.UseAbility(generateCreepOff);

            controller.LogIfSelectedUnit(unit, "Overseer {0} stopped generating creep @ {1} / {2}.", unit.tag, unit.position.X, unit.position.Y);
            return true;
        }

        // Morph to an overlord transport.
        public MorphToTransportResult MorphToOverlordTransport(Unit unit)
        {
            if (!IsUnitType(unit)) return MorphToTransportResult.NotUnitType;

            if (IsBusy(unit)) return MorphToTransportResult.UnitBusy;

            if (!controller.CanConstruct(overlordTransport, ignoreResourceSupply: true)) return MorphToTransportResult.CanNotConstruct;

            if (!controller.CanAfford(overlordTransport)) return MorphToTransportResult.CanNotAfford;

            unit.UseAbility(morphToTransport);

            Logger.Info("Overlord {2} morphing to transport @ {0} / {1}.", unit.position.X, unit.position.Y, unit.tag);

            return MorphToTransportResult.Success;
        }
    }
}
