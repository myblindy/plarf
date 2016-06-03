using Plarf.Engine.Helpers.Types;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine.Helpers.FileSystem
{
    class DataFile : DynamicObject
    {
        Dictionary<string, string> __DynamicPropertiesHolder = new Dictionary<string, string>();

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
            __DynamicPropertiesHolder[binder.Name] = value as string;
            return true;
        }

        public static int ToInt32(dynamic property, int def = 0)
        {
            return property == null ? def : Convert.ToInt32(property);
        }

        public static ValueRange<int> ToIntValueRange(dynamic property)
        {
            int dashidx = property.IndexOf('-');

            return new ValueRange<int>(
                Convert.ToInt32(property.Substring(0, dashidx)),
                Convert.ToInt32(property.Substring(dashidx + 1)));
        }

        public static bool ToBoolean(dynamic property, bool def = false)
        {
            return property == null ? def :
                property.Equals("yes", StringComparison.InvariantCultureIgnoreCase) || property.Equals("true", StringComparison.InvariantCultureIgnoreCase) || property == "1" ? true : false;
        }

        public static Tuple<ValueRange<int>, string> ToNamedIntValueRange(dynamic property)
        {
            var p = (string)property;

            var range = new string(p.TakeWhile(c => char.IsWhiteSpace(c) || c == '-' || char.IsDigit(c)).ToArray());
            return Tuple.Create(ToIntValueRange(range), p.Substring(range.Length));
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
