using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SC2APIProtocol;

namespace Bot.UnitActions
{
    class UnitActions
    {
        protected ControllerDefault controller;
        protected static Random random = new Random();

        protected uint unitType = 0;

        protected double workerHelpDistance = 12.0;

        public enum ResearchResult { Success, NotUnitType, AlreadyHas, IsResearching, CanNotAfford, UnitBusy, NoGasGysersStructures };

        public UnitActions(ControllerDefault controller)
        {
            this.controller = controller ?? throw new ArgumentNullException(nameof(controller));
        }


        // Checks to see if the passed unit is of the unit type that is action controller will deal with.
        public bool IsUnitType(Unit unit)
        {
            var isUnitType = false;

            if (unit.unitType == unitType) {
                isUnitType = true;
            }

            return isUnitType;
        }

        // Add this unit action to the passed unit actions list.
        virtual
        public void SetupUnitActionsList(ref UnitActionsList unitActionsList)
        {
            unitActionsList.addUnitAction(this, unitType);
        }

        // Check and see if the unit is busy.
        virtual
            public bool IsBusy(Unit unit)
        {
            if (unit.order.AbilityId != 0) return true;

            return false;
        }

        // Try and command actions intelligently.
        // This is meant to be overridden.
        virtual
            public void PreformIntelligentActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, bool saveFor = false, bool doNotUseResources = false)
        {

        }

        // Command random actions.
        // This is meant to be overridden.
        virtual
            public void PreformRandomActions(Unit unit, ref uint saveUnit, ref int saveUpgrade, bool saveFor = false, bool doNotUseResources = false)
        {

        }

        public void SummonHelp(Unit unit, Unit attacker = null, bool idleArmyOnly = true, bool includeNearByWorkers = false)
        {
            var army = controller.GetUnits(Units.ArmyUnits);

            if (idleArmyOnly)
            {
                army = controller.GetIdleUnits(army);
            }

            var targetPos = unit.position;

            if (attacker != null)
            {
                targetPos = attacker.position;
            }

            controller.Attack(army, targetPos);

            if (includeNearByWorkers)
            {
                var workers = controller.GetUnits(Units.Workers);

                List<Unit> attackWorkers = new List<Unit>();

                foreach (var worker in workers)
                {
                    if (worker.GetDistance(unit) <= workerHelpDistance)
                    {
                        attackWorkers.Add(worker);
                    }
                }

                controller.Attack(attackWorkers, targetPos);
            }
        }

        // Ask for help if being attack.
        virtual
            public void NeedHelpAction(Unit unit)
        {
            var enemyAttackers = controller.GetPotentialAttackers(unit);

            if (enemyAttackers.Count > 0)
            {
                SummonHelp(unit, enemyAttackers[0]);
                Logger.Info("{0} is under attack by {1} and summons help.", unit.name, enemyAttackers[0].name);
            }
        }
}
}
