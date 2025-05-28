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
        /// или на "InProgress", если урок идет в данный момент
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
            bool updated = false;
            
            // Если время окончания урока уже прошло, меняем статус на Completed
            if (lesson.Status == LessonStatus.Scheduled && lesson.EndTime < currentTime)
            {
                _logger.LogInformation("Автоматическое обновление статуса урока с ID={LessonId} на Completed, так как время окончания {EndTime} уже прошло", 
                    lessonId, lesson.EndTime);
                    
                lesson.Status = LessonStatus.Completed;
                updated = true;
            }
            // Если текущее время между началом и окончанием урока, меняем статус на InProgress
            else if (lesson.Status == LessonStatus.Scheduled && lesson.StartTime <= currentTime && lesson.EndTime > currentTime)
            {
                _logger.LogInformation("Автоматическое обновление статуса урока с ID={LessonId} на InProgress, так как урок сейчас идет", 
                    lessonId);
                    
                lesson.Status = LessonStatus.InProgress;
                updated = true;
            }
            
            if (updated)
            {
                await _context.SaveChangesAsync();
            }
            
            return lesson;
        }

        /// <summary>
        /// Обновляет статусы всех уроков в базе данных
        /// </summary>
        protected async Task UpdateAllLessonsStatusAsync()
        {
            var now = DateTime.UtcNow;
            
            // Находим все запланированные уроки, которые уже должны быть завершены
            var completedLessons = await _context.Lessons
                .Where(l => l.Status == LessonStatus.Scheduled && l.EndTime < now)
                .ToListAsync();
                
            // Находим все запланированные уроки, которые должны быть в процессе
            var inProgressLessons = await _context.Lessons
                .Where(l => l.Status == LessonStatus.Scheduled && l.StartTime <= now && l.EndTime > now)
                .ToListAsync();
                
            bool hasChanges = false;
                
            if (completedLessons.Any())
            {
                // Обновляем статусы на Completed
                foreach (var lesson in completedLessons)
                {
                    lesson.Status = LessonStatus.Completed;
                }
                
                hasChanges = true;
                _logger.LogInformation("Автоматически обновлены статусы {Count} уроков на Completed", completedLessons.Count);
            }
            
            if (inProgressLessons.Any())
            {
                // Обновляем статусы на InProgress
                foreach (var lesson in inProgressLessons)
                {
                    lesson.Status = LessonStatus.InProgress;
                }
                
                hasChanges = true;
                _logger.LogInformation("Автоматически обновлены статусы {Count} уроков на InProgress", inProgressLessons.Count);
            }
            
            // Сохраняем изменения, если они есть
            if (hasChanges)
            {
                await _context.SaveChangesAsync();
            }
        }
    }
} 