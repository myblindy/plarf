using Plarf.Engine.Helpers;
using Plarf.Engine.Helpers.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine.GameObjects
{
    public interface IPlaceableTemplate
    {
        Placeable CreatePlaceableInstance(Location location, Size size);
        void OnAdded();
    }
}
