namespace Orders.Models
{
    using Orders.ViewModels.Base;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RouteAdding")]
    public partial class RouteAdding : IEntity
    {
        [Key]
        [Column("idAdding")]
        public int id { get; set; }

        public int ad_routeOrderId { get; set; }

        [StringLength(200)]
        public string ad_text { get; set; }

        //[Column(TypeName = "image")]
        //public byte[] ad_file { get; set; }

        [NotMapped]
        public string FullName;

        public virtual RouteOrder RouteOrder { get; set; }
    }
}
