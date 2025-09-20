using color_nodes_backend.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace color_nodes_backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Game> Games => Set<Game>();
        public DbSet<GameMove> GameMoves => Set<GameMove>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var jsonOptions = new JsonSerializerOptions();

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Username)
                        .IsRequired() // ⚡ obligatorio
                        .HasMaxLength(10);

                entity.HasIndex(u => u.Username) // ⚡ índice único
                      .IsUnique();

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

            var listStringConverter = new ValueConverter<List<string>, string>(
                v => JsonSerializer.Serialize(v, jsonOptions),
                v => string.IsNullOrWhiteSpace(v)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(v, jsonOptions)!
            );

            var listIntConverter = new ValueConverter<List<int>, string>(
                v => JsonSerializer.Serialize(v, jsonOptions),
                v => string.IsNullOrWhiteSpace(v)
                    ? new List<int>()
                    : JsonSerializer.Deserialize<List<int>>(v, jsonOptions)!
            );

            modelBuilder.Entity<Game>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Cups).HasConversion(listStringConverter);
                e.Property(x => x.TargetPattern).HasConversion(listStringConverter);
                e.Property(x => x.PlayerOrder).HasConversion(listIntConverter);
            });
        }
    }
}
