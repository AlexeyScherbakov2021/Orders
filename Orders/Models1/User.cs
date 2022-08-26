namespace Orders.Models1
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class User
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public User()
        {
            RouteOrders = new HashSet<RouteOrder>();
            RouteOrders1 = new HashSet<RouteOrder>();
            RouteSteps = new HashSet<RouteStep>();
        }

        [Key]
        public int idUser { get; set; }

        [StringLength(30)]
        public string u_login { get; set; }

        [StringLength(30)]
        public string u_pass { get; set; }

        [StringLength(150)]
        public string u_name { get; set; }

        public int u_role { get; set; }

        [StringLength(150)]
        public string u_email { get; set; }

        [StringLength(150)]
        public string u_otdel { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RouteOrder> RouteOrders { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RouteOrder> RouteOrders1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RouteStep> RouteSteps { get; set; }
    }
}
