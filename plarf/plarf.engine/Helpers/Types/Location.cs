using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine.Helpers.Types
{
    public class Location
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Location(double x, double y) { X = x; Y = y; }

        public Location Offset(double x, double y) => new Location(X + x, Y + y);

        public static double Distance(Location l1, Location l2) =>
            Math.Sqrt((l1.X - l2.X) * (l1.X - l2.X) + (l1.Y - l2.Y) * (l1.Y - l2.Y));

        public double Distance(Location l) => Distance(this, l);

        public override string ToString() => "(" + X + "," + Y + ")";

        public override bool Equals(object obj)
        {
            if (obj is Location)
            {
                var other = (Location)obj;
                return other.X == X && other.Y == Y;
            }
            else
                return false;
        }

        public static bool operator ==(Location a, Location b) => a.Equals(b);

        public static bool operator !=(Location a, Location b) => !a.Equals(b);

        public override int GetHashCode()
        {
            var h = BitConverter.DoubleToInt64Bits(X * 23 + Y);
            return (int)(h >> 32 ^ (h & 0xFFFFFFFF));
        }
    }
}
