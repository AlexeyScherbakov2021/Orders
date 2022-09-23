using Orders.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Models
{
    [Table("RoleUser")]
    public class RoleUser : IEntity
    {
        public RoleUser()
        {
        }

        [Key]
        [Column("ru_id")]
        public int id { get; set; }
        public int ru_user_id { get; set; }
        public EnumRoles ru_role_id { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual User Users { get; set; }
    }
}
