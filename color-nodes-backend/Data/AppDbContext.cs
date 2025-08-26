using color_nodes_backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace color_nodes_backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Username)
                      .IsRequired(false)
                      .HasMaxLength(50);

                entity.Property(u => u.Score)
                      .HasDefaultValue(0);

                entity.HasOne(u => u.Room)
                      .WithMany(r => r.Users)
                      .HasForeignKey(u => u.RoomId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(r => r.Id);

                modelBuilder.Entity<Room>()
                    .HasIndex(r => r.Code)
                    .IsUnique();

                entity.Property(r => r.LeaderId)
                      .IsRequired();
            });
        }
    }
}
