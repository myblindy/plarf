﻿using Plarf.Engine.AI;
using Plarf.Engine.GameObjects;
using Plarf.Engine.Helpers.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine.Actors
{
    public abstract class Actor : Placeable
    {
        public string Texture { get; set; }

        public abstract Actor CreateActorInstance(string name, Location location);
    }
}
