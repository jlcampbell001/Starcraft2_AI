using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Utilities
{
    class UnitsDistanceFromList
    {
       public Vector3 fromLocation;

       public List<UnitDistance> toUnits = new List<UnitDistance>();

        public UnitsDistanceFromList(Vector3 fromLocation)
        {
            this.fromLocation = fromLocation;
        }

        private Double getDistanceforUnit(Unit unit)
        {
            var distance = 0.0;

            if (fromLocation != null)
            {
                distance = unit.GetDistance(fromLocation);
            }
            return distance;
        }
        public void UpdateDistances()
        {
            foreach (var toUnit in toUnits)
            {
                toUnit.distance = getDistanceforUnit(toUnit.unit);
            }

            toUnits.Sort();
        }

        public void AddUnit(Unit unit, bool sortAfter = true)
        {
            var toUnit = new UnitDistance();
            toUnit.unit = unit;
            toUnit.distance = getDistanceforUnit(unit);

            toUnits.Add(toUnit);

            if (sortAfter)
            {
                toUnits.Sort();
            }
        }

        public void AddUnits(List<Unit> units)
        {
            foreach(var unit in units)
            {
                AddUnit(unit, sortAfter: false);
            }

            if (units.Count > 0)
            {
                toUnits.Sort();
            }
        }

        override
            public string ToString()
        {
            var result = "From Location = " + fromLocation + " {" + Environment.NewLine;
            foreach(var toUnit in toUnits)
            {
                result = result + toUnit + "; " + Environment.NewLine;
            }
            result = result + "}";

            return result;
        }
    }
}
