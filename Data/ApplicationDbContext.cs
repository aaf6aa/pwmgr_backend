using Microsoft.EntityFrameworkCore;
using pwmgr_backend.Models;

namespace pwmgr_backend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<PasswordEntry> PasswordEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Unique constraint on Username
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Relationship
            modelBuilder.Entity<User>()
                .HasMany(u => u.PasswordEntries)
                .WithOne(pe => pe.User)
                .HasForeignKey(pe => pe.UserId);

            // Unique constraint for ServiceUsernameHash per user
            modelBuilder.Entity<PasswordEntry>()
                .HasIndex(pe => new { pe.UserId, pe.ServiceUsernameHash })
                .IsUnique();
        }
    }
}
