﻿using System;
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

        public Human(dynamic datafile)
        {
            MovementSpeed = Convert.ToDouble(datafile.MovementSpeed);
            GatherSpeed = Convert.ToDouble(datafile.GatherSpeed);
        }

        public Human(Human template)
        {
            MovementSpeed = template.MovementSpeed;
            GatherSpeed = template.GatherSpeed;
            Location = template.Location;
        }

        public override Actor CreateActorInstance(Location location) => new Human(this) { Location = location };

        public Job AssignedJob { get; set; }
        public Job AIProcessedJob { get; set; }

        protected JobStep[] JobSteps;
        protected double JobStepBuildup = 0;
        protected double JobStepDuration = 0;
        protected int JobStepIndex = 0;

        public override void Run(TimeSpan t)
        {
            if (AssignedJob == null)
                AssignedJob = Game.Instance.World.ActorCentralIntelligence.GetAvailableJob();

            if (AssignedJob != AIProcessedJob)
            {
                AIProcessedJob = AssignedJob;

                // break job down into steps
                JobSteps = Game.Instance.World.ActorCentralIntelligence.GetJobStepsFromJob(AssignedJob, this);

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
                }
            }
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
            throw new NotImplementedException();
        }

        private double GetJobStepDuration(JobStep jobStep)
        {
            const double DiagonalMovementCost = 1.41421356237;

            switch (jobStep.Type)
            {
                case JobType.Harvest:
                    return ((Resource)jobStep.Placeable).GatherDuration;
                case JobType.StepMove:
                    return jobStep.Location.X != Location.X && jobStep.Location.Y != Location.Y ? DiagonalMovementCost : 1;
            }

            throw new InvalidOperationException();
        }
    }
}