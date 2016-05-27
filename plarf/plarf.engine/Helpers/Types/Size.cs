using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine.Helpers.Types
{
    public struct Size
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public Size(int w, int h) { Width = w; Height = h; }

        public override string ToString() => Width + "x" + Height;
    }
}
