using MidtermProject.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidtermProject.DAL
{
    internal static class Database
    {
        public static List<object> Tables { get; private set; }

        public static void Init(Type[] types)
        {
            Tables = new List<object>();
            foreach (var type in types)
            {
                Tables.Add(GenerateDynamicList(type));
            }
            MockData();
        }

        public static List<T> GetTable<T>()
        {
            var temp = Tables.Where(x => typeof(List<T>).IsAssignableFrom(x.GetType())).First();
            return (List<T>)temp;
        }

        private static object GenerateDynamicList(Type type)
        {
            Type temp = typeof(List<>).MakeGenericType(new Type[] { type });
            return Activator.CreateInstance(temp);
        }

        private static void MockData()
        {
            AdministrativeUser _defaultUser = new AdministrativeUser("Michael Jay", "Zamoras", "mjzamoras", "12345678");
            _defaultUser.CreatedBy = -1;
            DBContext<ApplicationUser>.Add(_defaultUser);

            DBContext<ApplicationUser>.Add(new List<ApplicationUser>
            { 
                new Customer("Brian Jan", "Zamoras",  "Tacloban City, Leyte"),
                new Customer("Shirlyn", "Carcedo",  "Abuyog, Leyte"),
                new Customer("John Mark", "Zamoras",  "Liloan, Cebu")
            });

            ProductProfile product = new ProductProfile("Nature Spring Water", 145, "100001");

            DBContext<ProductProfile>.Add(new List<ProductProfile>
            {
                product,
                new ProductProfile("Dutch Mill (Mini)",38,"100002"),
                new ProductProfile("Gulp Juice (BIG)",55,"100003"),
                new ProductProfile("La Croix (Apple Flavor)",78,"100004")
            });

            product.AddStock(100, DateTime.Parse("2022-01-01"), DateTime.Parse("2023-01-01"));



            Transaction transaction = new Transaction()
            {
                AdditionalFees = 100,
                CustomerId = 3,
                CreatedOn = DateTime.Now,
                CreatedBy = 1,
                TransactionStatus = Enums.TransactionStatusEnum.PAID,
                Remarks = "Successfully Transacted!"
            };

            transaction.AddTransactionItem(1, 1, 4, 145, DateTime.Parse("2022-01-01"), DateTime.Parse("2023-01-01"));

            DBContext<Transaction>.Add(transaction);
        }
    }
}
