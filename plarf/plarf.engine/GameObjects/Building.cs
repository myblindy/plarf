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
        Storage
    }

    public abstract class Building : Placeable, IPlaceableTemplate
    {
        public string Texture { get; set; }
        public BuildingFunction Function { get; set; }
        public ResourceClass[] StorageAccepted { get; set; }
        public ResourceBundle Resources { get; set; } = new ResourceBundle();

        public abstract Placeable CreatePlaceableInstance(Location location, Size size);

        private static BuildingFunction BuildingFunctionFromString(string s)
        {
            if (s.Equals("storage", StringComparison.InvariantCultureIgnoreCase)) return BuildingFunction.Storage;
            throw new InvalidOperationException();
        }

        public static Building FromDataFile(dynamic datafile)
        {
            if (datafile.BuildingType.Equals("flex", StringComparison.InvariantCultureIgnoreCase))
                return new BuildingFlex
                {
                    Name = datafile.Name,
                    Texture = datafile.Texture,
                    Passable = DataFile.ToBoolean(datafile.Passable, false),
                    FlexHeight = DataFile.ToIntValueRange(datafile.FlexHeight),
                    FlexWidth = DataFile.ToIntValueRange(datafile.FlexWidth),
                    Function = BuildingFunctionFromString(datafile.Function),
                    StorageAccepted = datafile.StorageAccepted == "all" ? (ResourceClass[])null : null
                };

            throw new InvalidOperationException();
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
    }
}
