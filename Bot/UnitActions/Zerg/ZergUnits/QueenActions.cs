using Bot.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg.ZergUnits
{
    // --------------------------------------------------------------------------------
    /// <summary>
    /// Object to control the actions of queen units.
    /// </summary>
    // --------------------------------------------------------------------------------
    class QueenActions : ZergActions
    {
        protected QueenToResourceCenterManager queenToResourceCenterManager;

        protected uint creepTumor = Units.CREEP_TUMOR;

        protected int spawnLarva = Abilities.SPAWN_LARVA;
        protected int spawnCreepTumor = Abilities.SPAWN_CREEP_TUMOR_QUEEN;

        // Energy costs.
        protected float spawnLarvaCost = 25;
        protected float spawnCreepTumorCost = 25;
        protected float transfusionCost = 50;

        public QueenActions(ZergController controller, QueenToResourceCenterManager queenToResourceCenterManager) : base(controller)
        {
            this.queenToResourceCenterManager = queenToResourceCenterManager ?? throw new ArgumentNullException(nameof(queenToResourceCenterManager));

            unitType = Units.QUEEN;
            burrowedUnitType = Units.QUEEN_BURROWED;

            burrow = Abilities.BURROW_QUEEN;
            unburrow = Abilities.UNBURROW_QUEEN;
        }

        // ********************************************************************************
        /// <summary>
        /// Preform an Intelligent actions for the unit.
        /// </summary>
        /// <param name="unit">The queen unit.</param>
        /// <param name="saveUnit">Setting to return a value to save resources for a unit.</param>
        /// <param name="saveUpgrade">Setting to return a value to save resources for an upgrade.</param>
        /// <param name="ignoreSaveRandomRoll"> Setting to return to turn off the random chance when deciding to save resources.</param>
        /// <param name="saveFor">If set true it will setup the save resource information.</param>
        /// <param name="doNotUseResources">If set true it will not run any action that requires resources.</param>
        // ********************************************************************************
        public override void PreformIntelligentActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, ref bool ignoreSaveRandomRoll, bool saveFor = false, bool doNotUseResources = false)
        {
            if (!IsUnitType(unit) && (!IsBurrowedUnitType(unit))) return;

            if (IsUnitType(unit))
            {
                var preformAction = MoveToLinkedResourceCenter(unit);

                if (!preformAction)
                {
                    preformAction = SpawnLarva(unit);
                }
            }

            if (IsBurrowedUnitType(unit))
            {
                if (NeedHelpAction(unit, summonHelp: false))
                {
                    Unburrow(unit);
                }
            }
        }

        // ********************************************************************************
        /// <summary>
        /// Preform a random action for the passed unit.
        /// </summary>
        /// <param name="unit">The queen unit.</param>
        /// <param name="saveUnit">Setting to return a value to save resources for a unit.</param>
        /// <param name="saveUpgrade">Setting to return a value to save resources for an upgrade.</param>
        /// <param name="ignoreSaveRandomRoll"> Setting to return to turn off the random chance when deciding to save resources.</param>
        /// <param name="saveFor">If set true it will setup the save resource information.</param>
        /// <param name="doNotUseResources">If set true it will not run any action that requires resources.</param>
        // ********************************************************************************
        public override void PreformRandomActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, ref bool ignoreSaveRandomRoll, bool saveFor = false, bool doNotUseResources = false)
        {
            if (!IsUnitType(unit) && (!IsBurrowedUnitType(unit))) return;

            if (IsUnitType(unit))
            {
                var randomAction = random.Next(4);

                switch (randomAction)
                {
                    case 0:
                        Burrow(unit);
                        break;
                    case 1:
                        SpawnLarva(unit);
                        break;
                    case 2:
                        MoveToLinkedResourceCenter(unit);
                        break;
                    case 3:
                        SpawnCreepTumor(unit);
                        break;
                }
            }

            if (IsBurrowedUnitType(unit))
            {
                if (NeedHelpAction(unit, summonHelp: false))
                {
                    Unburrow(unit);
                }
            }
        }

        // ********************************************************************************
        /// <summary>
        /// Have the queen spawn larva in the closest resource center.
        /// </summary>
        /// <param name="unit">The queen unit.</param>
        /// <returns>True if it was able to make the command.</returns>
        // ********************************************************************************
        public bool SpawnLarva(Unit unit)
        {
            if (!IsUnitType(unit)) return false;

            if (IsBusy(unit)) return false;

            if (unit.energyCurrent < spawnLarvaCost) return false;

            var resourceCenter = GetAssignedResourceCenter(unit);

            if (resourceCenter == null) return false;

            unit.UseAbility(spawnLarva, targetUnit: resourceCenter);

            controller.LogIfSelectedUnit(unit, "Queen {0} spawning larva at {1} @ {2} / {3}",
                unit.tag, resourceCenter.name, resourceCenter.position.X, resourceCenter.position.Y);

            return true;
        }

        // ********************************************************************************
        /// <summary>
        /// Moves the queen to the resource center it is linked to. <para/>
        /// If there is no resource center linked to or it was destroyed link to another one that needs it.
        /// </summary>
        /// <param name="unit">The queen unit.</param>
        /// <returns>True if moving to a resource center.</returns>
        // ********************************************************************************
        public bool MoveToLinkedResourceCenter(Unit unit)
        {
            if (!IsUnitType(unit)) return false;

            if (IsBusy(unit)) return false;

            var resourceCenter = GetAssignedResourceCenter(unit);

            if (resourceCenter == null) return false;

            // Do not move if the queen can see it.
            if (unit.GetDistance(resourceCenter.position) <= unit.sight) return false;

            unit.Move(resourceCenter.position);

            controller.LogIfSelectedUnit(unit, "Moving queen {0} back to {1} @ {2} / {3}",
                unit.tag, resourceCenter.name, resourceCenter.position.X, resourceCenter.position.Y);

            return true;
        }

        // ********************************************************************************
        /// <summary>
        /// Get the resource center the queen is assigned to.
        /// </summary>
        /// <param name="unit">The queen unit.</param>
        /// <returns>The resource center assigned to or null if there is not one.</returns>
        // ********************************************************************************
        private Unit GetAssignedResourceCenter(Unit unit)
        {
            var queenLink = queenToResourceCenterManager.FindLinkByQueen(unit.tag, createNewLink: true);

            Unit resourceCenter = null;

            if (queenLink != null)
            {
                resourceCenter = controller.GetUnitByTag(queenLink.tag2);
            }

            return resourceCenter;
        }

        // ********************************************************************************
        /// <summary>
        /// Has the queen spawn a creep tumor at a location. <para/>
        /// If doing random it will try to get a location equal to randomCreepTumorPlacementTries variable.
        /// </summary>
        /// <param name="unit">Queen unit.</param>
        /// <param name="targetPosition">To spawn if not supplied it will be a random position.</param>
        /// <returns>True if it is able to spawn the creep tumor.</returns>
        // ********************************************************************************
        public bool SpawnCreepTumor(Unit unit, Vector3 targetPosition = new Vector3())
        {
            if (!IsUnitType(unit)) return false;

            if (IsBusy(unit)) return false;

            if (unit.energyCurrent < spawnCreepTumorCost) return false;

            var positionOK = false;

            // Get a random position if not supplied.
            if (targetPosition == Vector3.Zero)
            {
                targetPosition = GetRandomSpawnCreepTurmorPositio(unit);

                if (targetPosition != Vector3.Zero)
                {
                    positionOK = true;
                }
            }
            else
            {
                positionOK = controller.CanPlace(creepTumor, targetPosition);
            }

            if (!positionOK) return false;

            unit.UseAbility(spawnCreepTumor, targetPosition: targetPosition);

            controller.LogIfSelectedUnit(unit, "Queen {0} is spawning a creep tumor @ {1} / {2}", unit.tag, targetPosition.X, targetPosition.Y);

            return true;
        }
    }
}
