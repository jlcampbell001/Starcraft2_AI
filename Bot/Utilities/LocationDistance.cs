using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Utilities
{
    class LocationDistance : IComparable<LocationDistance>
    {
        public Vector3 location;
        public Double distance;

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

        override
        public String ToString()
        {
            return "Location = " + location + ", distance = " + distance;
        }
    }
}
