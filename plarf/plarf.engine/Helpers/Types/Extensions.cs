using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine.Helpers.Types
{
    public static class Extensions
    {
        public static bool EqualsI(this string a, string b) => string.Equals(a, b, StringComparison.InvariantCultureIgnoreCase);
    }
}
