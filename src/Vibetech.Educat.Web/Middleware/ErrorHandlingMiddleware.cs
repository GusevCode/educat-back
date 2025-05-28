using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Vibetech.Educat.Web.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Необработанная ошибка: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var statusCode = GetStatusCode(exception);
        context.Response.StatusCode = statusCode;

        // Проверяем, является ли исключение ошибкой валидации модели
        if (exception is ValidationException validationEx)
        {
            await HandleValidationExceptionAsync(context, validationEx);
            return;
        }
        
        var response = new 
        {
            success = false,
            message = GetUserFriendlyErrorMessage(exception),
            errorCode = GetErrorCode(exception),
            statusCode
        };
        
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }

    private static async Task HandleValidationExceptionAsync(HttpContext context, ValidationException validationEx)
    {
        var response = new 
        {
            success = false,
            message = "Ошибка валидации данных",
            errors = validationEx.Errors
        };
        
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
    
    private static string GetErrorCode(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException => "UNAUTHORIZED",
            ValidationException => "VALIDATION_ERROR",
            ArgumentException => "INVALID_ARGUMENT",
            InvalidOperationException => "INVALID_OPERATION",
            DbUpdateException => "DATABASE_ERROR",
            _ => "INTERNAL_ERROR"
        };
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            DbUpdateException => (int)HttpStatusCode.BadRequest,
            ValidationException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };
    }

    private static string GetUserFriendlyErrorMessage(Exception exception)
    {
        // Получаем исходное сообщение из исключения или внутреннего исключения
        string originalMessage = exception.Message;
        if (exception.InnerException != null)
        {
            originalMessage = exception.InnerException.Message;
        }
        
        // Перевод типичных ошибок Entity Framework
        if (originalMessage.Contains("Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone'"))
        {
            return "Ошибка формата даты. Пожалуйста, используйте формат даты YYYY-MM-DD для полей типа дата (например, 2024-05-15)";
        }
        
        if (originalMessage.Contains("is unknown when attempting to save changes") && originalMessage.Contains("foreign key"))
        {
            return "Ошибка при сохранении связей: указана ссылка на несуществующую запись";
        }
        
        if (originalMessage.Contains("duplicate key value violates unique constraint"))
        {
            if (originalMessage.Contains("email"))
            {
                return "Пользователь с таким email уже существует";
            }
            return "Запись с такими данными уже существует в системе";
        }
        
        if (originalMessage.Contains("The property") && originalMessage.Contains("is marked as required"))
        {
            return "Не заполнены обязательные поля";
        }
        
        if (originalMessage.Contains("Invalid login attempt"))
        {
            return "Неверный логин или пароль";
        }
        
        if (originalMessage.Contains("User already exists"))
        {
            return "Пользователь с таким логином уже существует";
        }
        
        if (originalMessage.Contains("value is outside the range"))
        {
            return "Указанное значение находится за пределами допустимого диапазона";
        }
        
        if (originalMessage.Contains("Invalid token") || originalMessage.Contains("token validation failed"))
        {
            return "Недействительный или просроченный токен авторизации";
        }
        
        if (exception is DbUpdateException)
        {
            return "Ошибка при сохранении данных в базу. Проверьте корректность введенных данных";
        }
        
        if (exception is UnauthorizedAccessException)
        {
            return "У вас нет прав для выполнения этого действия";
        }
        
        if (exception is ArgumentException || exception is InvalidOperationException)
        {
            return "Ошибка в параметрах запроса: " + originalMessage;
        }
        
        // Если это какая-то конкретная ошибка, которую мы не предусмотрели, оставляем оригинальное сообщение,
        // но префиксом указываем, что это ошибка
        return "Ошибка: " + originalMessage;
    }
}

/// <summary>
/// Специальное исключение для ошибок валидации
/// </summary>
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(string message, IDictionary<string, string[]> errors) 
        : base(message)
    {
        Errors = errors;
    }
} 