using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Vibetech.Educat.Web.Utils;

/// <summary>
/// Фильтр для настройки формата дат в примерах Swagger
/// </summary>
public class SwaggerDateFilter : ISchemaFilter
{
    // Список свойств, которые должны отображаться с полной датой и временем
    private static readonly HashSet<string> DateTimePropertyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "StartTime", "EndTime", "LessonTime", "ScheduledTime", "AppointmentTime", 
        "CompletedTime", "CancelledTime", "CreatedTime", "UpdatedTime"
    };

    // Генерируем примеры дат для Swagger-документации
    private static readonly string DateExample = DateTime.UtcNow.ToString("yyyy-MM-dd");
    private static readonly string DateTimeExample = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
    private static readonly string FutureDateTimeExample = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-ddTHH:mm:ssZ");
    
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(DateTime) || context.Type == typeof(DateTime?))
        {
            // По умолчанию все DateTime должны иметь формат date-time в Swagger
            schema.Format = "date-time";
            schema.Example = new OpenApiString(DateTimeExample);
            schema.Type = "string";
            return;
        }
        
        // Обрабатываем все свойства типа DateTime в моделях
        var dateTimeProperties = context.Type.GetProperties()
            .Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?))
            .ToList();
            
        if (schema?.Properties == null || dateTimeProperties.Count == 0) 
            return;
            
        foreach (var property in dateTimeProperties)
        {
            var propertyName = GetJsonPropertyName(property);
            
            if (schema.Properties.ContainsKey(propertyName))
            {
                // Проверяем, нужно ли сохранять время для этого свойства
                if (IsDateTimeProperty(property.Name))
                {
                    schema.Properties[propertyName].Format = "date-time";
                    
                    // Для StartTime и EndTime показываем будущие даты
                    if (propertyName.Equals("startTime", StringComparison.OrdinalIgnoreCase) ||
                        propertyName.Equals("endTime", StringComparison.OrdinalIgnoreCase))
                    {
                        // EndTime на 2 часа позже StartTime
                        var exampleTime = propertyName.Equals("endTime", StringComparison.OrdinalIgnoreCase)
                            ? DateTime.UtcNow.AddDays(7).AddHours(2)
                            : DateTime.UtcNow.AddDays(7);
                            
                        schema.Properties[propertyName].Example = 
                            new OpenApiString(exampleTime.ToString("yyyy-MM-ddTHH:mm:ssZ"));
                    }
                    else
                    {
                        schema.Properties[propertyName].Example = new OpenApiString(DateTimeExample);
                    }
                }
                else
                {
                    schema.Properties[propertyName].Format = "date";
                    schema.Properties[propertyName].Example = new OpenApiString(DateExample);
                }
                schema.Properties[propertyName].Type = "string";
            }
        }
    }
    
    private string GetJsonPropertyName(PropertyInfo property)
    {
        // Получаем имя свойства с учетом атрибутов JsonPropertyName, если они есть
        var attribute = property.GetCustomAttribute<System.Text.Json.Serialization.JsonPropertyNameAttribute>();
        if (attribute != null)
            return attribute.Name;
            
        // Для стандартного camelCase форматирования
        var name = property.Name;
        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }
    
    private bool IsDateTimeProperty(string propertyName)
    {
        // Определяем, является ли свойство датой-временем (для уроков, расписаний)
        // или только датой (для дней рождения, дат создания и т.д.)
        return DateTimePropertyNames.Contains(propertyName);
    }
} 