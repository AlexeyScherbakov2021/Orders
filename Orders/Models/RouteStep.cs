namespace Orders.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RouteStep")]
    public partial class RouteStep : IEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RouteStep()
        {
            RouteOrders = new HashSet<RouteOrder>();
        }

        [Key]
        [Column("idRouteStep")]
        public int id { get; set; }

        public int r_routeId { get; set; }

        public int? r_step { get; set; }

        public int r_userId { get; set; }

        public int r_type { get; set; }

        public bool? r_disabled { get; set; }

        [StringLength(100)]
        public string r_email { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RouteOrder> RouteOrders { get; set; }

        public virtual Route Route { get; set; }

        public virtual RouteType RouteType { get; set; }

        public virtual User User { get; set; }
    }
}
