using Plarf.Engine.Helpers.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.MonoGame.Helpers
{
    static class ObjectHelpers
    {
        private static ISet<Type> SelfPresentingTypes = new HashSet<Type> { typeof(ValueRange<int>), typeof(ValueRange<int>?), typeof(Size), typeof(Location),
            typeof(ProductionChain), typeof(NameWithPlural), typeof(WorkerType), typeof(Engine.AI.JobStep) };

        private static bool IsEnumerableEmpty(IEnumerable enumerable)
        {
            foreach (var item in enumerable)
                return false;
            return true;
        }

        public static IEnumerable<Tuple<string, object, int>> GetObjectProperties(object obj, int ident = 0)
        {
            foreach (var prop in obj.GetType().GetProperties())
            {
                // blacklist
                if (prop.Name == "Item") continue;

                // figure out if it's a basic type
                var rettype = prop.PropertyType;
                var basictype = rettype == typeof(string) || rettype == typeof(int) || rettype == typeof(double) || rettype == typeof(bool) || rettype.IsEnum;
                var val = prop.GetValue(obj);

                if (val == null)
                    yield return Tuple.Create(prop.Name, (object)"null", ident);                                      // if null then show null
                else if (basictype)
                    yield return Tuple.Create(prop.Name, val, ident);                                                 // output basic types directly
                else if (rettype == typeof(ResourceBundle))
                {                                                                                                     // special case for ResourceBundle, show contents as an array
                    yield return Tuple.Create(prop.Name, (object)("[" + rettype.Name + "]"), ident);
                    if (((ResourceBundle)val).Any())
                        foreach (var kvp in (ResourceBundle)val)
                            yield return Tuple.Create(prop.Name, (object)(kvp.Value + " " + kvp.Key), ident + 1);
                    else
                        yield return Tuple.Create(prop.Name, (object)"Empty", ident + 1);
                }
                else if (SelfPresentingTypes.Contains(rettype))
                    yield return Tuple.Create(prop.Name, (object)val.ToString(), ident);                              // types that have a readable ToString()
                else if ((rettype.IsGenericType && rettype.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    || rettype.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                {                                                                                                     // IEnumerable<T>
                    if (IsEnumerableEmpty((IEnumerable)val))
                        yield return Tuple.Create(prop.Name, (object)"Empty", ident);
                    else
                    {
                        var ienum = rettype.GetGenericTypeDefinition() == typeof(IEnumerable<>) ? rettype : rettype.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                        yield return Tuple.Create(prop.Name, (object)("[IEnumerable<" + ienum.GetGenericArguments()[0].Name + ">]"), ident);

                        // and iterate over it
                        int idx = 0;
                        foreach (var item in (IEnumerable)val)
                            yield return Tuple.Create(idx++.ToString(), item, ident + 1);
                    }
                }
                else
                {
                    yield return Tuple.Create(prop.Name, (object)("[" + rettype.Name + "]"), ident);
                    foreach (var subprop in GetObjectProperties(val, ident + 1))                                      // recurse non-basic types
                        yield return subprop;
                }
            }
        }
    }
}
