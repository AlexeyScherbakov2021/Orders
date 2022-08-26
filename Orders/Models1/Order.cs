namespace Orders.Models1
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Order
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Order()
        {
            RouteOrders = new HashSet<RouteOrder>();
        }

        [Key]
        public int idOrder { get; set; }

        [StringLength(200)]
        public string o_name { get; set; }

        [StringLength(50)]
        public string o_number { get; set; }

        public string o_body { get; set; }

        public int o_stepRoute { get; set; }

        public DateTime o_date_created { get; set; }

        public int o_statusId { get; set; }

        [StringLength(150)]
        public string o_buyer { get; set; }

        [StringLength(50)]
        public string o_CardOrder { get; set; }

        public virtual RouteStatu RouteStatu { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RouteOrder> RouteOrders { get; set; }
    }
}
