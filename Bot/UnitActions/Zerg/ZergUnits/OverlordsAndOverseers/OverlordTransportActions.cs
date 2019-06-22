using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg.ZergUnits.OverlordsAndOverseers
{
    // --------------------------------------------------------------------------------
    /// <summary>
    /// Object that can control the actions of overlord transport units.
    /// </summary>
    // --------------------------------------------------------------------------------
    class OverlordTransportActions : OverlordActions
    {
        protected int loadTransport = Abilities.LOAD_OVERLORD;
        protected int unloadTransportAll = Abilities.UNLOADAll_OVERLORD;
        protected int unloadTransportOneUnit = Abilities.UNLOADUNIT_OVERLORD;

        public OverlordTransportActions(ZergController controller) : base(controller)
        {
            unitType = Units.OVERLORD_TRANSPORT;
        }

        // ********************************************************************************
        /// <summary>
        /// Preform an Intelligent actions for the unit.
        /// </summary>
        /// <param name="unit">The overlord transport unit.</param>
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

            // Move the overlord around.
            MoveAroundResourceCenter(unit);
        }

        // ********************************************************************************
        /// <summary>
        /// Preform a random action for the passed unit.
        /// </summary>
        /// <param name="unit">The overlord transport unit.</param>
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

            if (unit.isSelected)
            {
                var a = 1;
            }

            var randomAction = random.Next(5);

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
                    preformingAction = LoadTransport(unit);
                    break;
                case 4:
                    preformingAction = UnloadTransport(unit);
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
        /// Load a unit into the transport. <para/>
        /// Will look for the closest army unit to load up.
        /// </summary>
        /// Note: I need a way to tell if the overlord is producing creep as this will say it will load a unit but it will not.
        /// <param name="unit">The unit that is the transport.</param>
        /// <returns>True if it was able to setup the load command.</returns>
        // ********************************************************************************
        public bool LoadTransport(Unit unit)
        {
            if (!IsUnitType(unit)) return false;

            if (IsBusy(unit)) return false;

            if (unit.cargoUsed == unit.cargoMax) return false;

            // Get the closest unit and load it.
            var armyUnit = controller.GetClosestUnit(unit, Units.ArmyUnits);

            if (armyUnit == null) return false;

            unit.UseAbility(loadTransport, targetUnit: armyUnit);

            controller.LogIfSelectedUnit(unit, "OverLoad {0} loading {1} @ {2} / {3} ", unit.tag, armyUnit.name, armyUnit.position.X, armyUnit.position.Y);

            return true;
        }

        // ********************************************************************************
        /// <summary>
        /// Unloads units from the overlord transport <para/>
        /// Will unload all the units unless told not to.  It will then unload the first unit in its passenger list.
        /// </summary>
        /// <param name="unit">The overlord transport to unload from.</param>
        /// <param name="unloadAll">Unload all the passengers if set true.</param>
        /// <returns>Returns true if it will be unload passengers.</returns>
        // ********************************************************************************
        public bool UnloadTransport(Unit unit, bool unloadAll = true)
        {
            if (!IsUnitType(unit)) return false;

            if (IsBusy(unit)) return false;

            if (unit.cargoUsed == 0) return false;

            var unloadingPassengers = false;

            if (unloadAll)
            {
                unit.UseAbility(unloadTransportAll, targetPosition: unit.position);

                controller.LogIfSelectedUnit(unit, "OverLoad {0} unloading all units @ {1} / {2} ", unit.tag, unit.position.X, unit.position.Y);

                unloadingPassengers = true;
            }
            else
            {
                // Unload the first unit.
                // This is not working as the tag can not be found.
                var foundUnit = controller.GetUnitByTag(unit.passangers[0].Tag);

                if (foundUnit != null)
                {
                    unit.UseAbility(unloadTransportOneUnit, targetUnit: foundUnit);

                    controller.LogIfSelectedUnit(unit, "OverLoad {0} unloading {1} @ {2} / {3} ", unit.tag, foundUnit.name, unit.position.X, unit.position.Y);

                    unloadingPassengers = true;
                }
            }

            return unloadingPassengers;
        }

    }
}
