using SC2APIProtocol;
using System.Collections.Generic;

namespace Bot
{
    public interface Bot
    {
        IEnumerable<Action> OnFrame();
    }
}