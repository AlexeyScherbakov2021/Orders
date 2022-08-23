namespace Orders.Models
{
    using Orders.Infrastructure;
    using Orders.ViewModels.Base;
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

        public bool ro_enabled { get; set; }

        public string ro_text { get; set; }

        public EnumCheckedStatus ro_check { get; set; }

        public int ro_statusId { get; set; }

        public DateTime? ro_date_check { get; set; }

        public int? ro_return_step { get; set; }

        public int? ro_ownerId { get; set; }

        public virtual Order Order { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RouteAdding> RouteAddings { get; set; }

        public virtual RouteStatus RouteStatus { get; set; }

        public virtual RouteType RouteType { get; set; }

        public virtual User User { get; set; }

        public string NameStep => ro_step.ToString() + (ro_return_step == null ? "" : "." +  ro_return_step?.ToString());
    }
}
