using Plarf.Engine.Actors;
using Plarf.Engine.GameObjects;
using Plarf.Engine.Helpers;
using Plarf.Engine.Helpers.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine.AI
{
    public enum JobType
    {
        Harvest,
        DropResources,
        StepMove,
        Invalid
    }

    public class Job
    {
        public JobType Type { get; set; }
        public Placeable Target { get; set; }
        public bool IsAvailable(Human human)
        {
            if (Type == JobType.Harvest)
                return !(PlarfGame.Instance.World.Actors.OfType<Human>().Count(a => a.AssignedJob == this) == ((Resource)Target).MaxWorkers)
                    && !human.FullForResourceClass(((Resource)Target).ResourceClass);
            if (Type == JobType.DropResources)
                return human.ResourcesCarried.Any(r => r.Value > 0);
            throw new InvalidOperationException();
        }

        public override string ToString() => Type + " " + Target;
    }

    [DebuggerDisplay("{Type} @ {Location} on {Placeable}")]
    public struct JobStep
    {
        public Location Location;
        public JobType Type;
        public Placeable Placeable;

        public JobStep(Location location, JobType type, Placeable placeable)
        {
            Location = location;
            Type = type;
            Placeable = placeable;
        }

        public JobStep(JobType type) : this(null, type, null) { }

        public override string ToString() => Type + " @ " + Location + (Placeable == null ? "" : " on " + Placeable);
    }

    public class ActorCentralIntelligence
    {
        private List<Job> Jobs = new List<Job>();

        [DebuggerDisplay("{Location}")]
        class AStarNode : AStarSearch.IHasNeighbours<AStarNode>
        {
            public readonly Location Location;

            public AStarNode(Location location) { Location = location; }

            public IEnumerable<AStarNode> Neighbours
            {
                get
                {
                    var placeables = PlarfGame.Instance.World.GetPlaceables((int)Location.X - 1, (int)Location.Y - 1, 2, 2).ToArray();

                    var loc = Location.Offset(-1, -1);
                    if (PlarfGame.Instance.World.IsLocationValid(loc) && !placeables.Any(p => !p.Passable && p.Location == loc))
                        yield return new AStarNode(loc);
                    loc = Location.Offset(0, -1);
                    if (PlarfGame.Instance.World.IsLocationValid(loc) && !placeables.Any(p => !p.Passable && p.Location == loc))
                        yield return new AStarNode(loc);
                    loc = Location.Offset(1, -1);
                    if (PlarfGame.Instance.World.IsLocationValid(loc) && !placeables.Any(p => !p.Passable && p.Location == loc))
                        yield return new AStarNode(loc);
                    loc = Location.Offset(-1, 0);
                    if (PlarfGame.Instance.World.IsLocationValid(loc) && !placeables.Any(p => !p.Passable && p.Location == loc))
                        yield return new AStarNode(loc);
                    loc = Location.Offset(1, 0);
                    if (PlarfGame.Instance.World.IsLocationValid(loc) && !placeables.Any(p => !p.Passable && p.Location == loc))
                        yield return new AStarNode(loc);
                    loc = Location.Offset(-1, 1);
                    if (PlarfGame.Instance.World.IsLocationValid(loc) && !placeables.Any(p => !p.Passable && p.Location == loc))
                        yield return new AStarNode(loc);
                    loc = Location.Offset(0, 1);
                    if (PlarfGame.Instance.World.IsLocationValid(loc) && !placeables.Any(p => !p.Passable && p.Location == loc))
                        yield return new AStarNode(loc);
                    loc = Location.Offset(1, 1);
                    if (PlarfGame.Instance.World.IsLocationValid(loc) && !placeables.Any(p => !p.Passable && p.Location == loc))
                        yield return new AStarNode(loc);
                }
            }

            public override bool Equals(object obj) => Location == ((AStarNode)obj).Location;

            public override int GetHashCode() => Location.GetHashCode();
        }

        public void AddResourceJob(Resource res)
        {
            Jobs.Add(new Job
            {
                Type = JobType.Harvest,
                Target = res
            });
        }

        public void RemoveResourceJob(Resource res)
        {
            Jobs.RemoveAll(j => j.Target == res);

            foreach (var actor in PlarfGame.Instance.World.Actors.OfType<Human>())
                if (actor.AssignedJob.Target == res)
                    actor.AssignedJob = null;
        }

        public void AddStorageJob(Building b)
        {
            Jobs.Add(new Job
            {
                Type = JobType.DropResources,
                Target = b
            });
        }

        public Job GetAvailableJob(Human human, Job last) =>
            Jobs.OrderBy(j => human.Location.Distance(j.Target.Location)).FirstOrDefault(j => j.IsAvailable(human));

        public JobStep[] GetJobStepsFromJob(Job job, Actor actor)
        {
            if (job == null)
                return null;

            Func<AStarSearch.Path<AStarNode>> buildpath = () =>
            {
                var destset = new HashSet<AStarNode>();
                for (int x = (int)job.Target.Location.X; x < job.Target.Location.X + job.Target.Size.Width; ++x)
                    for (int y = (int)job.Target.Location.Y; y < job.Target.Location.Y + job.Target.Size.Height; ++y)
                        destset.Add(new AStarNode(new Location(x, y)));

                return AStarSearch.FindPath(
                        new AStarNode(actor.Location),
                        destset,
                        (n1, n2) => Location.Distance(n1.Location, n2.Location),
                        n => Location.Distance(n.Location, new Location(job.Target.Location.X + job.Target.Size.Width / 2, job.Target.Location.Y + job.Target.Size.Height / 2)));
            };

            if (job.Type == JobType.Harvest || job.Type == JobType.DropResources)
            {
                var path = buildpath();

                return path.Select((n, i) => new JobStep(n.Location, JobType.StepMove, null))
                    .SkipWhile(w => job.Target.ContainsPoint(w.Location))                                  // skip anything inside the target, stop just outside
                    .Reverse().Skip(1)                                                                     // reverse since the path returned is from the resource to us, then skip the actor's location (first path item)
                    .Concat(Enumerable.Repeat(new JobStep(job.Target.Location, job.Type, job.Target), 1))  // after we get to the target, queue an action step (harvest, drop, etc)
                    .ToArray();
            }

            throw new InvalidOperationException();
        }
    }
}
