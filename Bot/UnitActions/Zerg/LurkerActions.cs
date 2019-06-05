﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions.Zerg
{
    class LurkerActions : ZergActions
    {
        public LurkerActions(ControllerDefault controller) : base(controller)
        {
            unitType = Units.LURKER;
            burrowedUnitType = Units.LURKER_BURROWED;

            burrow = Abilities.BURROW_LURKER;
            unburrow = Abilities.UNBURROW_LURKER;
        }
    }
}
