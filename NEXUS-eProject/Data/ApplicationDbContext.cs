using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NEXUS_eProject.Models;

namespace NEXUS_eProject.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


        // =====================================================
        // NEXUS DATABASE TABLES
        // =====================================================

        public DbSet<Employee> Employees { get; set; }

        public DbSet<Shop> Shops { get; set; }

        public DbSet<Vendor> Vendors { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<Plan> Plans { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<Connection> Connections { get; set; }

        public DbSet<Bill> Bills { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<Feedback> Feedbacks { get; set; }

        public DbSet<SystemSetting> SystemSettings { get; set; }


        // =====================================================
        // MODEL CONFIGURATION
        // =====================================================

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // =================================================
            // PRODUCT → VENDOR
            // =================================================

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Vendor)
                .WithMany(v => v.Products)
                .HasForeignKey(p => p.VendorId)
                .OnDelete(DeleteBehavior.Restrict);


            // =================================================
            // ORDER → CUSTOMER
            // =================================================

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);


            // =================================================
            // ORDER → PLAN
            // =================================================

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Plan)
                .WithMany()
                .HasForeignKey(o => o.PlanId)
                .OnDelete(DeleteBehavior.Restrict);


            // =================================================
            // CONNECTION → ORDER
            // =================================================

            modelBuilder.Entity<Connection>()
                .HasOne(c => c.Order)
                .WithMany()
                .HasForeignKey(c => c.OrderId)
                .OnDelete(DeleteBehavior.Restrict);


            // =================================================
            // CONNECTION → CUSTOMER
            // =================================================

            modelBuilder.Entity<Connection>()
                .HasOne(c => c.Customer)
                .WithMany()
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);


            // =================================================
            // CONNECTION → PLAN
            // =================================================

            modelBuilder.Entity<Connection>()
                .HasOne(c => c.Plan)
                .WithMany()
                .HasForeignKey(c => c.PlanId)
                .OnDelete(DeleteBehavior.Restrict);


            // =================================================
            // BILL → CUSTOMER
            // =================================================

            modelBuilder.Entity<Bill>()
                .HasOne(b => b.Customer)
                .WithMany()
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);


            // =================================================
            // BILL → CONNECTION
            // =================================================

            modelBuilder.Entity<Bill>()
                .HasOne(b => b.Connection)
                .WithMany()
                .HasForeignKey(b => b.ConnectionId)
                .OnDelete(DeleteBehavior.Restrict);


            // =================================================
            // PAYMENT → CUSTOMER
            // =================================================

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Customer)
                .WithMany()
                .HasForeignKey(p => p.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);


            // =================================================
            // PAYMENT → BILL
            // =================================================

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Bill)
                .WithMany()
                .HasForeignKey(p => p.BillId)
                .OnDelete(DeleteBehavior.Restrict);


            // =================================================
            // FEEDBACK → CUSTOMER
            // =================================================

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Customer)
                .WithMany()
                .HasForeignKey(f => f.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);


            // =================================================
            // SYSTEM SETTINGS
            // =================================================

            modelBuilder.Entity<SystemSetting>(entity =>
            {
                entity.HasKey(s => s.SystemSettingId);

                entity.Property(s => s.ApplicationName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(s => s.SupportEmail)
                    .HasMaxLength(150);

                entity.Property(s => s.SupportPhone)
                    .HasMaxLength(30);

                entity.Property(s => s.CompanyAddress)
                    .HasMaxLength(300);

                entity.Property(s => s.TaxPercentage)
                    .HasPrecision(5, 2);

                entity.Property(s => s.Currency)
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(s => s.CityCode)
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(s => s.BroadbandPrefix)
                    .HasMaxLength(5)
                    .IsRequired();

                entity.Property(s => s.TelephonePrefix)
                    .HasMaxLength(5)
                    .IsRequired();

                entity.Property(s => s.DialUpPrefix)
                    .HasMaxLength(5)
                    .IsRequired();

                entity.Property(s => s.UpdatedBy)
                    .HasMaxLength(150);
            });
        }
    }
}