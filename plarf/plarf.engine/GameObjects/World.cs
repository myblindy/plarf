using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine.GameObjects
{
    public class World
    {
        private List<Resource> Resources = new List<Resource>();

        public IEnumerable<Placeable> GetPlaceablesAt(int x, int y) => Resources.Where(r => r.ContainsPoint(x, y));
    }
}
