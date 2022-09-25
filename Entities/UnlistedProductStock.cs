using MidtermProject.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidtermProject.Entities
{
    internal class UnlistedProductStock : ProductStock
    {
        string remarks;

        [ConsoleTableColumn("Remarks", 1)]
        public string Remarks
        {
            get { return remarks; }
            set { remarks = value; }
        }


        DateTime unlistedOn;

        [ConsoleTableColumn("Date Received", 6)]
        public DateTime UnlistedOn { get => unlistedOn; set => unlistedOn = value; }


        public UnlistedProductStock(ProductStock productStock, string remarks) :
            base(productStock.Quantity, productStock.MfgDate, productStock.ExpDate, productStock.Batch, productStock.ProductId)
        {
            this.remarks = remarks;
            this.unlistedOn = DateTime.Now;
        }

        public UnlistedProductStock(int quantity, DateTime mfgDate, DateTime expDate, int batch, int productId, string remarks) 
            : base( quantity, mfgDate, expDate, batch, productId)
        {
            this.remarks = remarks;
            this.UnlistedOn = DateTime.Now;
        }


    }
}
