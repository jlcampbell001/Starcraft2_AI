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
        private Vector3 fromLocation;

        private List<UnitDistance> toUnits = new List<UnitDistance>();

        public Vector3 FromLocation { get => fromLocation; set => fromLocation = value; }
        internal List<UnitDistance> ToUnits { get => toUnits; set => toUnits = value; }

        // ********************************************************************************
        /// <summary>
        /// Create a units distance from list for the passed from location.
        /// </summary>
        /// <param name="fromLocation">The from location to work with.</param>
        /// <returns>A new units distance from list object.</returns>
        // ********************************************************************************
        public UnitsDistanceFromList(Vector3 fromLocation)
        {
            this.FromLocation = fromLocation;
        }

        // ********************************************************************************
        /// <summary>
        /// Get the distance a unit is from a location.
        /// </summary>
        /// <param name="unit">The unit to look up.</param>
        /// <returns>The distance the unit is from the location.</returns>
        // ********************************************************************************
        private double GetDistanceforUnit(Unit unit)
        {
            var distance = 0.0;

            if (FromLocation != null)
            {
                distance = unit.GetDistance(FromLocation);
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
            foreach (var toUnit in ToUnits)
            {
                toUnit.Distance = GetDistanceforUnit(toUnit.Unit);
            }

            ToUnits.Sort();
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
            toUnit.Unit = unit;
            toUnit.Distance = GetDistanceforUnit(unit);

            ToUnits.Add(toUnit);

            if (sortAfter)
            {
                ToUnits.Sort();
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
                ToUnits.Sort();
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
            var result = "From Location = " + FromLocation + " {" + Environment.NewLine;
            foreach (var toUnit in ToUnits)
            {
                result = result + toUnit + "; " + Environment.NewLine;
            }
            result += "}";

            return result;
        }
    }
}
