using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine.Helpers.Types
{
    public interface IRunnable
    {
        void Run(TimeSpan t);
    }
}
