using Plarf.Engine.Helpers;
using Plarf.Engine.Helpers.FileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine.GameObjects
{
    public class ResourceClass
    {
        public string Name { get; set; }
        public int StackableTo { get; set; }
        public double UnitWeight { get; set; }

        public ResourceClass(dynamic datafile)
        {
            Name = datafile.Name;
            StackableTo = DataFile.ToInt32(datafile.StackableTo, 1);
            UnitWeight = Convert.ToDouble(datafile.UnitWeight);
        }

        public override string ToString() => Name;
    }
}
