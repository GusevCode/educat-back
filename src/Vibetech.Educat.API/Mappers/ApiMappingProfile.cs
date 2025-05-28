using AutoMapper;
using Vibetech.Educat.API.Models;
using Vibetech.Educat.Services.DTO;

namespace Vibetech.Educat.API.Mappers;

public class ApiMappingProfile : Profile
{
    public ApiMappingProfile()
    {
        // Маппинг для TeacherProfile
        CreateMap<TeacherProfileDTO, TeacherProfileDto>()
            .ForMember(dest => dest.User, opt => 
                opt.MapFrom(src => src.User));
        
        // Маппинг для вложенных объектов
        CreateMap<SubjectDTO, SubjectDto>();
        CreateMap<PreparationProgramDTO, PreparationProgramDto>();
        
        // Маппинг для StudentProfile
        CreateMap<StudentProfile, StudentProfileDto>()
            .ForMember(dest => dest.User, opt => 
                opt.MapFrom(src => new UserDto
                {
                    Id = src.UserId,
                    FirstName = src.FirstName,
                    LastName = src.LastName,
                    MiddleName = src.MiddleName,
                    Email = src.Email,
                    BirthDate = src.BirthDate,
                    Gender = src.Gender,
                    ContactInfo = src.ContactInfo,
                    PhotoBase64 = src.PhotoBase64,
                    IsTeacher = false,
                    Roles = new List<string>() { "STUDENT" }
                }));
        
        // Маппинг для LessonDTO
        CreateMap<LessonDTO, LessonDto>()
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime))
            .ForMember(dest => dest.WhiteboardLink, opt => opt.MapFrom(src => src.BoardLink))
            .ForMember(dest => dest.Status, opt => opt.MapFrom((src, dest, member, context) => 
            {
                // Если статус уже Completed или Cancelled, оставляем его
                if (src.Status == "Completed" || src.Status == "Cancelled")
                    return src.Status;
                    
                // Если время окончания урока уже прошло, возвращаем Completed
                if (src.EndTime < DateTime.UtcNow && src.Status == "Scheduled")
                    return "Completed";
                    
                return src.Status;
            }));
        
        // Маппинг для StudentRequest
        CreateMap<StudentRequestDTO, StudentRequestDto>();
        
        // Маппинг для Attachment
        CreateMap<AttachmentDTO, AttachmentDto>();
        
        // Маппинг для TeacherStatistics
        CreateMap<TeacherStatisticsDTO, TeacherStatisticsDto>();
        
        // Маппинг для статистики
        CreateMap<StudentStatisticsDTO, StudentStatisticsDto>();
        
        // Маппинг для запросов и связей
        CreateMap<TeacherStudentDTO, TeacherStudentDto>();
        CreateMap<TeacherInfoDTO, StudentTeacherDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));
        
        // Маппинг для UserDTO
        CreateMap<UserDTO, UserDto>()
            .ForMember(dest => dest.ContactInfo, opt => 
                opt.MapFrom(src => src.ContactInfo))
            .ForMember(dest => dest.IsTeacher, opt => 
                opt.MapFrom(src => false)) // По умолчанию не преподаватель
            .ForMember(dest => dest.Roles, opt => 
                opt.Ignore()); // Роли должны быть заполнены вручную
        
        // Маппинг для ReviewDTO
        CreateMap<ReviewDTO, ReviewDto>();
    }
} 