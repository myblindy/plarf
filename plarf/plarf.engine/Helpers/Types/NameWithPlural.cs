using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine.Helpers.Types
{
    public class NameWithPlural
    {
        public string Singular { get; private set; }
        public string Plural { get; private set; }

        public NameWithPlural(string sg, string pl) { Singular = sg; Plural = pl; }
        public NameWithPlural(string buffer)
        {
            var arr = buffer.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            Singular = arr[0].Trim();
            Plural = arr[1].Trim();
        }

        public override string ToString() => Singular + " | " + Plural;
    }
}
