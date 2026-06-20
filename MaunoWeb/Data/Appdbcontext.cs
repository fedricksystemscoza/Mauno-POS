
using MaunoWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace MaunoWeb.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<SoftwareUpdate> SoftwareUpdates { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Client → Branches (cascade delete)
            modelBuilder.Entity<Branch>()
                .HasOne(b => b.Client)
                .WithMany(c => c.Branches)
                .HasForeignKey(b => b.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Branch → Devices (cascade delete)
            modelBuilder.Entity<Device>()
                .HasOne(d => d.Branch)
                .WithMany(b => b.Devices)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.Cascade);

            // Client → Subscriptions
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Client)
                .WithMany(c => c.Subscriptions)
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Client → SupportTickets
            modelBuilder.Entity<SupportTicket>()
                .HasOne(t => t.Client)
                .WithMany(c => c.SupportTickets)
                .HasForeignKey(t => t.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // SupportTicket → AssignedToUser (no cascade)
            modelBuilder.Entity<SupportTicket>()
                .HasOne(t => t.AssignedToUser)
                .WithMany()
                .HasForeignKey(t => t.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Client → Payments
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Client)
                .WithMany(c => c.Payments)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // User → Notifications
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed admin user (password: admin123 — pre-computed bcrypt hash, fixed value)
            modelBuilder.Entity<User>().HasData(new User
            {
                UserId = 1,
                Username = "admin",
                PasswordHash = "$2b$12$q3VOdpaFHJi3IOcHb3NiuexIf01bNxWQR0QoUs3/MotbE9576u74W",
                Role = "Admin",
                CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        }
    }
}
