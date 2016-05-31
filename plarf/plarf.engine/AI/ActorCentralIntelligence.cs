﻿using Plarf.Engine.Actors;
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
        StepMove
    }

    [DebuggerDisplay("{Type} {Target}, Available={Available}")]
    public class Job
    {
        public JobType Type { get; set; }
        public Placeable Target { get; set; }
        public bool Available
        {
            get
            {
                if (Type == JobType.Harvest)
                    return !(Game.Instance.World.Actors.OfType<Human>().Count(a => a.AssignedJob == this) == ((Resource)Target).MaxWorkers);
                throw new InvalidOperationException();
            }
        }
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
    }

    public class ActorCentralIntelligence : IRunnable
    {
        private List<Job> Jobs = new List<Job>();

        class AStarNode : AStarSearch.IHasNeighbours<AStarNode>
        {
            public readonly Location Location;

            public AStarNode(Location location) { Location = location; }

            public IEnumerable<AStarNode> Neighbours
            {
                get
                {
                    var placeables = Game.Instance.World.GetPlaceables((int)Location.X - 1, (int)Location.Y - 1, 2, 2).ToArray();

                    var loc = Location.Offset(-1, -1);
                    if (Game.Instance.World.IsLocationValid(loc) && !placeables.Any(p => !p.Passable && p.Location == loc))
                        yield return new AStarNode(loc);
                    loc = Location.Offset(0, -1);
                    if (Game.Instance.World.IsLocationValid(loc) && !placeables.Any(p => !p.Passable && p.Location == loc))
                        yield return new AStarNode(loc);
                    loc = Location.Offset(1, -1);
                    if (Game.Instance.World.IsLocationValid(loc) && !placeables.Any(p => !p.Passable && p.Location == loc))
                        yield return new AStarNode(loc);
                    loc = Location.Offset(-1, 0);
                    if (Game.Instance.World.IsLocationValid(loc) && !placeables.Any(p => !p.Passable && p.Location == loc))
                        yield return new AStarNode(loc);
                    loc = Location.Offset(1, 0);
                    if (Game.Instance.World.IsLocationValid(loc) && !placeables.Any(p => !p.Passable && p.Location == loc))
                        yield return new AStarNode(loc);
                    loc = Location.Offset(-1, 1);
                    if (Game.Instance.World.IsLocationValid(loc) && !placeables.Any(p => !p.Passable && p.Location == loc))
                        yield return new AStarNode(loc);
                    loc = Location.Offset(0, 1);
                    if (Game.Instance.World.IsLocationValid(loc) && !placeables.Any(p => !p.Passable && p.Location == loc))
                        yield return new AStarNode(loc);
                    loc = Location.Offset(1, 1);
                    if (Game.Instance.World.IsLocationValid(loc) && !placeables.Any(p => !p.Passable && p.Location == loc))
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

            foreach (var actor in Game.Instance.World.Actors.OfType<Human>())
                if (actor.AssignedJob.Target == res)
                    actor.AssignedJob = null;
        }

        public Job GetAvailableJob()
        {
            return Jobs.FirstOrDefault(j => j.Available);
        }

        public JobStep[] GetJobStepsFromJob(Job job, Actor actor)
        {
            if (job == null)
                return new JobStep[0];

            if (job.Type == JobType.Harvest)
            {
                var path = AStarSearch.FindPath(
                    new AStarNode(actor.Location),
                    new AStarNode(job.Target.Location),
                    (n1, n2) => Location.Distance(n1.Location, n2.Location),
                    n => Location.Distance(n.Location, job.Target.Location));

                return path.Select((n, i) => new JobStep(n.Location, JobType.StepMove, null))
                    .Reverse().Skip(1)
                    .Concat(Enumerable.Repeat(new JobStep(job.Target.Location, job.Type, job.Target), 1))
                    .ToArray();
            }

            throw new InvalidOperationException();
        }

        public void Run(TimeSpan t)
        {

        }
    }
}