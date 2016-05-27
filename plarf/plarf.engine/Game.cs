﻿using MoonSharp.Interpreter;
using Plarf.Engine.GameObjects;
using Plarf.Engine.Helpers;
using Plarf.Engine.Helpers.FileSystem;
using Plarf.Engine.Helpers.Types;
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
        public Script LuaScript { get; private set; }

        /// <summary>
        /// The array of resource classes loaded at runtime
        /// </summary>
        public List<ResourceClass> ResourceClasses { get; private set; }

        /// <summary>
        /// The array of resource templates to be stamped in the world
        /// </summary>
        public IDictionary<string, Resource> ResourceTemplates { get; private set; }

        public World World { get; private set; }

        private Random Random;
        public int GetNextRandomInteger() => Random.Next();
        public int GetNextRandomInteger(int from, int to) => Random.Next(from, to);
        public int GetNextRandomInteger(ValueRange<int> range) => Random.Next(range.From, range.To);
        public double GetNextRandomDouble() => Random.NextDouble();

        public void Initialize(Size worldsize, int randseed = 0)
        {
            Random = randseed == 0 ? new Random() : new Random(randseed);

            BindLuaObjects();
            LoadGameData();

            World = new World();
            World.Initialize(worldsize);
        }

        private void LoadGameData()
        {
            ResourceClasses = new List<ResourceClass>();
            foreach (var resfile in VFS.GetFiles(@"ResourceClasses", "*.dat"))
                using (var resfilestream = VFS.OpenStream(resfile))
                    ResourceClasses.Add(new ResourceClass(new DataFile(resfilestream)));

            ResourceTemplates = new Dictionary<string, Resource>();
            foreach (var resfile in VFS.GetFiles(@"Resources", "*.dat"))
                using (var resfilestream = VFS.OpenStream(resfile))
                {
                    var res = new Resource(new DataFile(resfilestream));
                    ResourceTemplates.Add(res.Name, res);
                }
        }

        #region Lua Bindings
        private void BindLuaObjects()
        {
            LuaScript = new Script();

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
