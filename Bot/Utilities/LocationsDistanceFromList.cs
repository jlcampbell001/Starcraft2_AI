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
    /// A list of locations from a location in order of closest to farthest.
    /// </summary>
    // --------------------------------------------------------------------------------
    class LocationsDistanceFromList
    {
        public Vector3 fromLocation;

        public List<LocationDistance> toLocations = new List<LocationDistance>();

        // ********************************************************************************
        /// <summary>
        /// Constructor for the location distance from list.
        /// </summary>
        /// <param name="fromLocation">The from location to compare other distances to.</param>
        /// <returns>A new locations distance from list object.</returns>
        // ********************************************************************************
        public LocationsDistanceFromList(Vector3 fromLocation)
        {
            this.fromLocation = fromLocation;
        }

        // ********************************************************************************
        /// <summary>
        /// Gets the distance from the set location to the passed.
        /// </summary>
        /// <param name="location">The location to compare.</param>
        /// <returns>The distance between the locations.</returns>
        // ********************************************************************************
        private Double getDistanceforLocation(Vector3 location)
        {
            var distance = 0.0;

            if (fromLocation != null)
            {
                distance = Vector3.Distance(location, fromLocation);
            }
            return distance;
        }

        // ********************************************************************************
        /// <summary>
        /// Updates the locations distances and sorts them.
        /// </summary>
        // ********************************************************************************
        public void UpdateDistances()
        {
            foreach (var toLocation in toLocations)
            {
                toLocation.distance = getDistanceforLocation(toLocation.location);
            }

            toLocations.Sort();
        }

        // ********************************************************************************
        /// <summary>
        /// Add a location and distance to the list.
        /// </summary>
        /// <param name="location">The location to add.</param>
        /// <param name="sortAfter">If true sort the list after the add.</param>
        // ********************************************************************************
        public void AddLocation(Vector3 location, bool sortAfter = true)
        {
            var toLocation = new LocationDistance();
            toLocation.location = location;
            toLocation.distance = getDistanceforLocation(location);

            toLocations.Add(toLocation);

            if (sortAfter)
            {
                toLocations.Sort();
            }
        }

        // ********************************************************************************
        /// <summary>
        /// Add a list of locations and distances to the list.
        /// </summary>
        /// <param name="locations">The location to add.</param>
        // ********************************************************************************
        public void AddLocation(List<Vector3> locations)
        {
            foreach (var location in locations)
            {
                AddLocation(location, sortAfter: false);
            }

            if (locations.Count > 0)
            {
                toLocations.Sort();
            }
        }

        // ********************************************************************************
        /// <summary>
        /// Add the locations of a list of units.
        /// </summary>
        /// <param name="units">A list of units to get positions to add.</param>
        // ********************************************************************************
        public void AddLocation(List<Unit> units)
        {
            foreach (var unit in units)
            {
                AddLocation(unit.position, sortAfter: false);
            }

            if (units.Count > 0)
            {
                toLocations.Sort();
            }
        }

        // ********************************************************************************
        /// <summary>
        /// The string versions.
        /// </summary>
        /// <returns>The string version.</returns>
        // ********************************************************************************
        public override string ToString()
        {
            var result = "From Location = " + fromLocation + " {" + Environment.NewLine;
            foreach (var toLocation in toLocations)
            {
                result = result + toLocation + "; " + Environment.NewLine;
            }
            result = result + "}";

            return result;
        }
    }
}
