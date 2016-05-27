using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plarf.Engine.Helpers.Types
{
    public struct ValueRange<T>
    {
        public T From { get; private set; }
        public T To { get; private set; }

        public ValueRange(T from, T to) { From = from; To = to; }

        public override string ToString() => From + "-" + To;
    }
}
