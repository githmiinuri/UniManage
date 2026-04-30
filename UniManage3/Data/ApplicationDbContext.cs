using Microsoft.EntityFrameworkCore;
using UniManage3.Models;

namespace UniManage3.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<Administrator> Administrators { get; set; }
        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Administrator" },
                new Role { Id = 2, Name = "Lecturer" },
                new Role { Id = 3, Name = "Student" }
            );

            modelBuilder.Entity<Administrator>(b =>
            {
                b.HasKey(a => a.Id);
                b.Property(a => a.Email).IsRequired().HasMaxLength(256).HasColumnType("varchar(256)");
                b.Property(a => a.PasswordHash).IsRequired().HasMaxLength(512).HasColumnType("varchar(512)");
                b.Property(a => a.FirstName).HasMaxLength(128).HasColumnType("varchar(128)");
                b.Property(a => a.LastName).HasMaxLength(128).HasColumnType("varchar(128)");
                b.Property(a => a.AddressLine1).HasMaxLength(256).HasColumnType("varchar(256)");
                b.Property(a => a.AddressLine2).HasMaxLength(256).HasColumnType("varchar(256)");
                b.Property(a => a.City).HasMaxLength(128).HasColumnType("varchar(128)");
                b.Property(a => a.NICNumber).HasMaxLength(64).HasColumnType("varchar(64)");
                b.Property(a => a.Province).HasMaxLength(128).HasColumnType("varchar(128)");
                b.Property(a => a.ContactNumber).HasColumnType("int");
                b.Property(a => a.ZipCode).HasColumnType("int");
            });

            modelBuilder.Entity<Lecturer>(b =>
            {
                b.HasKey(l => l.Id);
                b.Property(l => l.Email).IsRequired().HasMaxLength(256).HasColumnType("varchar(256)");
                b.Property(l => l.PasswordHash).IsRequired().HasMaxLength(512).HasColumnType("varchar(512)");
                b.Property(l => l.FirstName).HasMaxLength(128).HasColumnType("varchar(128)");
                b.Property(l => l.LastName).HasMaxLength(128).HasColumnType("varchar(128)");
                b.Property(l => l.AddressLine1).HasMaxLength(256).HasColumnType("varchar(256)");
                b.Property(l => l.AddressLine2).HasMaxLength(256).HasColumnType("varchar(256)");
                b.Property(l => l.City).HasMaxLength(128).HasColumnType("varchar(128)");
                b.Property(l => l.NICNumber).HasMaxLength(64).HasColumnType("varchar(64)");
                b.Property(l => l.Province).HasMaxLength(128).HasColumnType("varchar(128)");
                b.Property(l => l.ContactNumber).HasColumnType("int");
                b.Property(l => l.ZipCode).HasColumnType("int");
            });

            modelBuilder.Entity<Student>(b =>
            {
                b.HasKey(e => e.Id);
                b.Property(e => e.Email).IsRequired().HasMaxLength(256).HasColumnType("varchar(256)");
                b.Property(e => e.PasswordHash).IsRequired().HasMaxLength(512).HasColumnType("varchar(512)");
                b.Property(e => e.FirstName).HasMaxLength(128).HasColumnType("varchar(128)");
                b.Property(e => e.LastName).HasMaxLength(128).HasColumnType("varchar(128)");
                b.Property(e => e.AddressLine1).HasMaxLength(256).HasColumnType("varchar(256)");
                b.Property(e => e.AddressLine2).HasMaxLength(256).HasColumnType("varchar(256)");
                b.Property(e => e.City).HasMaxLength(128).HasColumnType("varchar(128)");
                b.Property(e => e.NICNumber).HasMaxLength(64).HasColumnType("varchar(64)");
                b.Property(e => e.Province).HasMaxLength(128).HasColumnType("varchar(128)");
                b.Property(e => e.ContactNumber).HasColumnType("int");
                b.Property(e => e.ZipCode).HasColumnType("int");
            });

            modelBuilder.Entity<User>(b =>
            {
                b.HasKey(u => u.Id);
                b.Property(u => u.FullName).HasMaxLength(256).HasColumnType("varchar(256)");
                b.Property(u => u.Email).HasMaxLength(256).HasColumnType("varchar(256)");
                b.Property(u => u.Password).HasMaxLength(512).HasColumnType("varchar(512)");
                b.Property(u => u.RoleId).HasColumnType("int");
                b.Property(u => u.IsActive).HasColumnType("tinyint(1)");
                b.Property(u => u.IsApproved).HasColumnType("tinyint(1)");
                b.Property(u => u.CreatedAt).HasColumnType("datetime");
                b.Property(u => u.LastLogin).HasColumnType("datetime");
            });
        }
    }
}