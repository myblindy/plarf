using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Plarf.Engine.Helpers.Types
{
    public class ProductionChain
    {
        public ResourceBundle Inputs { get; private set; } = new ResourceBundle();
        public ResourceBundle Outputs { get; private set; } = new ResourceBundle();
        public TimeSpan TimeRequired { get; set; }

        private void SetBundle(ResourceBundle rb, string buffer)
        {
            foreach (var input in buffer.Split('+'))
            {
                var m = Regex.Match(input, @"^\s*(\d+)\s+([\w\d][\w\d\s]*)$");

                var rcname = m.Groups[2].Value.Trim();
                if (rcname.EqualsI("time"))
                    TimeRequired = TimeSpan.FromSeconds(Convert.ToDouble(m.Groups[1].Value));
                else
                    rb.Add(PlarfGame.Instance.ResourceClasses.First(rc => rc.Name.EqualsI(rcname)), Convert.ToInt32(m.Groups[1].Value));
            }
        }

        public ProductionChain(string buffer)
        {
            var arrowidx = buffer.IndexOf("->");

            SetBundle(Inputs, buffer.Substring(0, arrowidx));
            SetBundle(Outputs, buffer.Substring(arrowidx + 2));
        }

        public override string ToString() =>
            string.Join(" + ", Inputs.Select(kvp => kvp.Value + " " + kvp.Key).Concat(Enumerable.Repeat(TimeRequired.TotalSeconds + " Time", 1)))
            + " -> "
            + string.Join(" + ", Outputs.Select(kvp => kvp.Value + " " + kvp.Key));
    }
}
