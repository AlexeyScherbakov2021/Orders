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
    [Table("Roles")]
    public class Role
    {
        [Key]
        public EnumRoles role_Id { get; set; }
        public string role_name { get; set; }

        [NotMapped]
        public bool IsCheckRole { get; set; }
    }
}
