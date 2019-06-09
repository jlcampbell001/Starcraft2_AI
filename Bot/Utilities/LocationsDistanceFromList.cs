using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Utilities
{
    // A list of locations from a location in order of closest to farthest.
    class LocationsDistanceFromList
    {
       public Vector3 fromLocation;

       public List<LocationDistance> toLocations = new List<LocationDistance>();

        public LocationsDistanceFromList(Vector3 fromLocation)
        {
            this.fromLocation = fromLocation;
        }

        // Gets the distance from the set location to the passed.
        private Double getDistanceforLocation(Vector3 location)
        {
            var distance = 0.0;

            if (fromLocation != null)
            {
                distance = Vector3.Distance(location, fromLocation);
            }
            return distance;
        }

        // Updates the locations distances and sorts them.
        public void UpdateDistances()
        {
            foreach (var toLocation in toLocations)
            {
                toLocation.distance = getDistanceforLocation(toLocation.location);
            }

            toLocations.Sort();
        }

        // Add a location and distance to the list.
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

        // Add a list of locations and distances to the list.
        public void AddLocation(List<Vector3> locations)
        {
            foreach(var location in locations)
            {
                AddLocation(location, sortAfter: false);
            }

            if (locations.Count > 0)
            {
                toLocations.Sort();
            }
        }

        // Add the locations of a list of units.
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

        override
            public string ToString()
        {
            var result = "From Location = " + fromLocation + " {" + Environment.NewLine;
            foreach(var toLocation in toLocations)
            {
                result = result + toLocation + "; " + Environment.NewLine;
            }
            result = result + "}";

            return result;
        }
    }
}
