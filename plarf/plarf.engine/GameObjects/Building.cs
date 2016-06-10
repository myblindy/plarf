using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plarf.Engine.Helpers.Types;
using System.Collections;
using Plarf.Engine.Helpers.FileSystem;

namespace Plarf.Engine.GameObjects
{
    public enum BuildingFunction
    {
        Storage,
        Production
    }

    public enum BuildingType
    {
        Flex,
        Static
    }

    public class Building : Placeable, IPlaceableTemplate
    {
        public string Texture { get; set; }
        public BuildingFunction Function { get; set; }
        public ResourceClass[] StorageAccepted { get; set; }
        public ResourceBundle Resources { get; set; } = new ResourceBundle();
        public ValueRange<int>? FlexWidth { get; set; }
        public ValueRange<int>? FlexHeight { get; set; }
        public BuildingType Type { get; set; }
        public int MaxWorkers { get; set; }
        public NameWithPlural WorkerName { get; set; }
        public ProductionChain ProductionChain { get; set; }

        public virtual Placeable CreatePlaceableInstance(Location location, Size size) => new Building
        {
            Name = Name,
            Texture = Texture,
            Function = Function,
            StorageAccepted = StorageAccepted,
            FlexHeight = FlexHeight,
            FlexWidth = FlexWidth,
            Passable = Passable,
            Location = location,
            Size = Type == BuildingType.Flex ? size : Size
        };

        private static BuildingFunction BuildingFunctionFromString(string s)
        {
            if (s.EqualsI("storage")) return BuildingFunction.Storage;
            if (s.EqualsI("production")) return BuildingFunction.Production;
            throw new InvalidOperationException();
        }

        public static Building FromDataFile(dynamic datafile)
        {
            // common fields
            var b = new Building
            {
                Name = datafile.Name,
                Texture = datafile.Texture,
                Passable = DataFile.ToBoolean(datafile.Passable, false),
                Function = BuildingFunctionFromString(datafile.Function),
            };

            // building type fields
            if (Extensions.EqualsI(datafile.BuildingType, "flex"))
            {
                b.FlexHeight = DataFile.ToIntValueRange(datafile.FlexHeight);
                b.FlexWidth = DataFile.ToIntValueRange(datafile.FlexWidth);
                b.Type = BuildingType.Flex;
            }
            else if (Extensions.EqualsI(datafile.BuildingType, "static"))
            {
                b.Size = new Size(Convert.ToInt32(datafile.Width), Convert.ToInt32(datafile.Height));
                b.Type = BuildingType.Static;
            }

            // function fields
            switch (b.Function)
            {
                case BuildingFunction.Production:
                    b.MaxWorkers = Convert.ToInt32(datafile.MaxWorkers);
                    b.WorkerName = new NameWithPlural(datafile.WorkerName);
                    b.ProductionChain = new ProductionChain(datafile.ProductionChain);
                    break;
                case BuildingFunction.Storage:
                    b.StorageAccepted = datafile.StorageAccepted == "all" ? (ResourceClass[])null : null;
                    break;
                default:
                    throw new InvalidOperationException();
            };

            return b;
        }

        public override void OnAdded()
        {
            switch (Function)
            {
                case BuildingFunction.Storage:
                    PlarfGame.Instance.World.ActorCentralIntelligence.AddStorageJob(this);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        internal void Store(ResourceBundle res)
        {
            if (StorageAccepted == null)
            {
                // all
                Resources.Add(res);
                res.Clear();
            }
            else
            {
                foreach (var resaccepted in StorageAccepted)
                    foreach (var kvp in res)
                        if (kvp.Key == resaccepted)
                            Resources.Add(resaccepted, kvp.Value);
                res.RemoveAll(kvp => StorageAccepted.Contains(kvp.Key));
            }
        }

        public override void Run(TimeSpan t) { }
    }
}
