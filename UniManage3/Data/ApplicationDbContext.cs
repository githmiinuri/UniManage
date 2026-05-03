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

        public DbSet<Department> Departments { get; set; }
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
                b.Property(a => a.UserId).HasColumnType("int");
                b.HasIndex(a => a.UserId).IsUnique();
                b.HasOne(a => a.User).WithMany().HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Cascade);
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
                b.Property(l => l.UserId).HasColumnType("int");
                b.HasIndex(l => l.UserId).IsUnique();
                b.HasOne(l => l.User).WithMany().HasForeignKey(l => l.UserId).OnDelete(DeleteBehavior.Cascade);
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
                b.Property(e => e.UserId).HasColumnType("int");
                b.HasIndex(e => e.UserId).IsUnique();
                b.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
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

            modelBuilder.Entity<Department>(b =>
            {
                b.HasKey(d => d.Id);
                b.Property(d => d.DepartmentName).IsRequired().HasMaxLength(255).HasColumnType("varchar(255)");
                b.Property(d => d.DepartmentCode).IsRequired().HasMaxLength(50).HasColumnType("varchar(50)");
                b.HasIndex(d => d.DepartmentCode).IsUnique();
                b.Property(d => d.Description).HasColumnType("text");
                b.Property(d => d.HeadOfDepartmentId).HasColumnType("int");
                b.Property(d => d.CreatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
                b.Property(d => d.UpdatedAt).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");

                // Relationship to Lecturer (Head of Department)
                b.HasOne(d => d.HeadOfDepartment)
                 .WithMany()
                 .HasForeignKey(d => d.HeadOfDepartmentId)
                 .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}