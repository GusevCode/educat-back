using Microsoft.EntityFrameworkCore;
using Vibetech.Educat.DataAccess.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Vibetech.Educat.DataAccess;

public class EducatDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public EducatDbContext(DbContextOptions<EducatDbContext> options)
        : base(options)
    {
    }

    public DbSet<TeacherProfile> TeacherProfiles { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<TeacherSubject> TeacherSubjects { get; set; }
    public DbSet<TeacherStudent> TeacherStudents { get; set; }
    public DbSet<Lesson> Lessons { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<PreparationProgram> PreparationPrograms { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Настройка TeacherProfile
        modelBuilder.Entity<TeacherProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Education).IsRequired();
            entity.Property(e => e.ExperienceYears).IsRequired();
            entity.Property(e => e.HourlyRate).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Rating).HasColumnType("double precision");
            entity.Property(e => e.ReviewsCount).IsRequired();

            entity.HasOne(t => t.User)
                .WithMany(u => u.TeacherProfiles)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Настройка Subject
        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
        });

        // Настройка TeacherSubject
        modelBuilder.Entity<TeacherSubject>(entity =>
        {
            entity.HasKey(e => new { e.TeacherProfileId, e.SubjectId });

            entity.HasOne(ts => ts.TeacherProfile)
                .WithMany(t => t.TeacherSubjects)
                .HasForeignKey(ts => ts.TeacherProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ts => ts.Subject)
                .WithMany(s => s.TeacherSubjects)
                .HasForeignKey(ts => ts.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Настройка TeacherStudent
        modelBuilder.Entity<TeacherStudent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RequestDate).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.AcceptedDate).IsRequired(false);

            entity.HasOne(ts => ts.Teacher)
                .WithMany(t => t.TeacherStudents)
                .HasForeignKey(ts => ts.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ts => ts.Student)
                .WithMany(s => s.Students)
                .HasForeignKey(ts => ts.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Настройка Lesson
        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StartTime).IsRequired();
            entity.Property(e => e.EndTime).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.ConferenceLink).IsRequired(false);
            entity.Property(e => e.WhiteboardLink).IsRequired(false);
            
            entity.HasOne(l => l.Teacher)
                .WithMany(t => t.TeacherLessons)
                .HasForeignKey(l => l.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(l => l.Student)
                .WithMany(s => s.StudentLessons)
                .HasForeignKey(l => l.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(l => l.Subject)
                .WithMany(s => s.Lessons)
                .HasForeignKey(l => l.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Настройка Attachment
        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired();
            entity.Property(e => e.FileType).IsRequired();
            entity.Property(e => e.Base64Content).IsRequired();
            entity.Property(e => e.Size).IsRequired();
            
            entity.HasOne(a => a.Lesson)
                .WithMany(l => l.Attachments)
                .HasForeignKey(a => a.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Настройка Review
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Comment).IsRequired();
            entity.Property(e => e.Rating).IsRequired();
            
            entity.HasOne(r => r.Teacher)
                .WithMany(t => t.Reviews)
                .HasForeignKey(r => r.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(r => r.Student)
                .WithMany()
                .HasForeignKey(r => r.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Настройка PreparationProgram
        modelBuilder.Entity<PreparationProgram>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Description).IsRequired(false);
            
            entity.HasOne(pp => pp.TeacherProfile)
                .WithMany(tp => tp.PreparationPrograms)
                .HasForeignKey(pp => pp.TeacherProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
} 