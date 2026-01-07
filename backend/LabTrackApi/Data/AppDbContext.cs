using Microsoft.EntityFrameworkCore;
using LabTrackApi.Models;

namespace LabTrackApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.Role);

                entity.Property(u => u.Role)
                    .HasConversion<string>();
            });

            modelBuilder.Entity<Asset>(entity =>
            {
                entity.HasIndex(a => a.QRCode).IsUnique();
                entity.HasIndex(a => a.Status);
                entity.HasIndex(a => a.Category);

                entity.Property(a => a.Status)
                    .HasConversion<string>();

                entity.HasOne(a => a.Creator)
                    .WithMany(u => u.CreatedAssets)
                    .HasForeignKey(a => a.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasIndex(t => t.Status);
                entity.HasIndex(t => t.Priority);
                entity.HasIndex(t => t.AssignedTo);

                entity.Property(t => t.Status)
                    .HasConversion<string>();

                entity.Property(t => t.Priority)
                    .HasConversion<string>();

                entity.HasOne(t => t.Asset)
                    .WithMany(a => a.Tickets)
                    .HasForeignKey(t => t.AssetId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(t => t.Creator)
                    .WithMany(u => u.CreatedTickets)
                    .HasForeignKey(t => t.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Assignee)
                    .WithMany(u => u.AssignedTickets)
                    .HasForeignKey(t => t.AssignedTo)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasIndex(c => c.TicketId);

                entity.HasOne(c => c.Ticket)
                    .WithMany(t => t.Comments)
                    .HasForeignKey(c => c.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.User)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasIndex(a => new { a.EntityType, a.EntityId });

                entity.HasOne(a => a.User)
                    .WithMany()
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
