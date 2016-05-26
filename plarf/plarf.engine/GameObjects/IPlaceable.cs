using Plarf.Engine.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine.GameObjects
{
    public class Placeable
    {
        public Location Location { get; set; }
        public Size Size { get; set; }

        public bool ContainsPoint(int x, int y)
        {
            return Location.X >= x && Location.X + Size.Width - 1 <= x && Location.Y >= y && Location.Y + Size.Height - 1 <= y;
        }
    }
}
