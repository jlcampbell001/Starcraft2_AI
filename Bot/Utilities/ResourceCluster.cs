using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Utilities
{
    // --------------------------------------------------------------------------------
    /// <summary>
    /// List of resources that are clustered to each other.
    /// </summary>
    // --------------------------------------------------------------------------------
    class ResourceCluster
    {
        private List<Unit> resources = new List<Unit>();

        public List<Unit> Resources { get => resources; set => resources = value; }
    }
}
