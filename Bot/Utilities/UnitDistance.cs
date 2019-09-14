using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Utilities
{
    // --------------------------------------------------------------------------------
    /// <summary>
    /// An object to store the unit and its distance from a location.
    /// </summary>
    // --------------------------------------------------------------------------------
    class UnitDistance : IComparable<UnitDistance>
    {
        private Unit unit;
        private double distance;

        public Unit Unit { get => unit; set => unit = value; }
        public double Distance { get => distance; set => distance = value; }

        // ********************************************************************************
        /// <summary>
        /// Compare the distance of two unit distances.
        /// </summary>
        /// <param name="compareUnitDistance"> The unit distance to compare.</param>
        /// <returns>The result of the compare.</returns>
        // ********************************************************************************
        public int CompareTo(UnitDistance compareUnitDistance)
        {
            if (compareUnitDistance == null)
            {
                return 1;
            }
            else
            {
                return this.Distance.CompareTo(compareUnitDistance.Distance);
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
            return "Tag = " + Unit.tag + ", Name = " + Unit.name + ", distance = " + Distance;
        }
    }
}
