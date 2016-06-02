using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plarf.Engine.Helpers.Types;
using Plarf.Engine.AI;
using Plarf.Engine.GameObjects;
using Plarf.Engine.Helpers.FileSystem;

namespace Plarf.Engine.Actors
{
    public class Human : Actor
    {
        public double MovementSpeed { get; private set; }
        public double GatherSpeed { get; private set; }
        public double MaxCarryWeight { get; private set; }

        public Human(dynamic datafile)
        {
            MovementSpeed = Convert.ToDouble(datafile.MovementSpeed);
            GatherSpeed = Convert.ToDouble(datafile.GatherSpeed);
            Texture = datafile.Texture;
            MaxCarryWeight = Convert.ToDouble(datafile.MaxCarryWeight);
        }

        public Human(Human template)
        {
            MovementSpeed = template.MovementSpeed;
            GatherSpeed = template.GatherSpeed;
            Location = template.Location;
            Texture = template.Texture;
            MaxCarryWeight = template.MaxCarryWeight;
        }

        public override Actor CreateActorInstance(Location location) => new Human(this) { Location = location };

        public Job AssignedJob { get; set; }
        public Job AIProcessedJob { get; set; }

        protected JobStep[] JobSteps;
        protected double JobStepBuildup = 0;
        protected double JobStepDuration = 0;
        protected int JobStepIndex = 0;
        public JobStep CurrentJobStep => JobStepIndex < JobSteps.Length ? JobSteps[JobStepIndex] : new JobStep(JobType.Invalid);

        public ResourceBundle ResourcesCarried { get; private set; } = new ResourceBundle();

        private int ResourceHarvestableAmount(Resource res) =>
            (int)(Math.Min(res.AmountLeft, Math.Floor(MaxCarryWeight - ResourcesCarried.Weight)) / res.ResourceClass.Weight);

        public override void Run(TimeSpan t)
        {
            if (AssignedJob == null)
                AssignedJob = PlarfGame.Instance.World.ActorCentralIntelligence.GetAvailableJob(this);

            if (AssignedJob != AIProcessedJob)
            {
                AIProcessedJob = AssignedJob;

                // break job down into steps
                JobSteps = PlarfGame.Instance.World.ActorCentralIntelligence.GetJobStepsFromJob(AssignedJob, this);

                if (!JobSteps.Any())
                    return;

                JobStepIndex = 0;
                JobStepBuildup = 0;
                JobStepDuration = GetJobStepDuration(JobSteps[JobStepIndex]);
            }

            if (JobSteps != null && JobSteps.Any() && JobStepIndex < JobSteps.Length)
            {
                var step = JobSteps[JobStepIndex];

                switch (step.Type)
                {
                    case JobType.StepMove:
                        if ((JobStepBuildup += MovementSpeed * t.TotalSeconds) >= JobStepDuration)
                        {
                            JobStepBuildup -= JobStepDuration;
                            Location = step.Location;
                            StepDone();
                        }
                        break;
                    case JobType.Harvest:
                        if ((JobStepBuildup += GatherSpeed * t.TotalSeconds) >= JobStepDuration)
                        {
                            JobStepBuildup -= JobStepDuration;
                            var res = (Resource)JobSteps[JobStepIndex].Placeable;
                            res.GatherFinished(this, ResourceHarvestableAmount(res));
                            StepDone();
                        }
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public int Carry(ResourceClass resclass, int amount)
        {
            ResourcesCarried.Add(resclass, amount);
            return amount;
        }

        private void StepDone()
        {
            if (++JobStepIndex < JobSteps.Length)
                JobStepDuration = GetJobStepDuration(JobSteps[JobStepIndex]);
            else
                JobDone();
        }

        private void JobDone()
        {
            return;
        }

        private double GetJobStepDuration(JobStep jobStep)
        {
            const double DiagonalMovementCost = 1.41421356237;

            switch (jobStep.Type)
            {
                case JobType.Harvest:
                    {
                        var res = (Resource)jobStep.Placeable;
                        return res.GatherDuration * ResourceHarvestableAmount(res);
                    }
                case JobType.StepMove:
                    return jobStep.Location.X != Location.X && jobStep.Location.Y != Location.Y ? DiagonalMovementCost : 1;
            }

            throw new InvalidOperationException();
        }
    }
}
