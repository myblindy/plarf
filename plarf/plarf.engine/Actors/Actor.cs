using Plarf.Engine.AI;
using Plarf.Engine.Helpers.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine.Actors
{
    public abstract class Actor : IRunnable
    {
        public Location Location { get; set; }
        public string Texture { get; set; }

        public abstract Actor CreateActorInstance(Location location);

        public virtual void Run(TimeSpan t)
        {
        }
    }
}
