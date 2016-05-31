using Plarf.Engine.Helpers;
using Plarf.Engine.Helpers.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine.GameObjects
{
    public abstract class Placeable : IRunnable
    {
        public Location Location { get; set; }
        public Size Size { get; set; }
        public bool Passable { get; set; }

        public bool ContainsPoint(int x, int y)
        {
            return x >= Location.X && x <= Location.X + Size.Width - 1 && y >= Location.Y && y <= Location.Y + Size.Height - 1;
        }

        public abstract void Run(TimeSpan t);

        internal bool Intersects(int x, int y, int w, int h)
        {
            return !(x > Location.X + Size.Width || x + w < Location.X || y > Location.Y + Size.Height || y + h < Location.Y);
        }
    }
}
