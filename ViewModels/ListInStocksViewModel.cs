using MidtermProject.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidtermProject.ViewModels
{
    internal class ListInStocksViewModel
    {
        [ConsoleTableColumn("BN" , 0)]
        public int Batch { get; set; }

        [ConsoleTableColumn("Product", 1)]
        public string ProductName { get; set; }

        [ConsoleTableColumn("Qty", 2)]
        public int Quantity { get; set; }

        [ConsoleTableColumn("Manufacturing Date", 3)]
        public DateTime MfgDate { get; set; }

        [ConsoleTableColumn("Expiry Date", 4)]
        public DateTime ExpiryDate { get; set; }

        [ConsoleTableColumn("Received On", 5)]
        public DateTime ReceivedOn { get; set; }

    }
}
