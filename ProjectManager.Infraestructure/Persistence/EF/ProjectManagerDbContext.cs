using Microsoft.EntityFrameworkCore;
using ProjectManager.Domain.Entities;

namespace ProjectManager.Infraestructure.Persistence.EF
{
    public class ProjectManagerDbContext(DbContextOptions<ProjectManagerDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Project> Projects => Set<Project>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id).ValueGeneratedOnAdd();
                entity.Property(u => u.Name).IsRequired().HasMaxLength(200);
                entity.Property(u => u.UserName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.Email).IsRequired().HasMaxLength(320);
                entity.Property(u => u.Role).IsRequired();
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.ToTable("Projects");
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).HasMaxLength(64);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
                entity.Property(p => p.Description).HasMaxLength(2000);
                entity.Property(p => p.OwnerId).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Status).IsRequired().HasMaxLength(50);
                entity.Property(p => p.CreatedAtUtc).IsRequired();
                entity.Property(p => p.UpdatedAtUtc);
            });
        }
    }
}