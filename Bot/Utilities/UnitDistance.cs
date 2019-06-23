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
        public Unit unit;
        public Double distance;

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
                return this.distance.CompareTo(compareUnitDistance.distance);
            }
        }

        // ********************************************************************************
        /// <summary>
        /// The string version.
        /// </summary>
        /// <returns>A string version.</returns>
        // ********************************************************************************
        public override String ToString()
        {
            return "Tag = " + unit.tag + ", Name = " + unit.name + ", distance = " + distance;
        }
    }
}
