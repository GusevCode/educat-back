using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Vibetech.Educat.DataAccess;
using Vibetech.Educat.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace Vibetech.Educat.Services.Services
{
    /// <summary>
    /// Базовый сервис с общими методами для всех сервисов
    /// </summary>
    public abstract class BaseService
    {
        protected readonly EducatDbContext _context;
        protected readonly ILogger _logger;

        protected BaseService(EducatDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Автоматически обновляет статусы уроков на "Completed", если время окончания урока уже прошло
        /// </summary>
        protected async Task<IEnumerable<Lesson>> UpdateLessonsStatusAsync(IEnumerable<Lesson> lessons)
        {
            var currentTime = DateTime.UtcNow;
            var lessonsToUpdate = lessons
                .Where(l => l.Status == LessonStatus.Scheduled && l.EndTime < currentTime)
                .ToList();
                
            if (lessonsToUpdate.Any())
            {
                _logger.LogInformation("Обновление статусов для {Count} завершившихся уроков", lessonsToUpdate.Count);
                
                foreach (var lesson in lessonsToUpdate)
                {
                    lesson.Status = LessonStatus.Completed;
                    _logger.LogInformation("Урок с ID={LessonId} автоматически помечен как завершенный", lesson.Id);
                }
                
                await _context.SaveChangesAsync();
            }
            
            return lessons;
        }

        /// <summary>
        /// Автоматически обновляет статус одного урока на "Completed", если время окончания урока уже прошло
        /// </summary>
        /// <param name="lessonId">Идентификатор урока</param>
        /// <returns>Урок с обновленным статусом или null, если урок не найден</returns>
        protected async Task<Lesson> UpdateLessonStatusAsync(int lessonId)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Teacher)
                .Include(l => l.Student)
                .Include(l => l.Subject)
                .FirstOrDefaultAsync(l => l.Id == lessonId);
                
            if (lesson == null)
            {
                _logger.LogWarning("Урок с ID={LessonId} не найден", lessonId);
                return null;
            }
            
            var currentTime = DateTime.UtcNow;
            if (lesson.Status == LessonStatus.Scheduled && lesson.EndTime < currentTime)
            {
                _logger.LogInformation("Автоматическое обновление статуса урока с ID={LessonId} на Completed, так как время окончания {EndTime} уже прошло", 
                    lessonId, lesson.EndTime);
                    
                lesson.Status = LessonStatus.Completed;
                await _context.SaveChangesAsync();
            }
            
            return lesson;
        }

        protected string GetActualLessonStatus(Lesson lesson)
        {
            // Если статус уже Completed или Cancelled, оставляем его
            if (lesson.Status == LessonStatus.Completed || lesson.Status == LessonStatus.Cancelled)
                return lesson.Status.ToString();
            
            // Если время окончания урока уже прошло, возвращаем Completed
            // Используем UTC время вместо локального
            if (lesson.EndTime < DateTime.UtcNow && lesson.Status == LessonStatus.Scheduled)
                return LessonStatus.Completed.ToString();
            
            return lesson.Status.ToString();
        }
        
        /// <summary>
        /// Обновляет статусы всех уроков в базе данных
        /// </summary>
        protected async Task UpdateAllLessonsStatusAsync()
        {
            // Находим все запланированные уроки, которые уже должны быть завершены
            var now = DateTime.UtcNow;
            var lessonsToUpdate = await _context.Lessons
                .Where(l => l.Status == LessonStatus.Scheduled && l.EndTime < now)
                .ToListAsync();
                
            if (lessonsToUpdate.Any())
            {
                // Обновляем статусы
                foreach (var lesson in lessonsToUpdate)
                {
                    lesson.Status = LessonStatus.Completed;
                }
                
                // Сохраняем изменения
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Автоматически обновлены статусы {Count} уроков", lessonsToUpdate.Count);
            }
        }
    }
} 