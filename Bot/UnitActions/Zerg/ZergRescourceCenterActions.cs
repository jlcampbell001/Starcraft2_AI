using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg
{
    class ZergRescourceCenterActions : ZergActions
    {
        protected int researchBurrow = Abilities.RESEARCH_BURROW;

        protected uint queen = Units.QUEEN;

        public enum ResearchBurrowResult { Success, NotUnitType, AlreadyHas, IsResearching, CanNotAfford, UnitBusy, NoGasGysersStructures };

        public enum BirthQueenResult { Success, NotUnitType, UnitBusy, CanNotConstruct };

        public ZergRescourceCenterActions(ZergController controller) : base(controller)
        {
        }

        // Check and see if the unit is busy.
        override
            public bool IsBusy(Unit unit)
        {
            if (unit.buildProgress != 1) return true;

            if (unit.order.AbilityId != 0) return true;

            return false;
        }

        // Researches the burrow ability.
        // returns 0 - success.
        public ResearchBurrowResult ResearchBurrow(Unit unit)
        {
            if (!IsUnitType(unit)) return ResearchBurrowResult.NotUnitType;

            if (controller.HasUpgrade(burrowUpgrade)) return ResearchBurrowResult.AlreadyHas;


            if (controller.IsResearchingUpgrade(researchBurrow, Units.ResourceCenters)) return ResearchBurrowResult.IsResearching;

            if (controller.GetTotalCount(Units.GasGeysersStructures) == 0) return ResearchBurrowResult.NoGasGysersStructures;

            if (!controller.CanAffordUpgrade(researchBurrow)) return ResearchBurrowResult.CanNotAfford;

                if (IsBusy(unit)) return ResearchBurrowResult.UnitBusy;

            unit.Research(researchBurrow);

            return ResearchBurrowResult.Success;
        }

        // Create a queen.
        public BirthQueenResult BirthQueen(Unit unit)
        {
            if (!IsUnitType(unit)) return BirthQueenResult.NotUnitType;

            if (IsBusy(unit)) return BirthQueenResult.UnitBusy;

            if (!controller.CanConstruct(queen)) return BirthQueenResult.CanNotConstruct;

            unit.Train(Units.QUEEN);
            return BirthQueenResult.Success;
        }

        // Ask for help if being attack.
        override
            public void NeedHelpAction(Unit unit)
        {
            var enemyAttackers = controller.GetPotentialAttackers(unit);

            if (enemyAttackers.Count > 0)
            {
                SummonHelp(unit, enemyAttackers[0], idleArmyOnly: false, includeNearByWorkers: true);
                Logger.Info("{0} is under attack by {1} and summons help.", unit.name, enemyAttackers[0].name);
            }
        }
}
}
