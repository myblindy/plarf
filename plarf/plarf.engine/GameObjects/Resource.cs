using Plarf.Engine.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine.GameObjects
{
    public class Resource
    {
        public string Name { get; private set; }
        public ResourceClass ResourceClass { get; private set; }
        public ValueRange<int> ValueRange { get; private set; }

        public Resource(dynamic datafile)
        {
            Name = datafile.Name;

            Tuple<ValueRange<int>, string> classvalues = DataFile.ToNamedIntValueRange(datafile.Holds);
            ResourceClass = Game.Instance.ResourceClasses.Single(r => r.Name.Equals(classvalues.Item2, StringComparison.CurrentCultureIgnoreCase));
            ValueRange = classvalues.Item1;
        }
    }
}
