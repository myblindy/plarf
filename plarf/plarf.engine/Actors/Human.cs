using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plarf.Engine.Helpers.Types;

namespace Plarf.Engine.Actors
{
    public class Human : Actor
    {
        public override Actor CreateActorInstance(Location location) => new Human
        {
            Location = location
        };
    }
}
