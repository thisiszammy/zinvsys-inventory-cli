using MidtermProject.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidtermProject.ViewModels
{
    internal class ListOutStocksViewModel
    {
        [ConsoleTableColumn("BN", 0)]
        public int Batch { get; set; }

        [ConsoleTableColumn("Product", 1)]
        public string ProductName { get; set; }

        [ConsoleTableColumn("Qty", 2)]
        public int Quantity { get; set; }

        [ConsoleTableColumn("Received On", 3)]
        public DateTime ReceivedOn { get; set; }

        [ConsoleTableColumn("Removed On", 4)]
        public DateTime RemovedOn { get; set; }

        [ConsoleTableColumn("Remarks", 5)]
        public string Remarks { get; set; }
    }
}
