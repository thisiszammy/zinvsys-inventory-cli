using MidtermProject.Attributes;
using MidtermProject.DAL;
using MidtermProject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace MidtermProject.Entities
{
    internal class Transaction : BaseEntity
    {
        int? customerId;
        string transactionId;
        decimal additionalFees;
        string remarks;
        TransactionStatusEnum transactionStatus;
        List<TransactionItem> transactionItems;
        List<RefundedTransactionItem> refundedItems;

        public Transaction()
        {
            this.TransactionItems = new List<TransactionItem>();
            this.RefundedItems = new List<RefundedTransactionItem>();
            TransactionStatus = TransactionStatusEnum.PAID;
        }

        public Transaction(int? customerId, 
            string transactionId, 
            decimal additionalFees,
            TransactionStatusEnum transactionStatus,
            string remarks)
        {
            this.CustomerId = customerId;
            this.TransactionId = transactionId;
            this.AdditionalFees = additionalFees;
            this.TransactionStatus = transactionStatus;
            this.Remarks = remarks;

            this.TransactionItems = new List<TransactionItem>();
            this.RefundedItems = new List<RefundedTransactionItem>();

        }

        public int? CustomerId { get => customerId; set => customerId = value; }
        public string TransactionId { get => transactionId; set => transactionId = value; }
        [UpdateEntity]
        public decimal AdditionalFees { get => additionalFees; set => additionalFees = value; }
        public TransactionStatusEnum TransactionStatus { get => transactionStatus; set => transactionStatus = value; }
        [UpdateEntity]
        public List<TransactionItem> TransactionItems { get => transactionItems; set => transactionItems = value; }
        [UpdateEntity]
        public List<RefundedTransactionItem> RefundedItems { get => refundedItems; set => refundedItems = value; }
        public decimal TotalAmountDue
        {
            get
            {
                return transactionItems.Sum(x => x.SubTotalPrice) + additionalFees;
            }
        }
        public void AddTransactionItem(int productId, int batch, int quantity, decimal price, DateTime mfgDate, DateTime expDate)
        {
            var item = transactionItems.Where(x => x.ProductId == productId && x.Batch == batch).FirstOrDefault();
            if(item == null) transactionItems.Add(new TransactionItem(quantity, mfgDate, expDate, batch, productId, price));
            else item.Quantity += quantity;
        }

        public void RemoveTransactionItem(int productId, int batch, int quantity)
        {
            var item = transactionItems.Where(x => x.ProductId == productId && x.Batch == batch).FirstOrDefault();
            item.Quantity -= quantity;
        }

        public void RefundTransactionItem(int productId, int batch, int quantity, string remarks)
        {
            var item = transactionItems.Where(x => x.ProductId == productId && x.Batch == batch).FirstOrDefault();
            item.Quantity -= quantity;

            var rItem = refundedItems.Where(x => x.ProductId == productId && x.Batch == batch).FirstOrDefault();
            if(rItem != null)
            {
                rItem.Quantity += quantity;
                rItem.Remarks = remarks;
                rItem.RefundedOn = DateTime.Now;
            }
            else
            {
                refundedItems.Add(new RefundedTransactionItem(quantity, item.MfgDate, item.ExpDate, item.Batch, Uid, item.Price, remarks));
            }
        }

        [UpdateEntity]
        public string Remarks { get => remarks; set => remarks = value; }

        public override TEntity Clone<TEntity>()
        {
            var transaction = new Transaction()
            {
                Uid = this.Uid,
                AdditionalFees = this.additionalFees,
                CreatedBy = this.CreatedBy,
                CreatedOn = this.CreatedOn,
                CustomerId = this.CustomerId,
                DeletedBy = this.DeletedBy,
                DeletedOn = this.DeletedOn,
                IsDeleted = this.IsDeleted,
                LastModifiedBy = this.LastModifiedBy,
                LastModifiedOn = this.LastModifiedOn,
                RefundedItems = new List<RefundedTransactionItem>(this.RefundedItems),
                TransactionId = this.TransactionId,
                TransactionItems = new List<TransactionItem>(this.TransactionItems),
                TransactionStatus = this.TransactionStatus,
                Remarks = this.Remarks
            };

            return (TEntity)(object)transaction;
        }
    }
}
