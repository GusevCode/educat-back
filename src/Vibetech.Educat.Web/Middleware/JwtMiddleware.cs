using Microsoft.Extensions.Primitives;

namespace Vibetech.Educat.Web.Middleware;

/// <summary>
/// Промежуточный слой для обработки JWT токенов без префикса Bearer
/// </summary>
public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtMiddleware> _logger;

    public JwtMiddleware(RequestDelegate next, ILogger<JwtMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"];
        
        if (!StringValues.IsNullOrEmpty(authHeader) && authHeader.Count > 0)
        {
            var authValue = authHeader.First();
            
            // Если заголовок Authorization существует, но не начинается с "Bearer " и похож на JWT
            if (!string.IsNullOrEmpty(authValue) && 
                !authValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) && 
                authValue.Count(c => c == '.') == 2)  // JWT содержит две точки
            {
                _logger.LogInformation("Обнаружен JWT без префикса Bearer, добавляем префикс");
                context.Request.Headers["Authorization"] = $"Bearer {authValue}";
            }
        }
        
        await _next(context);
    }
} 