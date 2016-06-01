using Plarf.Engine.Helpers;
using Plarf.Engine.Helpers.FileSystem;
using Plarf.Engine.Helpers.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine.GameObjects
{
    public class Resource : Placeable, IPlaceableTemplate
    {
        public string Name { get; private set; }
        public ResourceClass ResourceClass { get; private set; }
        public ValueRange<int> ValueRange { get; private set; }
        public int Value { get; set; }
        public int MaxWorkers => 1;
        public double GatherDuration { get; internal set; }
        public string Texture { get; internal set; }

        public Resource(dynamic datafile)
        {
            Name = datafile.Name;
            GatherDuration = Convert.ToDouble(datafile.GatherDuration);
            Size = new Size(DataFile.ToInt32(datafile.Width, 1), DataFile.ToInt32(datafile.Height, 1));
            Passable = true;
            Texture = datafile.Texture;

            Tuple<ValueRange<int>, string> classvalues = DataFile.ToNamedIntValueRange(datafile.Holds);
            ResourceClass = PlarfGame.Instance.ResourceClasses.Single(r => r.Name.Equals(classvalues.Item2, StringComparison.CurrentCultureIgnoreCase));
            ValueRange = classvalues.Item1;
        }

        public Resource() { }

        public Placeable CreatePlaceableInstance(Location location) => new Resource
        {
            Name = Name,
            ResourceClass = ResourceClass,
            Value = PlarfGame.Instance.GetNextRandomInteger(ValueRange),
            Location = location,
            Size = Size,
            Passable = true,
            Texture = Texture
        };

        public override void Run(TimeSpan t)
        {
        }
    }
}
