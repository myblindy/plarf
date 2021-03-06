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
        public double MaxCarryWeight { get; private set; }
        public WorkerType WorkerType { get; set; }

        public Job AssignedJob { get; set; }
        private Job AIProcessedJob { get; set; }
        private Job LastCompletedJob { get; set; }
        public Building ChosenWorkplace { get; set; }
        public bool InsideWorkplace { get; set; }

        protected JobStep[] JobSteps;
        public double JobStepBuildup { get; set; } = 0;
        public double JobStepDuration { get; set; } = 0;
        protected int JobStepIndex = 0;
        protected bool JobStepFirstCycle = true;
        public JobStep CurrentJobStep => JobSteps != null && JobStepIndex < JobSteps.Length ? JobSteps[JobStepIndex] : new JobStep(JobType.Invalid);

        public ResourceBundle ResourcesCarried { get; private set; } = new ResourceBundle();

        public Human(dynamic datafile)
        {
            MovementSpeed = Convert.ToDouble(datafile.MovementSpeed);
            GatherSpeed = Convert.ToDouble(datafile.GatherSpeed);
            Texture = datafile.Texture;
            MaxCarryWeight = Convert.ToDouble(datafile.MaxCarryWeight);
            Size = new Size(1, 1);
        }

        public Human(Human template)
        {
            MovementSpeed = template.MovementSpeed;
            GatherSpeed = template.GatherSpeed;
            Location = template.Location;
            Texture = template.Texture;
            MaxCarryWeight = template.MaxCarryWeight;
            Size = template.Size;
        }

        public override Actor CreateActorInstance(string name, Location location) => new Human(this) { Name = name, Location = location };

        private int ResourceHarvestableAmount(Resource res) =>
            (int)(Math.Min(res.AmountLeft, Math.Floor(MaxCarryWeight - ResourcesCarried.Weight)) / res.ResourceClass.UnitWeight);
        public bool FullForResourceClass(ResourceClass rc) =>
            Math.Floor(MaxCarryWeight - ResourcesCarried.Weight) / rc.UnitWeight <= 0;

        public override void Run(TimeSpan t)
        {
            if (ChosenWorkplace == null && WorkerType != null)
            {
                // try to find a matching workplace
                ChosenWorkplace = PlarfGame.Instance.World.Placeables.OfType<Building>()
                    .FirstOrDefault(b => b.Function == BuildingFunction.Production && b.WorkerType == WorkerType && b.Workers.Count() < b.MaxWorkers);
            }

            if (AssignedJob == null)
                AssignedJob = PlarfGame.Instance.World.ActorCentralIntelligence.GetAvailableJob(this, LastCompletedJob);

            if (AssignedJob != AIProcessedJob)
            {
                AIProcessedJob = AssignedJob;

                // break job down into steps
                JobSteps = PlarfGame.Instance.World.ActorCentralIntelligence.GetJobStepsFromJob(AssignedJob, this);

                if (JobSteps == null || !JobSteps.Any())
                    return;

                JobStepIndex = 0;
                JobStepBuildup = 0;
                JobStepDuration = GetJobStepDuration(JobSteps[JobStepIndex]);
            }

            if (JobSteps != null && JobSteps.Length > 0 && JobStepIndex < JobSteps.Length)
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
                    case JobType.Production:
                        if (JobStepFirstCycle)
                            ((Building)JobSteps[JobStepIndex].Placeable).ConsumeProductionInputs();
                        if ((JobStepBuildup += MovementSpeed * t.TotalSeconds) >= JobStepDuration)
                        {
                            JobStepBuildup -= JobStepDuration;
                            ((Building)JobSteps[JobStepIndex].Placeable).ProductionDone();
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
                    case JobType.DropResources:
                        {
                            // no buildup
                            var b = (Building)JobSteps[JobStepIndex].Placeable;
                            b.Store(ResourcesCarried);
                            StepDone();
                        }
                        break;
                    case JobType.EnterWorkplace:
                        {
                            // no buildup
                            InsideWorkplace = true;
                            StepDone();
                        }
                        break;
                    case JobType.StepRetrieveResources:
                        {
                            // no buildup
                            foreach (var rc in JobSteps[JobStepIndex].Resources.Keys.ToArray())
                            {
                                var available = JobSteps[JobStepIndex].Resources[rc];
                                var amt = Math.Min(available, (int)((MaxCarryWeight - ResourcesCarried.Weight) / rc.UnitWeight));
                                JobSteps[JobStepIndex].Resources[rc] = available - amt;
                                Carry(rc, amt);
                            }
                            StepDone();
                        }
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                JobStepFirstCycle = false;
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

            JobStepFirstCycle = true;
        }

        private void JobDone()
        {
            LastCompletedJob = AssignedJob;
            AssignedJob = null;
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
                    return jobStep.Location == Location ? 0 : jobStep.Location.X != Location.X && jobStep.Location.Y != Location.Y ? DiagonalMovementCost : 1;
                case JobType.DropResources:
                case JobType.EnterWorkplace:
                case JobType.StepRetrieveResources:
                    return 0;
                case JobType.Production:
                    return ChosenWorkplace.ProductionChain.TimeRequired.TotalSeconds;
            }

            throw new InvalidOperationException();
        }
    }
}
