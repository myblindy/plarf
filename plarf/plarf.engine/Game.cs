using MoonSharp.Interpreter;
using Plarf.Engine.GameObjects;
using Plarf.Engine.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine
{
    public class Game
    {
        public static Game Instance { get; private set; } = new Game();
        private Game() { }

        /// <summary>
        /// The main Lua script object
        /// </summary>
        public Script LuaScript { get; } = new Script();

        /// <summary>
        /// The array of resource classes loaded at runtime
        /// </summary>
        public List<ResourceClass> ResourceClasses { get; } = new List<ResourceClass>();

        /// <summary>
        /// The array of resource templates to be stamped in the world
        /// </summary>
        public List<Resource> ResourceTemplates { get; } = new List<Resource>();

        public World World { get; } = new World();

        private Random Random;
        public int GetNextRandomInteger() => Random.Next();
        public int GetNextRandomInteger(int from, int to) => Random.Next(from, to);
        public int GetNextRandomInteger(ValueRange<int> range) => Random.Next(range.From, range.To);
        public double GetNextRandomDouble() => Random.NextDouble();

        public void Initialize(int randseed = 0)
        {
            Random = randseed == 0 ? new Random() : new Random(randseed);

            BindLuaObjects();
            LoadGameData();
        }

        private void LoadGameData()
        {
            foreach (var resfile in VFS.GetFiles(@"ResourceClasses", "*.dat"))
                using (var resfilestream = VFS.OpenStream(resfile))
                    ResourceClasses.Add(new ResourceClass(new DataFile(resfilestream)));

            foreach (var resfile in VFS.GetFiles(@"Resources", "*.dat"))
                using (var resfilestream = VFS.OpenStream(resfile))
                    ResourceTemplates.Add(new Resource(new DataFile(resfilestream)));
        }

        #region Lua Bindings
        private void BindLuaObjects()
        {
            // the types
            UserData.RegisterType<LuaInterface.LIDebug>();
            UserData.RegisterType<LuaInterface.LIGame>();
            UserData.RegisterType<ResourceClass>();
            UserData.RegisterType<Resource>();
            UserData.RegisterType<ValueRange<int>>();
            UserData.RegisterType<World>();
            UserData.RegisterType<Location>();
            UserData.RegisterType<Size>();

            // and the global objects
            LuaScript.Globals.Set("game", UserData.Create(new LuaInterface.LIGame(this)));
        }
        #endregion
    }
}
