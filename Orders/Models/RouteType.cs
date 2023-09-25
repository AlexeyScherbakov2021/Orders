namespace Orders.Models
{
    using Orders.Infrastructure;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RouteType")]
    public partial class RouteType //: IEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RouteType()
        {
            RouteOrders = new HashSet<RouteOrder>();
            RouteSteps = new HashSet<RouteStep>();
        }

        [Key]
        [Column("idRouteType")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public EnumTypesStep id { get; set; }

        [StringLength(50)]
        public string rt_name { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RouteOrder> RouteOrders { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RouteStep> RouteSteps { get; set; }

        [NotMapped]
        public bool IsCheck { get; set; } = false;
    }
}
