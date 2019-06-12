using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bot.Utilities;
using SC2APIProtocol;

namespace Bot.UnitActions.Zerg
{
    //Note: The drones morph into building ability is being handled by the bot and controller instead.
    class DroneActions : ZergActions
    {
        public int BURROW_CHANCE = 10;
        public int UNBURROW_CHANCE = 80;

        public DroneActions(ZergController controller) : base(controller)
        {
            unitType = Units.DRONE;
            burrowedUnitType = Units.DRONE_BURROWED;

            burrow = Abilities.BURROW_DRONE;
            unburrow = Abilities.UNBURROW_DRONE;
        }

        public override void PreformIntelligentActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, bool saveFor = false, bool doNotUseResources = false)
        {
            if (!IsUnitType(unit) && !IsBurrowedUnitType(unit)) return;

            // Attack any enemy workers in sight.
            AttackEnemyWorkers(unit);

            // Do not go to far in attacking an enemy.
            ReturnToBase(unit);

            // If a drone may be under attack ask for help.
            NeedHelpAction(unit);
        }

        public override void PreformRandomActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, bool saveFor = false, bool doNotUseResources = false)
        {
            if (!IsUnitType(unit) && !IsBurrowedUnitType(unit)) return;

            // Attack any enemy workers in sight.
            AttackEnemyWorkers(unit);

            // Do not go to far in attacking an enemy.
            ReturnToBase(unit);

            // If a drone may be under attack ask for help.
            NeedHelpAction(unit);

            if (unit.isBurrowed)
            {
                if (random.Next(100) < UNBURROW_CHANCE)
                {
                    Unburrow(unit);
                }
            } else
            {
                if (!IsBusy(unit) && random.Next(100) < BURROW_CHANCE)
                {
                    Burrow(unit);
                }
            }
        }

        // If an enemy worker can be seen attack it.
        private void AttackEnemyWorkers(Unit unit)
        {
            if (!IsUnitType(unit)) return;

            if (IsBusy(unit) && !Abilities.GatherMinerals.Contains((int)unit.order.AbilityId)) return;

            var enemyWorkers = controller.GetUnits(Units.Workers, alliance: Alliance.Enemy, displayType: DisplayType.Visible);

            var enemyWorker = controller.GetClosestUnit(unit, enemyWorkers, unit.sight);
                if (enemyWorker != null)
                {
                    unit.Attack(unit, enemyWorker.position);
                    Logger.Info("Drone {0} is attacking {1}.", unit.tag, enemyWorker.name);
                }
        }

        // If followed an enemy worker to far return to base.
        private void ReturnToBase(Unit unit)
        {
            if (!IsUnitType(unit)) return;

            if (unit.order.AbilityId == Abilities.ATTACK)
            {
                var resourceCenters = controller.GetUnits(Units.ResourceCenters);

                if (resourceCenters.Count > 0)
                {
                    var resourceCenter = controller.GetClosestUnit(unit, resourceCenters, unit.sight * 3);

                    if (resourceCenter == null)
                    {
                        resourceCenter = controller.GetClosestUnit(unit, resourceCenters);
                        unit.Move(resourceCenter.position);
                    }
                }
            }
        }
    }
}
