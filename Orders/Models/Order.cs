namespace Orders.Models
{
    using Orders.Infrastructure;
    using Orders.ViewModels.Base;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Order : IEntity

    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Order()
        {
            RouteOrders = new HashSet<RouteOrder>();
        }

        [Key]
        [Column("idOrder")]
        public int id { get; set; }

        [StringLength(200)]
        public string o_name { get; set; }

        [StringLength(50)]
        public string o_number { get; set; }

        //public int? o_statusId { get; set; }

        public string o_body { get; set; }

        public int o_stepRoute { get; set; }

        public EnumStatus o_statusId { get; set; }

        public int o_ownerUserId { get; set; }

        public decimal? o_summa { get; set; }

        [NotMapped]
        public User WorkUser { get; set; }

        public virtual User Owner { get; set; }


        public DateTime o_date_created { get; set; }
        public string o_buyer { get; set; }
        public string o_CardOrder { get; set; }

        public virtual RouteStatus RouteStatus { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public ICollection<RouteOrder> RouteOrders { get; set; }


    }
}
