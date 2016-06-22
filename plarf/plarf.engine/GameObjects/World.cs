using MoonSharp.Interpreter;
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
            MarkedResources = new HashSet<Resource>();
            ActorCentralIntelligence = new ActorCentralIntelligence();
        }

        public List<Placeable> Placeables { get; private set; }

        public Placeable AddPlaceable(IPlaceableTemplate template, int x, int y, int w = 0, int h = 0) =>
            AddPlaceable(template, new Location(x, y), w == 0 && h == 0 ? null : new Size(w, h));
        public Placeable AddPlaceable(IPlaceableTemplate template, Location loc, Size sz = null)
        {
            if (sz == null)
            {
                if (GetPlaceables((int)loc.X, (int)loc.Y).Any())
                    throw new LocationAlreadyInUseException(loc);
            }
            else
            {
                if (GetPlaceables((int)loc.X, (int)loc.Y, sz.Width, sz.Height).Any())
                    throw new LocationAlreadyInUseException(loc);
            }

            var placeable = template.CreatePlaceableInstance(loc, sz);
            Placeables.Add(placeable);
            placeable.OnAdded();
            return placeable;
        }

        public ISet<Resource> MarkedResources { get; private set; }
        public void MarkResourceForHarvest(Resource res, bool mark = true)
        {
            if (mark)
            {
                if (MarkedResources.Add(res))
                    ActorCentralIntelligence.AddResourceJob(res, JobPriority.Low);
            }
            else
            {
                if (MarkedResources.Remove(res))
                    ActorCentralIntelligence.RemoveResourceJob(res);
            }
        }

        public Actor AddActor(Actor template, string name, double x, double y) => AddActor(template, name, new Location(x, y));

        public Actor AddActor(Actor template, string name, Location location)
        {
            var actor = template.CreateActorInstance(name, location);
            Placeables.Add(actor);
            return actor;
        }

        public ActorCentralIntelligence ActorCentralIntelligence { get; private set; }

        public ResourceBundle StoredResources =>
            Placeables.OfType<Building>().Where(b => b.Function == BuildingFunction.Storage).Aggregate(new ResourceBundle(), (acc, rb) => acc + rb.Resources);

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

            Placeables.RemoveAll(p => p.Dead);
        }
    }
}
