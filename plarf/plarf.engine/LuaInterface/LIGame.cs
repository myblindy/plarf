using MoonSharp.Interpreter;
using Plarf.Engine.Actors;
using Plarf.Engine.GameObjects;
using Plarf.Engine.Helpers.Types;
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
        public IEnumerable<ResourceClass> ResourceClasses => Game.ResourceClasses;

        public IDictionary<string, Resource> ResourceTemplates => Game.ResourceTemplates;

        public World World => Game.World;

        public Human HumanTemplate => Game.HumanTemplate;

        public LIGame(Game game)
        {
            Game = game;
        }
    }
}
