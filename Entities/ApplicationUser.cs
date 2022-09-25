using MidtermProject.Attributes;
using MidtermProject.DAL;
using MidtermProject.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidtermProject.Entities
{
    internal class ApplicationUser : BaseEntity
    {
        string firstName;
        string lastName;
        string username;
        string password;
        ApplicationUserTypeEnum userType;

        [UpdateEntity]
        public string Username
        {
            get { return username; }
            set { username = value ?? string.Empty; }
        }

        [UpdateEntity]
        public string Password
        {
            get { return password; }
            set { password = value ?? string.Empty; }
        }


        [UpdateEntity]
        public string FirstName
        {
            get { return firstName; }
            set { firstName = value; }
        }

        [UpdateEntity]
        public string LastName
        {
            get { return lastName; }
            set { lastName = value; }
        }

        [ConsoleTableColumn("Name", 1)]
        public string CompleteName
        {
            get => $"{FirstName} {LastName}";
        }
        public ApplicationUserTypeEnum UserType { get => userType; set => userType = value; }

        public ApplicationUser()
        {

        }
        public ApplicationUser(string firstName, string lastName, string? username, string? password, ApplicationUserTypeEnum userType)
        {
            FirstName = firstName;
            LastName = lastName;
            CreatedOn = DateTime.Now;
            CreatedBy = (ApplicationWorker.currentUser == null) ? -1 : ApplicationWorker.currentUser.Uid;
            IsDeleted = false;
            this.username = username ?? string.Empty;
            this.password = password ?? string.Empty;
            this.UserType = userType;
        }


        public override TEntity Clone<TEntity>() => throw new NotImplementedException("This entity is not cloneable!");

        public virtual bool AuthenticateUser(string password)
        {
            return password == this.password;
        }
    }
}
