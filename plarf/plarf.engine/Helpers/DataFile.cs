using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine.Helpers
{
    class DataFile : DynamicObject
    {
        Dictionary<string, object> __DynamicPropertiesHolder = new Dictionary<string, object>();

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (__DynamicPropertiesHolder.ContainsKey(binder.Name))
            {
                result = __DynamicPropertiesHolder[binder.Name];
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            __DynamicPropertiesHolder[binder.Name] = value;
            return true;
        }

        public DataFile(Stream inputstream)
        {
            using (var stream = new StreamReader(inputstream))
            {
                string line;
                while ((line = stream.ReadLine()) != null)
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        // find the = sign
                        int eqidx = line.IndexOf('=');

                        var key = line.Substring(0, eqidx).TrimEnd();
                        var val = line.Substring(eqidx + 1).Trim();

                        __DynamicPropertiesHolder.Add(key, val);
                    }
            }
        }
    }
}
