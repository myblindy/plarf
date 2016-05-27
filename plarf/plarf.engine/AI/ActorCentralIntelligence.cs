using Plarf.Engine.Actors;
using Plarf.Engine.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine.AI
{
    public enum JobType
    {
        Move,
        Harvest
    }

    public class Job
    {
        public JobType Type { get; set; }
        public Placeable Target { get; set; }
    }

    public class ActorCentralIntelligence
    {
        private List<Job> Jobs = new List<Job>();

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
        }
    }
}
