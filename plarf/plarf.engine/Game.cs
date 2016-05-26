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
        /// <summary>
        /// The main Lua script object
        /// </summary>
        public Script LuaScript { get; } = new Script();

        /// <summary>
        /// The array of resource classes loaded at runtime
        /// </summary>
        public List<Resource> ResourceClasses { get; } = new List<Resource>();

        public Game()
        {
            BindLuaObjects();
            LoadGameData();

            LuaScript.DoString(@"
                for name in game.resourceClasses do
                    game.debug.writeLine(name)
                end");
        }

        private void LoadGameData()
        {
            foreach (var resfile in VFS.GetFiles(@"Resources", "*.dat"))
                using (var resfilestream = VFS.OpenStream(resfile))
                    ResourceClasses.Add(new Resource(new DataFile(resfilestream)));
        }

        private void BindLuaObjects()
        {
            // the types
            UserData.RegisterType<LuaInterface.LIDebug>();
            UserData.RegisterType<LuaInterface.LIGame>();

            // and the global objects
            LuaScript.Globals.Set("game", UserData.Create(new LuaInterface.LIGame(this)));
        }
    }
}
