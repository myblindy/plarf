using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plarf.Engine.Helpers.Types;

namespace Plarf.Engine.GameObjects
{
    public class BuildingFlex : Building
    {
        public ValueRange<int> FlexWidth { get; set; }
        public ValueRange<int> FlexHeight { get; set; }

        public override Placeable CreatePlaceableInstance(Location location, Size size) => new BuildingFlex
        {
            Name = Name,
            Texture = Texture,
            Function = Function,
            StorageAccepted = StorageAccepted,
            FlexHeight = FlexHeight,
            FlexWidth = FlexWidth,
            Passable = Passable,
            Location = location,
            Size = size
        };

        public override void Run(TimeSpan t)
        {
        }
    }
}
