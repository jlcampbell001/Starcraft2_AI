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
            unitActionsList.addUnitAction(this, unitType);

            if (burrowedUnitType != 0)
            {
                unitActionsList.addUnitAction(this, burrowedUnitType);
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
        /// It will not be within a certain distance of another creep tumor or a resource center.
        /// </summary>
        /// <param name="unit">The unit that will spawn the creep tumor.</param>
        /// <returns>The position or null if one can not be figured out.</returns>
        // ********************************************************************************
        public Vector3 GetRandomSpawnCreepTurmorPositio(Unit unit)
        {
            Vector3 spawnPosition = Vector3.Zero;

            var sight = (int)unit.sight;

            var resourceCenters = controller.GetUnits(Units.ResourceCenters);
            var creepTumors = controller.GetUnits(Units.CREEP_TUMOR_BURROWED);

            for (var i = 0; i < randomCreepTumorPlacementTries; i++)
            {
                var targetPosition = controller.GetRandomLocation(unit.position, -sight, sight, -sight, sight);

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
    }
}
