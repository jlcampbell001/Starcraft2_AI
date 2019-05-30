using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Utilities
{
    class UnitDistance : IComparable<UnitDistance>
    {
        public Unit unit;
        public Double distance;

        public int CompareTo(UnitDistance compareUnitDistance)
        {
            if (compareUnitDistance == null)
            {
                return 1;
            }
            else
            {
                return this.distance.CompareTo(compareUnitDistance.distance);
            }
        }

        override
        public String ToString()
        {
            return "Tag = " + unit.tag + ", Name = " + unit.name + ", distance = " + distance;
        }
    }
}
