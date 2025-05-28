using System;
using System.Collections.Generic;
using Vibetech.Educat.DataAccess.Models;

namespace Vibetech.Educat.Services.DTO
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;
        public string PhotoBase64 { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
    }

    public class TeacherRequestDTO
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
    }

    public class LessonDTO
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ConferenceLink { get; set; } = string.Empty;
        public string BoardLink { get; set; } = string.Empty;
    }

    public class SubjectStatisticsDTO
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public int UpcomingLessons { get; set; }
        public decimal HoursSpent { get; set; }
    }

    public class StudentStatisticsDTO
    {
        public int TotalTeachers { get; set; }
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public int UpcomingLessons { get; set; }
        public Dictionary<int, int> LessonsBySubject { get; set; } = new();
        public Dictionary<string, int> LessonsByWeekday { get; set; } = new();
        public Dictionary<string, int> SubjectStatistics { get; set; } = new();
        public int TeachersCount { get; set; }
        public int TotalLessonHours { get; set; }
    }

    public class TeacherProfileDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Education { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public decimal HourlyRate { get; set; }
        public double Rating { get; set; }
        public int ReviewsCount { get; set; }
        public string PhotoBase64 { get; set; } = string.Empty;
        public List<SubjectDTO> Subjects { get; set; } = new();
        public List<PreparationProgramDTO> PreparationPrograms { get; set; } = new();
        public string Email { get; set; } = string.Empty;
        public UserDTO? User { get; set; }
    }

    public class SubjectDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class PreparationProgramDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class TeacherStudentDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime AcceptedDate { get; set; }
        public string ContactInfo { get; set; } = string.Empty;
    }

    public class TeacherStatisticsDTO
    {
        public int TotalStudents { get; set; }
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public int UpcomingLessons { get; set; }
        public double Rating { get; set; }
        public int ReviewsCount { get; set; }
        public Dictionary<int, int> LessonsBySubject { get; set; } = new();
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
    }

    public class StudentRequestDTO
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int TeacherId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class AttachmentDTO
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long Size { get; set; }
        public string Base64Content { get; set; } = string.Empty;
    }
    
    public class StudentProfile
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;
        public string PhotoBase64 { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
    }
    
    public class TeacherInfoDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public decimal HourlyRate { get; set; }
        public DateTime AcceptedDate { get; set; }
    }

    public class ReviewDTO
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
} 