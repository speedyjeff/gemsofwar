using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gemsofwar.engine
{
    public enum Direction { None = 0, Up = 1, Left = 2, Right = 3, Down = 4 };
    public struct Move
    {
        public Gem[] Gems { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
        public Direction Direction { get; set; }

        public bool IsValid { get { return Direction != Direction.None; } }
    }
}
