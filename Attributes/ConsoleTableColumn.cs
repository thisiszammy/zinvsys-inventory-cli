using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidtermProject.Attributes
{
    internal class ConsoleTableColumn : Attribute
    {
        public string displayName { get; set; }
        public int order { get; set; }

        public ConsoleTableColumn(string displayName, int order)
        {
            this.displayName = displayName;
            this.order = order;
        }
    }
}
