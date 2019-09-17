using Bot.UnitActions.Zerg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg.ZergUnits.OverlordsAndOverseers
{
    // --------------------------------------------------------------------------------
    /// <summary>
    /// Object that can control the actions of overlord units.
    /// </summary>
    // --------------------------------------------------------------------------------
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

        // ********************************************************************************
        /// <summary>
        /// Preform an Intelligent actions for the unit.
        /// </summary>
        /// <param name="unit">The overlord unit.</param>
        /// <param name="saveUnit">Setting to return a value to save resources for a unit.</param>
        /// <param name="saveUpgrade">Setting to return a value to save resources for an upgrade.</param>
        /// <param name="ignoreSaveRandomRoll"> Setting to return to turn off the random chance when deciding to save resources.</param>
        /// <param name="saveFor">If set true it will setup the save resource information.</param>
        /// <param name="doNotUseResources">If set true it will not run any action that requires resources.</param>
        // ********************************************************************************
        public override void PreformIntelligentActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, ref bool ignoreSaveRandomRoll, bool saveFor = false,
            bool doNotUseResources = false)
        {
            if (!IsUnitType(unit)) return;

            // If under attack ask for help.
            NeedHelpAction(unit);

            var preformingAction = false;

            // Create an overseer if there is no overseers.
            if (!doNotUseResources)
            {
                var overseerCount = controller.GetTotalCount(Units.OVERSEER);
                var transportCount = controller.GetTotalCount(Units.OVERLORD_TRANSPORT);

                var buildOverseer = false;
                var buildTransport = false;

                if (overseerCount == 0)
                {
                    buildOverseer = true;
                }
                else if (transportCount == 0)
                {
                    buildTransport = true;
                }
                else if (Random.Next(100) < 50)
                {
                    buildOverseer = true;
                }
                else
                {
                    buildTransport = true;
                }

                if (buildOverseer)
                {
                    var overseerResult = MorphToOverseer(unit);
                    if (saveFor && overseerResult == OverseerResult.CanNotAfford)
                    {
                        saveUnit = overseer;
                        ignoreSaveRandomRoll = true;
                    }
                }
                else if (buildTransport)
                {
                    var mutateResult = MorphToOverlordTransport(unit);
                    if (saveFor && mutateResult == MorphToTransportResult.CanNotAfford)
                    {
                        saveUnit = overlordTransport;
                        ignoreSaveRandomRoll = true;
                    }
                }
                else
                {
                    // Lets try and generate creep or stop generating.
                    if (Random.Next(100) < 50)
                    {
                        preformingAction = GenerateCreep(unit);
                    }
                    else
                    {
                        GenerateCreepStop(unit);
                    }
                }
            }

            // Move the overlord around.
            if (!preformingAction)
            {
                MoveAroundResourceCenter(unit);
            }
        }

        // ********************************************************************************
        /// <summary>
        /// Preform a random action for the passed unit.
        /// </summary>
        /// <param name="unit">The overlord unit.</param>
        /// <param name="saveUnit">Setting to return a value to save resources for a unit.</param>
        /// <param name="saveUpgrade">Setting to return a value to save resources for an upgrade.</param>
        /// <param name="ignoreSaveRandomRoll"> Setting to return to turn off the random chance when deciding to save resources.</param>
        /// <param name="saveFor">If set true it will setup the save resource information.</param>
        /// <param name="doNotUseResources">If set true it will not run any action that requires resources.</param>
        // ********************************************************************************        
        public override void PreformRandomActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, ref bool ignoreSaveRandomRoll, bool saveFor = false,
        bool doNotUseResources = false)
        {
            if (!IsUnitType(unit)) return;

            // If under attack ask for help.
            NeedHelpAction(unit);

            var preformingAction = false;

            var randomAction = Random.Next(4);

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
                    preformingAction = GenerateCreep(unit);
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
            if (!preformingAction)
            {
                MoveAroundResourceCenter(unit);
            }
        }

        // ********************************************************************************
        /// <summary>
        /// Move randomly around a resource center.
        /// </summary>
        /// <param name="unit"> The overlord to move.</param>
        // ********************************************************************************
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

        // ********************************************************************************
        /// <summary>
        /// Morph to an overseer.
        /// </summary>
        /// <param name="unit">The overlord to morph.</param>
        /// <returns>An OverseerResult.</returns>
        // ********************************************************************************
        public OverseerResult MorphToOverseer(Unit unit)
        {
            if (!IsUnitType(unit)) return OverseerResult.NotUnitType;

            if (IsBusy(unit)) return OverseerResult.UnitBusy;

            if (unit.cargoUsed != 0) return OverseerResult.HasCargo;

            if (!controller.CanConstruct(overseer, ignoreResourceSupply: true)) return OverseerResult.CanNotConstruct;

            if (!controller.CanAfford(overseer)) return OverseerResult.CanNotAfford;

            unit.Train(overseer);

            controller.LogIfSelectedUnit(unit, "Overlord {2} morphing to overseer @ {0} / {1}.", unit.position.X, unit.position.Y, unit.tag);

            return OverseerResult.Success;
        }

        // ********************************************************************************
        /// <summary>
        /// Start generating creep. <para />
        /// Note: Need to figure out a way to tell if the unit is generating creep.  It is not in the list of orders while they are doing that.
        /// </summary>
        /// <param name="unit">The overlord to generate creep.</param>
        /// <returns>True if generating creep.</returns>
        // ********************************************************************************
        public bool GenerateCreep(Unit unit)
        {
            if (!IsUnitType(unit)) return false;

            if (IsBusy(unit)) return false;

            if (!controller.HasUnits(Units.LairsAndHives)) return false;

            unit.UseAbility(generateCreepOn);

            controller.LogIfSelectedUnit(unit, "Overseer {0} generating creep @ {1} / {2}.", unit.tag, unit.position.X, unit.position.Y);
            return true;
        }

        // ********************************************************************************
        /// <summary>
        /// Stop generating creep. <para />
        /// Note: Need to figure out a way to tell if the unit is generating creep.  It is not in the list of orders while they are doing that.
        /// </summary>
        /// <param name="unit">The overlord to stop generate creep.</param>
        /// <returns>True if stopping.</returns>
        // ********************************************************************************
        public bool GenerateCreepStop(Unit unit)
        {
            if (!IsUnitType(unit)) return false;

            if (IsBusy(unit)) return false;

            if (!controller.HasUnits(Units.LairsAndHives)) return false;

            unit.UseAbility(generateCreepOff);

            controller.LogIfSelectedUnit(unit, "Overseer {0} stopped generating creep @ {1} / {2}.", unit.tag, unit.position.X, unit.position.Y);
            return true;
        }

        // ********************************************************************************
        /// <summary>
        /// Morph to an overlord transport.
        /// </summary>
        /// <param name="unit">The overlord to morph.</param>
        /// <returns>MorphToTransportResult</returns>
        // ********************************************************************************
        public MorphToTransportResult MorphToOverlordTransport(Unit unit)
        {
            if (!IsUnitType(unit)) return MorphToTransportResult.NotUnitType;

            if (IsBusy(unit)) return MorphToTransportResult.UnitBusy;

            if (!controller.CanConstruct(overlordTransport, ignoreResourceSupply: true)) return MorphToTransportResult.CanNotConstruct;

            if (!controller.CanAfford(overlordTransport)) return MorphToTransportResult.CanNotAfford;

            unit.UseAbility(morphToTransport);

            controller.LogIfSelectedUnit(unit, "Overlord {2} morphing to transport @ {0} / {1}.", unit.position.X, unit.position.Y, unit.tag);

            return MorphToTransportResult.Success;
        }
    }
}
