using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.UnitActions
{
    class UnitActionListItem
    {
        private uint unitType;
        private UnitActions unitAction;

        public uint UnitType { get => unitType; set => unitType = value; }
        internal UnitActions UnitAction { get => unitAction; set => unitAction = value; }
    }
}
