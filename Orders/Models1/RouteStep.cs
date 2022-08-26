namespace Orders.Models1
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RouteStep")]
    public partial class RouteStep
    {
        [Key]
        public int idRouteStep { get; set; }

        public int r_routeId { get; set; }

        public int? r_step { get; set; }

        public int? r_userId { get; set; }

        public int? r_type { get; set; }

        public bool? r_enabled { get; set; }

        [StringLength(100)]
        public string r_email { get; set; }

        public virtual Route Route { get; set; }

        public virtual RouteType RouteType { get; set; }

        public virtual User User { get; set; }
    }
}
