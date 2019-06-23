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
    /// A class that will figure of the distance for the location.
    /// </summary>
    // --------------------------------------------------------------------------------
    class LocationDistance : IComparable<LocationDistance>
    {
        public Vector3 location;
        public Double distance;

        // ********************************************************************************
        /// <summary>
        /// Compare two location distances.
        /// </summary>
        /// <param name="compareLocationDistance">The location distance to compare.</param>
        /// <returns>The order of the distances.</returns>
        // ********************************************************************************
        public int CompareTo(LocationDistance compareLocationDistance)
        {
            if (compareLocationDistance == null)
            {
                return 1;
            }
            else
            {
                return this.distance.CompareTo(compareLocationDistance.distance);
            }
        }

        // ********************************************************************************
        /// <summary>
        /// Coverts the location distance to a string.
        /// </summary>
        /// <returns>The string version.</returns>
        // ********************************************************************************
        public override String ToString()
        {
            return "Location = " + location + ", distance = " + distance;
        }
    }
}
