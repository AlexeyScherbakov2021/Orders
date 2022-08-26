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
    [Table("ChildRoute")]

    public partial class ChildRoute : IEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ChildRoute()
        {
            RouteAddings = new HashSet<RouteAdding>();
        }


        [Key]
        [Column("idChildRoute")]
        public int id { get; set; }
        public int cr_userId { get; set; }

        public int cr_typeId { get; set; }

        public int cr_step { get; set; }

        public string cr_text { get; set; }

        public EnumCheckedStatus cr_check { get; set; }

        public int cr_statusId { get; set; }

        public DateTime? cr_date_check { get; set; }

        public int? cr_parentId { get; set; }

        public int? cr_ownerId { get; set; }

        public virtual ICollection<RouteAdding> RouteAddings { get; set; }

        public virtual RouteOrder RouteOrder { get; set; }

        public virtual RouteStatus RouteStatus { get; set; }

        public virtual RouteType RouteType { get; set; }

        public virtual User User { get; set; }

        public string NameStep => RouteOrder.ro_step.ToString() + "." + cr_step.ToString();

    }
}
