using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Tests.Helpers
{
    static class ExtraAssert
    {
        public static void Throws<T>(Action task) where T : Exception
        {
            try
            {
                task();
            }
            catch (Exception ex)
            {
                if (typeof(T).IsAssignableFrom(ex.GetType()))
                    return;
            }

            Assert.Fail();
        }
    }
}
