using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions
{
    class UnitActions
    {
        protected ControllerDefault controller;

        protected uint unitType = 0;

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
    }
}
