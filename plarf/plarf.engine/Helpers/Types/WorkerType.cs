using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine.Helpers.Types
{
    public class WorkerType
    {
        public NameWithPlural WorkerName { get; set; }

        public WorkerType(NameWithPlural name) { WorkerName = name; }
        public WorkerType(string buffer) : this(new NameWithPlural(buffer)) { }

        public override string ToString() => WorkerName.Singular;
    }
}
