using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg.ZergStructures
{
    class CreepTumorActions : ZergStructureActions
    {
        protected uint creepTumor = Units.CREEP_TUMOR;

        protected int spawnCreepTumorID = Abilities.SPAWN_CREEP_TUMOR;

        public CreepTumorActions(ZergController controller) : base(controller)
        {
            unitType = Units.CREEP_TUMOR;
            burrowedUnitType = Units.CREEP_TUMOR_BURROWED;
        }

        // ********************************************************************************
        /// <summary>
        /// Preform an Intelligent actions for the unit.
        /// </summary>
        /// <param name="unit">The creep tumor unit.</param>
        /// <param name="saveUnit">Setting to return a value to save resources for a unit.</param>
        /// <param name="saveUpgrade">Setting to return a value to save resources for an upgrade.</param>
        /// <param name="ignoreSaveRandomRoll"> Setting to return to turn off the random chance when deciding to save resources.</param>
        /// <param name="saveFor">If set true it will setup the save resource information.</param>
        /// <param name="doNotUseResources">If set true it will not run any action that requires resources.</param>
        // ********************************************************************************
        public override void PreformIntelligentActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, ref bool ignoreSaveRandomRoll, bool saveFor = false, bool doNotUseResources = false)
        {
            base.PreformIntelligentActions(unit, ref saveUnit, ref saveUpgrade, ref ignoreSaveRandomRoll, saveFor, doNotUseResources);

            if (controller.UnitHasAbility(spawnCreepTumorID, unit))
            {
                var targetPosition = Vector3.Zero;

                if (Random.Next(100) < 50)
                {
                    targetPosition = GetRandomSpawnCreepTumorPositionEnemyPosition(unit);
                }
                else
                {
                    targetPosition = GetRandomSpawnCreepTumorPositionExpansionPosition(unit);
                }

                SpawnCreepTumor(unit, targetPosition);
            }
        }

        // ********************************************************************************
        /// <summary>
        /// Preform a random action for the passed unit.
        /// </summary>
        /// <param name="unit">The creep tumor unit.</param>
        /// <param name="saveUnit">Setting to return a value to save resources for a unit.</param>
        /// <param name="saveUpgrade">Setting to return a value to save resources for an upgrade.</param>
        /// <param name="ignoreSaveRandomRoll"> Setting to return to turn off the random chance when deciding to save resources.</param>
        /// <param name="saveFor">If set true it will setup the save resource information.</param>
        /// <param name="doNotUseResources">If set true it will not run any action that requires resources.</param>
        // ********************************************************************************
        public override void PreformRandomActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, ref bool ignoreSaveRandomRoll, bool saveFor = false, bool doNotUseResources = false)
        {
            base.PreformRandomActions(unit, ref saveUnit, ref saveUpgrade, ref ignoreSaveRandomRoll, saveFor, doNotUseResources);

            SpawnCreepTumor(unit);
        }

        // ********************************************************************************
        /// <summary>
        /// Spawn a creep tumor at a location. <para/>
        /// If doing random it will try to get a location equal to randomCreepTumorPlacementTries variable.
        /// </summary>
        /// <remarks>
        /// Note: Need to figure out a way to tell if the cool down is still running and if they have already unused the ability.
        /// </remarks>
        /// <param name="unit">Creep tumor unit.</param>
        /// <param name="targetPosition">To spawn if not supplied it will be a random position.</param>
        /// <returns>True if it is able to spawn the creep tumor.</returns>
        // ********************************************************************************
        public bool SpawnCreepTumor(Unit unit, Vector3 targetPosition = new Vector3())
        {
            if (!IsUnitType(unit) && !IsBurrowedUnitType(unit)) return false;

            if (IsBusy(unit)) return false;

            if (!controller.UnitHasAbility(spawnCreepTumorID, unit)) return false;

            var positionOK = false;

            // Get a random position if not supplied.
            if (targetPosition == Vector3.Zero)
            {
                targetPosition = GetRandomSpawnCreepTumorPosition(unit);

                if (targetPosition != Vector3.Zero)
                {
                    positionOK = true;
                }
            }
            else
            {
                positionOK = controller.CanPlace(creepTumor, targetPosition);
            }

            if (!positionOK)
                return false;

            unit.UseAbility(spawnCreepTumorID, targetPosition: targetPosition);

            controller.LogIfSelectedUnit(unit, "Creep tumor {0} is spawning a creep tumor @ {1} / {2}", unit.tag, targetPosition.X, targetPosition.Y);

            return true;
        }
    }
}
