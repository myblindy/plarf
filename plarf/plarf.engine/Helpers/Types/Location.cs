using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine.Helpers.Types
{
    public struct Location
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Location(double x, double y) { X = x; Y = y; }

        public override string ToString() => "(" + X + "," + Y + ")";
    }
}
