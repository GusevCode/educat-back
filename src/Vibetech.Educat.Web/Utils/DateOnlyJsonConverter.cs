using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vibetech.Educat.Web.Utils;

/// <summary>
/// Конвертер JSON для преобразования DateTime в строку даты без времени
/// </summary>
public class DateOnlyJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // При чтении преобразуем в UTC
        var dateString = reader.GetString();
        if (DateTime.TryParse(dateString, out var date))
        {
            // Устанавливаем время 12:00 UTC для избежания проблем с часовыми поясами
            return new DateTime(date.Year, date.Month, date.Day, 12, 0, 0, DateTimeKind.Utc);
        }
        return DateTime.UtcNow;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Гарантируем, что дата имеет тип UTC
        if (value.Kind != DateTimeKind.Utc)
        {
            value = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
        
        // При записи форматируем только дату без времени
        writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
    }
}

/// <summary>
/// Конвертер JSON для даты-времени, сохраняющий информацию о времени
/// </summary>
public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateTimeString = reader.GetString();
        if (string.IsNullOrEmpty(dateTimeString))
            return DateTime.UtcNow;
            
        if (DateTime.TryParse(dateTimeString, out var dateTime))
        {
            // Всегда преобразуем в UTC, сохраняя время
            if (dateTime.Kind != DateTimeKind.Utc)
            {
                return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            }
            return dateTime;
        }
        return DateTime.UtcNow;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Проверяем, что дата в UTC
        if (value.Kind != DateTimeKind.Utc)
        {
            value = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
        
        // Сохраняем время в формате ISO 8601 с указанием Z (UTC)
        writer.WriteStringValue(value.ToString("o"));
    }
} 