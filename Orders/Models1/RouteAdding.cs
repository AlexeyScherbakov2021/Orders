namespace Orders.Models1
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RouteAdding")]
    public partial class RouteAdding
    {
        [Key]
        public int idAdding { get; set; }

        public int ad_routeOrderId { get; set; }

        [StringLength(200)]
        public string ad_text { get; set; }

        [Column(TypeName = "image")]
        public byte[] ad_file { get; set; }

        public virtual RouteOrder RouteOrder { get; set; }
    }
}
