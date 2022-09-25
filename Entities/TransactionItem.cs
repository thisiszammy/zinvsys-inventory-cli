﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidtermProject.Entities
{
    internal class TransactionItem : ProductStock
    {
        decimal price;

        public TransactionItem(int quantity, DateTime mfgDate, DateTime expDate, int batch, int productId, decimal price) 
            : base(quantity, mfgDate, expDate, batch, productId)
        {
            this.price = price;
        }

        public decimal Price { get => price; set => price = value; }
        public decimal SubTotalPrice { get => price * Quantity; }
    }
}
