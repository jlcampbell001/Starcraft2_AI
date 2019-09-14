using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg
{
    // --------------------------------------------------------------------------------
    /// <summary>
    /// Object to control Zerg actions.
    /// </summary>
    // --------------------------------------------------------------------------------
    class ZergActions : UnitActions
    {
        protected new ZergController controller;

        protected uint burrowedUnitType = 0;

        // Upgrades
        protected int burrowUpgrade = Abilities.BURROW;

        // Abilities
        protected int burrow = 0;
        protected int unburrow = 0;

        protected int randomCreepTumorPlacementTries = 20;
        protected double minSpawnTumorDistance = 5.0;
        protected double minDistanceFromResourceCenter = 5.0;

        public ZergActions(ZergController controller) : base(controller)
        {
            this.controller = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        // ********************************************************************************
        /// <summary>
        /// Add this object to a unit actions list.
        /// </summary>
        /// <param name="unitActionsList">Unit actions list to add object to.</param>
        // ********************************************************************************
        public override void SetupUnitActionsList(ref UnitActionsList unitActionsList)
        {
            unitActionsList.AddUnitAction(this, unitType);

            if (burrowedUnitType != 0)
            {
                unitActionsList.AddUnitAction(this, burrowedUnitType);
            }
        }

        // ********************************************************************************
        /// <summary>
        /// Checks to see if the passed unit is of the burrowed unit type that is action controller will deal with.
        /// </summary>
        /// <param name="unit">The burrow unit to check.</param>
        /// <returns>True if the unit is the burrow version type.</returns>
        // ********************************************************************************
        public bool IsBurrowedUnitType(Unit unit)
        {
            var isUnitType = false;

            if (unit.unitType == burrowedUnitType)
            {
                isUnitType = true;
            }

            return isUnitType;
        }

        // ********************************************************************************
        /// <summary>
        /// Used the units burrow command if it has it.
        /// </summary>
        /// <param name="unit">The unit to burrow.</param>
        // ********************************************************************************
        public void Burrow(Unit unit)
        {
            if (burrow == 0) return;

            if (!IsUnitType(unit)) return;

            if (!controller.HasUpgrade(burrowUpgrade)) return;

            unit.UseAbility(burrow);

            controller.LogIfSelectedUnit(unit, "Burrow {0} {1} @ {2} / {3}", unit.name, unit.tag, unit.position.X, unit.position.Y);
        }

        // ********************************************************************************
        /// <summary>
        /// Used the units unburrow command if it has it.
        /// </summary>
        /// <param name="unit">The unit to unborrow.</param>
        // ********************************************************************************
        public void Unburrow(Unit unit)
        {
            if (burrow == 0) return;

            if (!IsBurrowedUnitType(unit)) return;

            if (!controller.HasUpgrade(burrowUpgrade)) return;

            unit.UseAbility(unburrow);

            controller.LogIfSelectedUnit(unit, "Unburrow {0} {1} @ {2} / {3}", unit.name, unit.tag, unit.position.X, unit.position.Y);
        }

        // ********************************************************************************
        /// <summary>
        /// Get a random position to spawn a creep tumor. <para/>
        /// It will not be within a certain distance of another creep tumor or a resource center. </param>
        /// If the ending position is passed it will get the location between the unit and ending points.
        /// </summary>
        /// <param name="unit">The unit that will spawn the creep tumor.</param>
        /// <param name="endingPosition">An ending position.</param>
        /// <returns>The position or Zero position if one can not be figured out.</returns>
        // ********************************************************************************
        public Vector3 GetRandomSpawnCreepTumorPosition(Unit unit, Vector3 endingPosition = new Vector3())
        {
            Vector3 spawnPosition = Vector3.Zero;

            var sight = (int)unit.sight;

            var resourceCenters = controller.GetUnits(Units.ResourceCenters);
            var creepTumors = controller.GetUnits(Units.CREEP_TUMOR_BURROWED);

            for (var i = 0; i < randomCreepTumorPlacementTries; i++)
            {
                var targetPosition = new Vector3();

                if (endingPosition == Vector3.Zero)
                {
                    targetPosition = controller.GetRandomLocation(unit.position, -sight, sight, -sight, sight);
                }
                else
                {
                    targetPosition = controller.GetRandomLocationBetween2Points(unit.position, endingPosition, sight);
                }

                var canPlace = controller.CanPlace(Units.CREEP_TUMOR, targetPosition);
                var closestCreepTumor = controller.GetClosestUnit(targetPosition, creepTumors, minSpawnTumorDistance);
                var closestResourceCenter = controller.GetClosestUnit(targetPosition, resourceCenters, minDistanceFromResourceCenter);

                if (canPlace && closestCreepTumor == null && closestResourceCenter == null)
                {
                    spawnPosition = targetPosition;
                    break;
                }
            }

            return spawnPosition;
        }

        // ********************************************************************************
        /// <summary>
        /// Get a random location between the unit and an enemy position for a creep tumor. <para/>
        /// If a creep tumor is already near the enemy position it will just pick a random position around the unit.
        /// </summary>
        /// <param name="unit">The unit to start with.</param>
        /// <returns>The position or Zero position if one can not be figured out.</returns>
        // ********************************************************************************
        public Vector3 GetRandomSpawnCreepTumorPositionEnemyPosition(Unit unit)
        {
            var targetPosition = Vector3.Zero;

            var enemyPosition = controller.enemyLocations[Random.Next(controller.enemyLocations.Count)];

            if (controller.GetClosestUnit(enemyPosition, Units.CREEP_TUMOR, unit.sight) == null)
            {
                targetPosition = GetRandomSpawnCreepTumorPosition(unit, enemyPosition);
            }
            else
            {

                targetPosition = GetRandomSpawnCreepTumorPosition(unit);
            }

            return targetPosition;
        }

        // ********************************************************************************
        /// <summary>
        /// Get a random location between the unit and an expansion position for a creep tumor. <para/>
        /// It will try for the closest expansion position but if a creep tumor is near there it will go to the next one. <para/>
        /// If a creep tumor is already near the enemy position it will just pick a random position around the unit.
        /// </summary>
        /// <param name="unit">The unit to start with.</param>
        /// <returns>The position or Zero position if one can not be figured out.</returns>
        // ********************************************************************************
        public Vector3 GetRandomSpawnCreepTumorPositionExpansionPosition(Unit unit)
        {
            var targetPosition = Vector3.Zero;

            var expansionPosition = Vector3.Zero;

            foreach (var expansion in controller.expansionPositions.toLocations)
            {
                if (controller.GetClosestUnit(expansion.location, Units.CREEP_TUMOR, unit.sight) == null)
                {
                    expansionPosition = expansion.location;
                    break;

                }
            }

            targetPosition = GetRandomSpawnCreepTumorPosition(unit, expansionPosition);

            return targetPosition;
        }
    }
}
