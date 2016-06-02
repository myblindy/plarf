﻿using MoonSharp.Interpreter;
using Plarf.Engine.Actors;
using Plarf.Engine.AI;
using Plarf.Engine.Helpers;
using Plarf.Engine.Helpers.Exceptions;
using Plarf.Engine.Helpers.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine.GameObjects
{
    public class World : IRunnable
    {
        public Size Size { get; private set; }

        public void Initialize(Size worldsize)
        {
            Size = worldsize;
            Placeables = new List<Placeable>();
            Actors = new List<Actor>();
            MarkedResources = new HashSet<Resource>();
            ActorCentralIntelligence = new ActorCentralIntelligence();
        }

        public List<Placeable> Placeables { get; private set; }

        public Placeable AddPlaceable(IPlaceableTemplate template, int x, int y) => AddPlaceable(template, new Location(x, y));
        public Placeable AddPlaceable(IPlaceableTemplate template, Location location)
        {
            if (GetPlaceables((int)location.X, (int)location.Y).Any())
                throw new LocationAlreadyInUseException(location);

            var placeable = template.CreatePlaceableInstance(location);
            Placeables.Add(placeable);
            return placeable;
        }

        public ISet<Resource> MarkedResources { get; private set; }
        public void MarkResourceForHarvest(Resource res, bool mark = true)
        {
            if (mark)
            {
                if (MarkedResources.Add(res))
                    ActorCentralIntelligence.AddResourceJob(res);
            }
            else
            {
                if (MarkedResources.Remove(res))
                    ActorCentralIntelligence.RemoveResourceJob(res);
            }
        }

        public List<Actor> Actors { get; private set; }

        public Actor AddActor(Actor template, double x, double y) => AddActor(template, new Location(x, y));

        public Actor AddActor(Actor template, Location location)
        {
            var actor = template.CreateActorInstance(location);
            Actors.Add(actor);
            return actor;
        }

        public ActorCentralIntelligence ActorCentralIntelligence { get; private set; }

        public IEnumerable<Placeable> GetPlaceables(int x, int y) => Placeables.Where(r => r.ContainsPoint(x, y));
        public IEnumerable<Placeable> GetPlaceables(int x, int y, int w, int h) => Placeables.Where(r => r.Intersects(x, y, w, h));

        internal bool IsLocationValid(Location loc)
        {
            return loc.X >= 0 && loc.Y >= 0 && loc.X < Size.Width && loc.Y < Size.Height;
        }

        [MoonSharpHidden]
        public void Run(TimeSpan t)
        {
            foreach (var placeable in Placeables)
                placeable.Run(t);

            foreach (var actor in Actors)
                actor.Run(t);
        }
    }
}
