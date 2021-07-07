using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gemsofwar.engine
{
    public enum GemId : int { Empty = 0, G1=1, G2=2, G3=3, G4=4, G5=5, G6=6, G7=7, G8=8, G9=9 }
    // todo effect for DoomSkull10
    public enum GemEffect { None = 0, Lycanthropy = 1, Explode9x9 = 2 }

    public struct Gem
    {
        public GemId Id { get; set; }
        public GemEffect Effect { get; set; }
        public bool CanMatch { get; set; }

        public Gem Copy()
        {
            return new Gem() { Id = Id, CanMatch = CanMatch, Effect = Effect };
        }
    }
}
