using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

        public int chanceToLoadUnload = 30;

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

            var preformingAction = false;

            // If under attack ask for help.
            NeedHelpAction(unit, ref preformingAction);

            if (!preformingAction)
            {
                var buildOverseer = false;

                // If there are no overseers morph into one.
                if (!doNotUseResources)
                {
                    var overseerCount = controller.GetTotalCount(Units.OVERSEER);

                    if (overseerCount == 0)
                    {
                        buildOverseer = true;
                    }
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
                else
                {
                    var loading = false;
                    var unloading = false;

                    if (unit.cargoUsed == unit.cargoMax)
                    {
                        unloading = true;
                    } else if (random.Next(100) < chanceToLoadUnload)
                    {
                        var tempCalculation = ((decimal)unit.cargoUsed / (decimal)unit.cargoMax * 100m);
                        var cargoBaseChance = (int)(tempCalculation);
                        if (unit.cargoUsed !=0 && random.Next(100) < Math.Min(50, cargoBaseChance))
                        {
                            unloading = true;
                        } else
                        {
                            loading = true;
                        }
                    }

                    if (loading)
                    {
                        preformingAction = LoadTransport(unit);
                    } else if (unloading)
                    {
                        var possibleLocations = controller.expansionPositions.toLocations;
                        var position = possibleLocations[random.Next(possibleLocations.Count())].location;
                        preformingAction = UnloadTransport(unit, targetPosition: position);
                    }
                    // Lets try and generate creep or stop generating.
                    else if (random.Next(100) < 50)
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

            var preformingAction = false;

            // If under attack ask for help.
            NeedHelpAction(unit, ref preformingAction);

            if (!preformingAction)
            {
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
            var armyUnit = controller.GetClosestUnit(unit, Units.ArmyUnits, isNotBurrowed: true);

            if (armyUnit == null) return false;

            // Do not load a selected unit because I am watching it and I lose the selected when they get picked up.
            if (armyUnit.isSelected) return false;

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
        /// <param name="targetPosition">Can pass a position to unload to otherwise it will unload at there the overlord is.</param>
        /// <returns>Returns true if it will be unload passengers.</returns>
        // ********************************************************************************
        public bool UnloadTransport(Unit unit, bool unloadAll = true, Vector3 targetPosition = new Vector3())
        {
            if (!IsUnitType(unit)) return false;

            if (IsBusy(unit)) return false;

            if (unit.cargoUsed == 0) return false;

            var unloadingPassengers = false;

            if (targetPosition == Vector3.Zero)
            {
                targetPosition = unit.position;
            }

            if (unloadAll)
            {
                unit.UseAbility(unloadTransportAll, targetPosition: targetPosition);

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

        // ********************************************************************************
        /// <summary>
        /// Summons help if under attack and if has units in transport unloads them.
        /// </summary>
        /// <param name="unit">overlord transport under attack</param>
        /// <param name="preformAction">If unloading units set the preformAction to true.</param>
        /// <returns>true if under attack</returns>
        // ********************************************************************************
        public bool NeedHelpAction(Unit unit, ref bool preformAction)
        {
            if (!IsUnitType(unit)) return false;

            var underAttack = base.NeedHelpAction(unit);

            if (underAttack && unit.cargoUsed != 0)
            {
                preformAction = UnloadTransport(unit);
            }

            return underAttack;
        }
    }
}
