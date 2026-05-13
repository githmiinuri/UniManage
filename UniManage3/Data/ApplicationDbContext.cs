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
        public DbSet<Course> Courses { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<CourseMaterial> CourseMaterials { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Batch> Batches { get; set; }
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }
        public DbSet<Semester> Semesters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Administrator" },
                new Role { Id = 2, Name = "Lecturer" },
                new Role { Id = 3, Name = "Student" }
            );

            // Configure CourseMaterial.MaterialType as integer-backed enum to match DB
            modelBuilder.Entity<CourseMaterial>(b =>
            {
                // store enum as int to match existing migration/schema
                b.Property(cm => cm.MaterialType)
                    .HasConversion<int>()
                    .HasColumnType("int");

                b.Property(cm => cm.MaterialName).IsRequired().HasMaxLength(255).HasColumnType("varchar(255)");
                b.Property(cm => cm.FilePath).HasColumnType("longtext");
                b.Property(cm => cm.Description).HasColumnType("longtext");
                b.Property(cm => cm.Duration).HasMaxLength(50).HasColumnType("varchar(50)");
                b.HasOne(cm => cm.Module)
                 .WithMany(m => m.CourseMaterials)
                 .HasForeignKey(cm => cm.ModuleId)
                 .OnDelete(DeleteBehavior.Cascade);
            });


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
                // Map UserId and relationship to Users table
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

            modelBuilder.Entity<Course>(b =>
            {
                b.HasKey(c => c.Id);

                // Basic property configurations
                b.Property(c => c.CourseCode)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("varchar(100)");

                b.Property(c => c.CourseName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnType("varchar(255)");

                b.Property(c => c.Description)
                    .HasColumnType("text");

                b.Property(c => c.Credits)
                    .IsRequired()
                    .HasColumnType("int");

                b.Property(c => c.Duration)
         .HasMaxLength(50)
         .HasColumnType("varchar(50)")
         .HasDefaultValue("Not Specified");

                b.Property(c => c.PrerequisiteCourseId)
                    .HasColumnType("int");

                // Configure the self-referencing relationship for Prerequisites
                b.HasOne(c => c.PrerequisiteCourse)
                 .WithMany() // A course can be a prerequisite for many other courses
                 .HasForeignKey(c => c.PrerequisiteCourseId)
                 .OnDelete(DeleteBehavior.Restrict);

                // Note: Restrict is used here to prevent accidental deletion 
                // of a required prerequisite course.

                b.Property(c => c.IsActive).HasColumnType("tinyint(1)").HasDefaultValue(1);

                b.HasOne(c => c.Coordinator)
                 .WithMany()
                 .HasForeignKey(c => c.CoordinatorId)
                 .OnDelete(DeleteBehavior.SetNull);

                // Department relationship
                b.Property(c => c.DepartmentId).HasColumnType("int");
                b.HasOne(c => c.Department)
                 .WithMany()
                 .HasForeignKey(c => c.DepartmentId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Enrollment>(b =>
            {
                b.HasKey(e => e.Id);

                b.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnType("varchar(20)");

                b.Property(e => e.EnrollmentDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Configure relationship with Student
                b.HasOne(e => e.Student)
                    .WithMany() // A student can have many enrollment records
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Configure relationship with Course
                b.HasOne(e => e.Course)
                    .WithMany() // A course can have many students enrolled
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);

            });
            modelBuilder.Entity<Module>(b =>
            {
                b.HasKey(m => m.Id);
                b.Property(m => m.ModuleCode).IsRequired().HasMaxLength(50).HasColumnType("varchar(50)");
                b.Property(m => m.ModuleName).IsRequired().HasMaxLength(255).HasColumnType("varchar(255)");
                b.Property(m => m.Description).HasColumnType("text");
                b.Property(m => m.Credits).HasColumnType("int");
                b.Property(m => m.IsActive)
     .IsRequired()
     .HasDefaultValue(true)
     .HasColumnType("tinyint(1)");


                // Existing relationship with Course
                b.HasOne(m => m.Course)
                 .WithMany(c => c.Modules)
                 .HasForeignKey(m => m.CourseId)
                 .OnDelete(DeleteBehavior.Cascade);

                // NEW: Optional relationship with Lecturer (Module Leader)
                b.Property(m => m.LecturerId).HasColumnType("int");
                b.HasOne(m => m.Lecturer)
                 .WithMany()
                 .HasForeignKey(m => m.LecturerId)
                 .OnDelete(DeleteBehavior.SetNull);

                // Link Module to Semester
                b.HasOne(m => m.Semester)
                 .WithMany(s => s.Modules) // Ensure Semester.cs has: public virtual ICollection<Module> Modules { get; set; }
                 .HasForeignKey(m => m.SemesterId)
                 .OnDelete(DeleteBehavior.SetNull);

                // Index for faster lookups when filtering by semester
                b.HasIndex(m => m.SemesterId);
            });


            modelBuilder.Entity<Assignment>(b =>
            {
                b.HasKey(a => a.Id);
                b.Property(a => a.AssignmentName).IsRequired().HasMaxLength(255).HasColumnType("varchar(255)");
                b.Property(a => a.Description).HasColumnType("text");
                b.Property(a => a.IssuedDate).HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP");
                b.Property(a => a.DeadlineDate).HasColumnType("datetime");
                b.Property(a => a.LateSubmitDate).HasColumnType("datetime");
                b.Property(a => a.UpdatedDate).HasColumnType("datetime");
                b.Property(a => a.ResourceFilePath).IsRequired().HasColumnType("varchar(500)");

                // Relationship with Module
                b.HasOne(a => a.Module)
                 .WithMany(m => m.Assignments)
                 .HasForeignKey(a => a.ModuleId)
                 .OnDelete(DeleteBehavior.Cascade);

                // Relationship with Batch (each assignment belongs to one batch)
                b.Property(a => a.BatchId).HasColumnType("int");
                b.HasOne(a => a.Batch)
                 .WithMany(batch => batch.Assignments)
                 .HasForeignKey(a => a.BatchId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Batch>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.BatchCode).IsRequired().HasMaxLength(50).HasColumnType("varchar(50)");
                b.Property(x => x.BatchName).IsRequired().HasMaxLength(255).HasColumnType("varchar(255)");
                b.Property(x => x.CreatedAt).HasColumnType("datetime");
                b.Property(p => p.AllowExceptions)
          .HasColumnType("tinyint(1)")
          .HasDefaultValue(false);

                b.Property(p => p.MinStudents)
                  .HasDefaultValue(10);

                b.Property(p => p.MaxStudents)
                  .HasDefaultValue(30);
            });

            modelBuilder.Entity<AssignmentSubmission>(b =>
            {
                b.HasKey(x => x.Id);
                b.Property(x => x.StudentId).HasColumnType("int");
                b.Property(x => x.AssignmentId).HasColumnType("int");

                // Store enum as integer to match DB column type (fix InvalidCastException when DB has int)
                b.Property(x => x.Status).HasConversion<int>().HasColumnType("int");

                b.Property(x => x.SubmittedTime).HasColumnType("datetime");
                b.Property(x => x.SubmittedFilePath).HasMaxLength(1024).HasColumnType("varchar(1024)");
                b.Property(x => x.Marks).HasColumnType("double");
                b.Property(x => x.Grade).HasMaxLength(8).HasColumnType("varchar(8)");
                b.Property(x => x.Review).HasMaxLength(2000).HasColumnType("varchar(2000)");

                b.HasOne(x => x.Student)
                 .WithMany()
                 .HasForeignKey(x => x.StudentId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(x => x.Assignment)
                 .WithMany()
                 .HasForeignKey(x => x.AssignmentId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Semester>(b =>
            {
                // Define the relationship to Course
                b.HasOne(s => s.Course)
                 .WithMany(c => c.Semesters) // Ensure your Course model has: public virtual ICollection<Semester> Semesters { get; set; }
                 .HasForeignKey(s => s.CourseId)
                 .OnDelete(DeleteBehavior.Cascade); // Matches ON DELETE CASCADE in your SQL
                b.Property(s => s.SemesterNumber)
     .HasDefaultValue(1)
     .HasColumnType("int");

                // Ensure proper indexing for performance
                b.HasIndex(s => s.CourseId);
            });

            modelBuilder.Entity<LibraryResource>(b =>
            {
                // Ensure the file path length matches the MySQL VARCHAR(1024)
                b.Property(p => p.FilePath)
                 .HasMaxLength(1024)
                 .IsRequired();

                // Matches: CONSTRAINT FK_Library_Department FOREIGN KEY (DepartmentId) REFERENCES departments(Id) ON DELETE SET NULL
                b.HasOne(lr => lr.Department)
                 .WithMany() // Only add a collection to Department if you need to list resources from the Department object
                 .HasForeignKey(lr => lr.DepartmentId)
                 .OnDelete(DeleteBehavior.SetNull);

                // Default timestamp configuration for MySQL
                b.Property(p => p.UploadDate)
                 .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            modelBuilder.Entity<Event>(b =>
            {
                // Define the relationship to the Administrator
                b.HasOne(e => e.CreatedBy)
                 .WithMany() // Only add a collection to Administrator if you need to track events per admin
                 .HasForeignKey(e => e.CreatedByAdminId)
                 .OnDelete(DeleteBehavior.SetNull); // Common for auditing; keep event if admin is deleted

                // Mapping for specific data types if necessary
                b.Property(p => p.Description).HasColumnType("text");

                // Ensure Title and EventDate are properly indexed if you plan to filter by them
                b.HasIndex(e => e.EventDate);
            });

            modelBuilder.Entity<Class>(b =>
            {
                // Explicitly map MySQL TIME types
                b.Property(p => p.StartTime).HasColumnType("time");
                b.Property(p => p.EndTime).HasColumnType("time");

                // Configure Foreign Key Relationships with Restricted Delete
                b.HasOne(c => c.Batch)
                 .WithMany() // Or .WithMany(b => b.Classes) if added to Batch model
                 .HasForeignKey(c => c.BatchId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(c => c.Module)
                 .WithMany()
                 .HasForeignKey(c => c.ModuleId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(c => c.Lecturer)
                 .WithMany()
                 .HasForeignKey(c => c.LecturerId)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(c => c.Semester)
                 .WithMany()
                 .HasForeignKey(c => c.SemesterId)
                 .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}