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

        public List<Resource> ResourceTemplates { get; } = new List<Resource>();

        public void Initialize()
        {
            BindLuaObjects();
            LoadGameData();

            LuaScript.DoString(@"
                for i, r in ipairs(game.resourceClasses) do
                    game.debug.writeLine(r.name .. ' stackable to ' .. r.stackableTo)
                end");

            LuaScript.DoString(@"
                for i, r in ipairs(game.resourceTemplates) do
                    game.debug.writeLine(r.name .. ' contains ' .. r.valueRange.from .. '-' .. r.valueRange.to .. ' ' .. r.resourceClass.name)
                end");
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

        private void BindLuaObjects()
        {
            // the types
            UserData.RegisterType<LuaInterface.LIDebug>();
            UserData.RegisterType<LuaInterface.LIGame>();
            UserData.RegisterType<ResourceClass>();
            UserData.RegisterType<Resource>();
            UserData.RegisterType<ValueRange<int>>();

            // and the global objects
            LuaScript.Globals.Set("game", UserData.Create(new LuaInterface.LIGame(this)));
        }
    }
}
