using Plarf.Engine.Helpers.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine.Helpers.Exceptions
{
    public class LocationAlreadyInUseException : Exception
    {
        public Location Location { get; private set; }

        public LocationAlreadyInUseException(Location location, string msg = null)
            : base("Location already in use: " + location + (string.IsNullOrWhiteSpace(msg) ? "" : ": " + msg))
        {
            Location = location;
        }
    }
}
