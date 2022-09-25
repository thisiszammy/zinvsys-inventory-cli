using MidtermProject.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidtermProject.ViewModels
{
    internal class TransactionsViewModel
    {
        [ConsoleTableColumn("Id", 0)]
        public int Id { get; set; }
        [ConsoleTableColumn("Transaction #", 1)]
        public string TransactionId { get; set; }

        [ConsoleTableColumn("Customer", 2)]
        public string Customer { get; set; }

        [ConsoleTableColumn("Total Amount Due", 3)]
        public decimal TotalAmountDue { get; set; }


        [ConsoleTableColumn("Status", 4)]
        public string Status { get; set; }


        [ConsoleTableColumn("Created On", 5)]
        public DateTime CreatedOn { get; set; }

        [ConsoleTableColumn("Resolved On", 6)]
        public DateTime? LastModifiedOn { get; set; }
    }
}
