namespace Orders.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RouteOrder")]
    public partial class RouteOrder : IEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RouteOrder()
        {
            RouteAddings = new HashSet<RouteAdding>();
        }

        [Key]
        [Column("idRouteOrder")]
        public int id { get; set; }

        public int ro_orderId { get; set; }

        public int ro_RouteId { get; set; }

        public int ro_userId { get; set; }

        public int ro_typeId { get; set; }

        public int ro_step { get; set; }

        public bool ro_disabled { get; set; }

        public string ro_text { get; set; }

        public int ro_check { get; set; }

        public virtual Order Order { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RouteAdding> RouteAddings { get; set; }

        public virtual RouteStep RouteStep { get; set; }

        public virtual RouteType RouteType { get; set; }

        public virtual User User { get; set; }
    }
}
