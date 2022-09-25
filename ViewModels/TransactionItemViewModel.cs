using MidtermProject.Attributes;
using MidtermProject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidtermProject.ViewModels
{
    internal class TransactionItemViewModel
    {
        [ConsoleTableColumn("BN", 0)]
        public int Batch { get; set; }

        [ConsoleTableColumn("Product", 1)]
        public string ProductName { get; set; }

        [ConsoleTableColumn("Qty", 2)]
        public int Quantity { get; set; }

        [ConsoleTableColumn("Price", 3)]
        public decimal Price { get; set; }

        [ConsoleTableColumn("Sub-Total", 4)]
        public decimal SubTotal { get; set; }

        [ConsoleTableColumn("Manufacturing Date", 5)]
        public DateTime MfgDate { get; set; }

        [ConsoleTableColumn("Expiry Date", 6)]
        public DateTime ExpiryDate { get; set; }
        public int ProductId { get; set; }
    }
}
