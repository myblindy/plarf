using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine.GameObjects
{
    public class Resource
    {
        public string Name { get; set; }
        public int StackableTo { get; set; }

        public Resource(dynamic datafile)
        {
            Name = datafile.Name;
            StackableTo = datafile.StackableTo == null ? 1 : Convert.ToInt32(datafile.StackableTo);
        }
    }
}
