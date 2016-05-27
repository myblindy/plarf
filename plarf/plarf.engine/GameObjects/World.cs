using Plarf.Engine.Helpers;
using Plarf.Engine.Helpers.Exceptions;
using Plarf.Engine.Helpers.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine.GameObjects
{
    public class World
    {
        public Size Size { get; private set; }

        public void Initialize(Size worldsize)
        {
            Size = worldsize;
        }

        private List<Placeable> Placeables = new List<Placeable>();

        public void AddPlaceable(IPlaceableTemplate template, int x, int y) { AddPlaceable(template, new Location(x, y)); }
        public void AddPlaceable(IPlaceableTemplate template, Location location)
        {
            if (GetPlaceablesAt((int)location.X, (int)location.Y).Any())
                throw new LocationAlreadyInUseException(location);

            Placeables.Add(template.CreatePlaceableInstance(location));
        }

        public IEnumerable<Placeable> GetPlaceablesAt(int x, int y) => Placeables.Where(r => r.ContainsPoint(x, y));
    }
}
