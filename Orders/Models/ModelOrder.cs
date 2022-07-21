using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace Orders.Models
{
    public partial class ModelOrder : DbContext
    {
        public ModelOrder()
            : base("name=ModelOrder")
        {
        }

        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<RouteAdding> RouteAddings { get; set; }
        public virtual DbSet<RouteOrder> RouteOrders { get; set; }
        public virtual DbSet<Route> Routes { get; set; }
        public virtual DbSet<RouteStatus> RouteStatus { get; set; }
        public virtual DbSet<RouteStep> RouteSteps { get; set; }
        public virtual DbSet<RouteType> RouteTypes { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .Property(e => e.o_name)
                .IsUnicode(false);

            modelBuilder.Entity<Order>()
                .Property(e => e.o_number)
                .IsUnicode(false);

            modelBuilder.Entity<Order>()
                .Property(e => e.o_body)
                .IsUnicode(false);

            modelBuilder.Entity<Order>()
                .HasMany(e => e.RouteOrders)
                .WithRequired(e => e.Order)
                .HasForeignKey(e => e.ro_orderId);

            modelBuilder.Entity<RouteAdding>()
                .Property(e => e.ad_text)
                .IsUnicode(false);

            modelBuilder.Entity<RouteOrder>()
                .Property(e => e.ro_text)
                .IsUnicode(false);

            modelBuilder.Entity<RouteOrder>()
                .HasMany(e => e.RouteAddings)
                .WithRequired(e => e.RouteOrder)
                .HasForeignKey(e => e.ad_routeOrderId);

            modelBuilder.Entity<Route>()
                .Property(e => e.r_name)
                .IsUnicode(false);

            modelBuilder.Entity<Route>()
                .HasMany(e => e.RouteSteps)
                .WithRequired(e => e.Route)
                .HasForeignKey(e => e.r_routeId);

            modelBuilder.Entity<RouteStatus>()
                .Property(e => e.sr_name)
                .IsUnicode(false);

            modelBuilder.Entity<RouteStatus>()
                .HasMany(e => e.RouteOrders)
                .WithRequired(e => e.RouteStatus)
                .HasForeignKey(e => e.ro_statusId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RouteStatus>()
                .HasMany(e => e.Orders)
                .WithRequired(e => e.RouteStatus)
                .HasForeignKey(e => e.o_statusId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RouteStep>()
                .Property(e => e.r_email)
                .IsUnicode(false);

            modelBuilder.Entity<RouteType>()
                .Property(e => e.rt_name)
                .IsUnicode(false);

            modelBuilder.Entity<RouteType>()
                .HasMany(e => e.RouteOrders)
                .WithRequired(e => e.RouteType)
                .HasForeignKey(e => e.ro_typeId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RouteType>()
                .HasMany(e => e.RouteSteps)
                .WithRequired(e => e.RouteType)
                .HasForeignKey(e => e.r_type)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .Property(e => e.u_login)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .Property(e => e.u_pass)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .Property(e => e.u_name)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .Property(e => e.u_email)
                .IsUnicode(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.RouteOrders)
                .WithRequired(e => e.User)
                .HasForeignKey(e => e.ro_userId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.RouteSteps)
                .WithRequired(e => e.User)
                .HasForeignKey(e => e.r_userId)
                .WillCascadeOnDelete(false);
        }
    }
}
