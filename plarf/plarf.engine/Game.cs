using MoonSharp.Interpreter;
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

        public Game()
        {
            BindLuaObjects();

            LuaScript.DoString("game.debug.writeLine('arf')");
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
