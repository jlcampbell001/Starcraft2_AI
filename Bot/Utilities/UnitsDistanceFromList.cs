using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Utilities
{
    // --------------------------------------------------------------------------------
    /// <summary>
    /// A list of units in order of closest to furthest from a location.
    /// </summary>
    // --------------------------------------------------------------------------------
    class UnitsDistanceFromList
    {
        public Vector3 fromLocation;

        public List<UnitDistance> toUnits = new List<UnitDistance>();

        // ********************************************************************************
        /// <summary>
        /// Create a units distance from list for the passed from location.
        /// </summary>
        /// <param name="fromLocation">The from location to work with.</param>
        /// <returns>A new units distance from list object.</returns>
        // ********************************************************************************
        public UnitsDistanceFromList(Vector3 fromLocation)
        {
            this.fromLocation = fromLocation;
        }

        // ********************************************************************************
        /// <summary>
        /// Get the distance a unit is from a location.
        /// </summary>
        /// <param name="unit">The unit to look up.</param>
        /// <returns>The distance the unit is from the location.</returns>
        // ********************************************************************************
        private Double getDistanceforUnit(Unit unit)
        {
            var distance = 0.0;

            if (fromLocation != null)
            {
                distance = unit.GetDistance(fromLocation);
            }
            return distance;
        }

        // ********************************************************************************
        /// <summary>
        /// Update all the distances the units are from the location.
        /// </summary>
        // ********************************************************************************
        public void UpdateDistances()
        {
            foreach (var toUnit in toUnits)
            {
                toUnit.distance = getDistanceforUnit(toUnit.unit);
            }

            toUnits.Sort();
        }

        // ********************************************************************************
        /// <summary>
        /// Add a unit to the list.
        /// </summary>
        /// <param name="unit">The unit to add.</param>
        /// <param name="sortAfter">If true sort the list after the add.</param>
        // ********************************************************************************
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

        // ********************************************************************************
        /// <summary>
        /// A list of units to add.
        /// </summary>
        /// <param name="units">A list of units.</param>
        // ********************************************************************************
        public void AddUnits(List<Unit> units)
        {
            foreach (var unit in units)
            {
                AddUnit(unit, sortAfter: false);
            }

            if (units.Count > 0)
            {
                toUnits.Sort();
            }
        }

        // ********************************************************************************
        /// <summary>
        /// The string version.
        /// </summary>
        /// <returns>A string version.</returns>
        // ********************************************************************************
        public override string ToString()
        {
            var result = "From Location = " + fromLocation + " {" + Environment.NewLine;
            foreach (var toUnit in toUnits)
            {
                result = result + toUnit + "; " + Environment.NewLine;
            }
            result = result + "}";

            return result;
        }
    }
}
