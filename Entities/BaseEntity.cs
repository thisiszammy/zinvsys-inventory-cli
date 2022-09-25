using MidtermProject.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidtermProject.Entities
{
    internal abstract class BaseEntity
    {
        [ConsoleTableColumn("Id", 0)]
        public int Uid { get; set; }

        [ConsoleTableColumn("Created On", 100)]
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public int? LastModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }
        public int? DeletedBy { get; set; }
        public abstract TEntity Clone<TEntity>() where TEntity : BaseEntity;
    }
}
