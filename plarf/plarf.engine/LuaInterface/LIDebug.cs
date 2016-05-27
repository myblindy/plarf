using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine.LuaInterface
{
    /// <summary>
    /// Lua Interface class for debugging purposes
    /// </summary>
    class LIDebug
    {
        /// <summary>
        /// Writes a message to the debug console
        /// </summary>
        public void WriteLine(object msg) { Debug.WriteLine(msg); }

        public void TestAssertIsTrue(bool b) { Assert.IsTrue(b); }
    }
}
