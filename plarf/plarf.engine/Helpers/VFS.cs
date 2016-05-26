using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine.Helpers
{
    class VFS
    {
        public static FileStream OpenStream(string path)
        {
            return File.OpenRead(Path.Combine(@"Data", path));
        }

        public static IEnumerable<string> GetFiles(string path, string filter = null)
        {
            return Directory.GetFiles(Path.Combine(@"Data", path), filter)
                .Select(p => p.Substring(5));
        }
    }
}
