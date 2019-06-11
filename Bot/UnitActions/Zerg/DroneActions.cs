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
            // Attack any enemy workers in sight.
            AttackEnemyWorkers(unit);

            // If a drone may be under attack ask for help.
            NeedHelpAction(unit);
        }

        public override void PreformRandomActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, bool saveFor = false, bool doNotUseResources = false)
        {
            // Attack any enemy workers in sight.
            AttackEnemyWorkers(unit);

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
            var enemyWorkers = controller.GetUnits(Units.Workers, alliance: Alliance.Enemy, onlyVisible: true);

            if (enemyWorkers.Count > 0)
            {
                UnitsDistanceFromList unitsDistanceFromList = new UnitsDistanceFromList(unit.position);
                unitsDistanceFromList.AddUnits(enemyWorkers);

                var enemyWorker = unitsDistanceFromList.toUnits[0];
                if (enemyWorker.distance <= unit.sight)
                {
                    unit.Attack(unit, enemyWorker.unit.position);
                    Logger.Info("Drone {0} is attacking {1}.", unit.tag, enemyWorker.unit.name);
                }
            }
        }

    }
}
