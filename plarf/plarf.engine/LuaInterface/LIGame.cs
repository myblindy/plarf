using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine.LuaInterface
{
    /// <summary>
    /// Lua Interface class for the game container class
    /// </summary>
    class LIGame
    {
        [MoonSharpHidden]
        public Game Game { get; private set; }

        /// <summary>
        /// Provides the debugging context
        /// </summary>
        public LIDebug Debug { get; } = new LIDebug();

        /// <summary>
        /// The resource classes loaded at runtime
        /// </summary>
        public IEnumerable<string> ResourceClasses { get { return Game.ResourceClasses.Select(r => r.Name); } }

        public LIGame(Game game)
        {
            Game = game;
        }
    }
}
