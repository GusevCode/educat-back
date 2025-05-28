using Microsoft.EntityFrameworkCore;
using Vibetech.Educat.DataAccess.Models;

namespace Vibetech.Educat.DataAccess;

public static class DbInitializer
{
    public static async Task SeedDataAsync(EducatDbContext context)
    {
        // Инициализация предметов
        var subjects = new List<Subject>
        {
            new Subject { Name = "Русский язык" },
            new Subject { Name = "Математика" },
            new Subject { Name = "Литература" },
            new Subject { Name = "Физика" },
            new Subject { Name = "Химия" },
            new Subject { Name = "Биология" },
            new Subject { Name = "География" },
            new Subject { Name = "Обществознание" },
            new Subject { Name = "История" },
            new Subject { Name = "Информатика" },
            new Subject { Name = "Английский язык" }
        };

        // Проверяем, существуют ли уже предметы, если нет - добавляем
        foreach (var subject in subjects)
        {
            var existingSubject = await context.Subjects
                .FirstOrDefaultAsync(s => s.Name == subject.Name);
                
            if (existingSubject == null)
            {
                await context.Subjects.AddAsync(subject);
            }
        }
        
        // Инициализация программ подготовки
        var preparationPrograms = new List<PreparationProgram>
        {
            new PreparationProgram { Name = "Подготовка к ОГЭ", Description = "Комплексная подготовка к Основному Государственному Экзамену" },
            new PreparationProgram { Name = "Подготовка к ЕГЭ", Description = "Комплексная подготовка к Единому Государственному Экзамену" },
            new PreparationProgram { Name = "Повышение успеваемости", Description = "Занятия для улучшения общей успеваемости по предмету" },
            new PreparationProgram { Name = "Подготовка к контрольной", Description = "Подготовка к контрольным работам и тестированиям" },
            new PreparationProgram { Name = "Помощь с домашним заданием", Description = "Помощь в выполнении и разборе домашних заданий" }
        };

        // Проверяем, существуют ли уже программы подготовки, если нет - добавляем
        foreach (var program in preparationPrograms)
        {
            var existingProgram = await context.PreparationPrograms
                .FirstOrDefaultAsync(p => p.Name == program.Name && p.TeacherProfileId == null);
                
            if (existingProgram == null)
            {
                program.TeacherProfileId = null; // Устанавливаем null для шаблонных программ
                await context.PreparationPrograms.AddAsync(program);
            }
        }

        // Сохраняем изменения
        await context.SaveChangesAsync();
    }
} 