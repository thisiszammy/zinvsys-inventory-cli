using MidtermProject.Attributes;
using MidtermProject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidtermProject.Entities
{
    internal class AdministrativeUser : ApplicationUser
    {
        public AdministrativeUser()
        {

        }

        public AdministrativeUser(string firstName, string lastName, string username, string password)
            :base(firstName, lastName, username,   password, ApplicationUserTypeEnum.ADMIN)
        {

        }


        public override TEntity Clone<TEntity>()
        {
            var administrativeUser = new AdministrativeUser()
            {
                Uid = this.Uid,
                CreatedBy = this.CreatedBy,
                CreatedOn = this.CreatedOn,
                DeletedBy = this.DeletedBy,
                DeletedOn = this.DeletedOn,
                FirstName = this.FirstName,
                IsDeleted = this.IsDeleted,
                LastModifiedBy = this.LastModifiedBy,
                LastModifiedOn = this.LastModifiedOn,
                LastName = this.LastName,
                Password = this.Password,
                Username = this.Username
            };

            return (TEntity)(object)administrativeUser;
        }

    }
}
