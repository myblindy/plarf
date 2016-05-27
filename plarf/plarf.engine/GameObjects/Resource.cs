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

        public Resource(dynamic datafile)
        {
            Name = datafile.Name;
            Size = new Size(DataFile.ToInt32(datafile.Width, 1), DataFile.ToInt32(datafile.Height, 1));

            Tuple<ValueRange<int>, string> classvalues = DataFile.ToNamedIntValueRange(datafile.Holds);
            ResourceClass = Game.Instance.ResourceClasses.Single(r => r.Name.Equals(classvalues.Item2, StringComparison.CurrentCultureIgnoreCase));
            ValueRange = classvalues.Item1;
        }

        public Resource() { }

        public Placeable CreatePlaceableInstance(Location location) => new Resource
        {
            Name = Name,
            ResourceClass = ResourceClass,
            Value = Game.Instance.GetNextRandomInteger(ValueRange),
            Location = location,
            Size = Size
        };
    }
}
