using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Utilities
{
    class LocationsDistanceFromList
    {
       public Vector3 fromLocation;

       public List<LocationDistance> toLocations = new List<LocationDistance>();

        public LocationsDistanceFromList(Vector3 fromLocation)
        {
            this.fromLocation = fromLocation;
        }

        private Double getDistanceforLocation(Vector3 location)
        {
            var distance = 0.0;

            if (fromLocation != null)
            {
                distance = Vector3.Distance(location, fromLocation);
            }
            return distance;
        }
        public void UpdateDistances()
        {
            foreach (var toLocation in toLocations)
            {
                toLocation.distance = getDistanceforLocation(toLocation.location);
            }

            toLocations.Sort();
        }

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
