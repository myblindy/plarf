using Plarf.Engine.Helpers;
using Plarf.Engine.Helpers.FileSystem;
using Plarf.Engine.Helpers.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plarf.Engine.Actors;

namespace Plarf.Engine.GameObjects
{
    public class Resource : Placeable, IPlaceableTemplate
    {
        public string Name { get; private set; }
        public ResourceClass ResourceClass { get; private set; }
        public ValueRange<int> ValueRange { get; private set; }
        public int AmountLeft { get; set; }
        public int MaxWorkers { get; private set; }
        public double GatherDuration { get; private set; }
        public string Texture { get; private set; }

        public Resource(dynamic datafile)
        {
            Name = datafile.Name;
            GatherDuration = Convert.ToDouble(datafile.GatherDuration);
            Size = new Size(DataFile.ToInt32(datafile.Width, 1), DataFile.ToInt32(datafile.Height, 1));
            Passable = true;
            Texture = datafile.Texture;
            MaxWorkers = Convert.ToInt32(datafile.MaxWorkers);

            Tuple<ValueRange<int>, string> classvalues = DataFile.ToNamedIntValueRange(datafile.Holds);
            ResourceClass = PlarfGame.Instance.ResourceClasses.Single(r => r.Name.Equals(classvalues.Item2, StringComparison.CurrentCultureIgnoreCase));
            ValueRange = classvalues.Item1;
        }

        public Resource() { }

        public Placeable CreatePlaceableInstance(Location location) => new Resource
        {
            Name = Name,
            ResourceClass = ResourceClass,
            AmountLeft = PlarfGame.Instance.GetNextRandomInteger(ValueRange),
            Location = location,
            Size = Size,
            Passable = true,
            Texture = Texture,
            MaxWorkers = MaxWorkers,
            GatherDuration = GatherDuration
        };

        public override void Run(TimeSpan t)
        {
        }

        internal void GatherFinished(Human human, int amount)
        {
            AmountLeft -= human.Carry(ResourceClass, Math.Min(amount, AmountLeft));
        }

        public override string ToString() => Name + " (" + AmountLeft + ")";
    }
}
