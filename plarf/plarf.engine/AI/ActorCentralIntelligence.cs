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
        Harvest,                                                    // harvest a resource
        DropResources,                                              // drops accepted resources held into placeable
        EnterWorkplace,                                             // move to enter workplace
        FeedProduction,                                             // feed a production chain
        Production,                                                 // run the production chain

        StepMove,                                                   // just a step, move to an adjacent cell
        StepRetrieveResources,                                      // just a step, retrieve requested resources

        Invalid                                                     // nothing, idle
    }

    public class Job
    {
        public JobType Type { get; set; }
        public Placeable Target { get; set; }
        public WorkerType WorkerType { get; set; }
        public bool IsAvailable(Human human, bool allowtocarry = false)
        {
            if (!allowtocarry && human.WorkerType != WorkerType)
                return false;

            switch (Type)
            {
                case JobType.Harvest:
                    return !(PlarfGame.Instance.World.Placeables.OfType<Human>().Count(a => a.AssignedJob == this) == ((Resource)Target).MaxWorkers)
                        && !human.FullForResourceClass(((Resource)Target).ResourceClass);
                case JobType.DropResources:
                    return human.ResourcesCarried.Any(r => r.Value > 0);
                case JobType.Production:
                    {
                        var b = (Building)Target;
                        return b.Workers.Any() && b.Resources.ContainsFully(b.ProductionChain.Inputs);
                    }
                case JobType.FeedProduction:
                    return PlarfGame.Instance.World.StoredResources.ContainsAny(((Building)Target).ProductionChain.Inputs);
                case JobType.EnterWorkplace:
                    return human.ChosenWorkplace == Target && !human.InsideWorkplace;
                default:
                    throw new InvalidOperationException();
            }
        }

        public override string ToString() => Type + " " + Target;
    }

    public struct JobStep
    {
        public Location Location;
        public JobType Type;
        public Placeable Placeable;
        public ResourceBundle Resources;

        public JobStep(Location location, JobType type, Placeable placeable, ResourceBundle resources = null)
        {
            Location = location;
            Type = type;
            Placeable = placeable;
            Resources = resources;
        }

        public JobStep(JobType type) : this(null, type, null) { }

        public override string ToString() => Type + " @ " + Location + (Placeable == null ? "" : " on " + Placeable);
    }

    public enum JobPriority { VeryHigh, High, Normal, Low, VeryLow }

    public class ActorCentralIntelligence
    {
        private PriorityList<JobPriority, Job> Jobs = new PriorityList<JobPriority, Job>();

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

        public void AddProductionJob(Building building, JobPriority prod, JobPriority feed, JobPriority workermovetobuilding)
        {
            Jobs.Add(prod, new Job
            {
                Type = JobType.Production,
                Target = building,
                WorkerType = building.WorkerType
            });
            Jobs.Add(feed, new Job
            {
                Type = JobType.FeedProduction,
                Target = building
            });
            Jobs.Add(workermovetobuilding, new Job
            {
                Type = JobType.EnterWorkplace,
                Target = building,
                WorkerType = building.WorkerType
            });
        }

        public void AddResourceJob(Resource res, JobPriority p)
        {
            Jobs.Add(p, new Job
            {
                Type = JobType.Harvest,
                Target = res
            });
        }

        public void RemoveResourceJob(Resource res)
        {
            Jobs.RemoveAll(j => j.Target == res);

            foreach (var actor in PlarfGame.Instance.World.Placeables.OfType<Human>())
                if (actor.AssignedJob.Target == res)
                    actor.AssignedJob = null;
        }

        public void AddStorageJob(Building b, JobPriority p)
        {
            Jobs.Add(p, new Job
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

            Func<Location, Placeable, IEnumerable<JobStep>> buildpath = (from, to) =>
              {
                  var destset = new HashSet<AStarNode>();
                  for (int x = (int)to.Location.X; x < to.Location.X + to.Size.Width; ++x)
                      for (int y = (int)to.Location.Y; y < to.Location.Y + to.Size.Height; ++y)
                          destset.Add(new AStarNode(new Location(x, y)));

                  var path = AStarSearch.FindPath(
                      new AStarNode(from),
                      destset,
                      (n1, n2) => Location.Distance(n1.Location, n2.Location),
                      n => Location.Distance(n.Location, new Location(job.Target.Location.X + job.Target.Size.Width / 2, job.Target.Location.Y + job.Target.Size.Height / 2)));

                  return path.Select((n, i) => new JobStep(n.Location, JobType.StepMove, null))
                      .SkipWhile(w => job.Target.ContainsPoint(w.Location))                                  // skip anything inside the target, stop just outside
                      .Reverse().Skip(1);                                                                    // reverse since the path returned is from the resource to us, then skip the actor's location (first path item)
              };

            if (job.Type == JobType.Harvest || job.Type == JobType.DropResources || job.Type == JobType.EnterWorkplace)
                return buildpath(actor.Location, job.Target)
                    .Concat(new JobStep(job.Target.Location, job.Type, job.Target).ToEnumerable())       // after we get to the target, queue an action step (harvest, drop, etc)
                    .ToArray();
            else if (job.Type == JobType.FeedProduction)
            {
                var b = (Building)job.Target;

                // find the closest storage that contains stuff the production building needs
                // we know there is one or we wouldn't have gotten here
                var closeststorage = PlarfGame.Instance.World.Placeables.OfType<Building>()
                    .Where(w => w.Function == BuildingFunction.Storage && w.Resources.ContainsAny(b.ProductionChain.Inputs))
                    .OrderBy(w => w.Location.Distance(actor.Location))
                    .First();

                // and build the path, actor->storage--(retrieve)-->prod--(prod)-->done
                var tostorage = buildpath(actor.Location, closeststorage).ToArray();
                var toprod = buildpath(tostorage.Last().Location, b);
                return tostorage
                    .Concat(new JobStep(closeststorage.Location, JobType.StepRetrieveResources, closeststorage, b.ProductionChain.Inputs.Intersect(closeststorage.Resources)).ToEnumerable())
                    .Concat(toprod)
                    .Concat(new JobStep(b.Location, JobType.DropResources, b).ToEnumerable())
                    .ToArray();
            }
            else if (job.Type == JobType.Production)
                return new[] { new JobStep(JobType.Production) };                                           // simply start the production step

            throw new InvalidOperationException();
        }
    }
}
