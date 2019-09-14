using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Utilities
{
    // --------------------------------------------------------------------------------
    /// <summary>
    /// This object links two units by their tags together.
    /// </summary>
    // --------------------------------------------------------------------------------
    class UnitsLink
    {
        private ulong tag1 = 0;
        private ulong tag2 = 0;

        public ulong Tag1 { get => tag1; set => tag1 = value; }
        public ulong Tag2 { get => tag2; set => tag2 = value; }
    }
}
