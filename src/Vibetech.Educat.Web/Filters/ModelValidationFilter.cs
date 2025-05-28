using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using Vibetech.Educat.Web.Middleware;

namespace Vibetech.Educat.Web.Filters
{
    /// <summary>
    /// Фильтр действий для локализации сообщений об ошибках валидации моделей
    /// </summary>
    public class ModelValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = new Dictionary<string, string[]>();
                
                foreach (var key in context.ModelState.Keys)
                {
                    if (context.ModelState[key] != null && context.ModelState[key]!.Errors.Count > 0)
                    {
                        var errorMessages = context.ModelState[key]!.Errors
                            .Select(e => LocalizeValidationErrorMessage(key, e.ErrorMessage))
                            .ToArray();
                            
                        if (errorMessages.Any())
                        {
                            errors[key] = errorMessages;
                        }
                    }
                }
                
                // Создаем и выбрасываем специальное исключение для обработки в ErrorHandlingMiddleware
                throw new ValidationException("Ошибка валидации", errors);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Метод не используется, но должен быть реализован из интерфейса
        }
        
        private string LocalizeValidationErrorMessage(string propertyName, string errorMessage)
        {
            // Переводим стандартные сообщения валидации на русский
            if (errorMessage.Contains("field is required"))
                return $"Поле '{propertyName}' обязательно для заполнения";
                
            if (errorMessage.Contains("must be a number"))
                return $"Поле '{propertyName}' должно быть числом";
                
            if (errorMessage.Contains("must be a date"))
                return $"Поле '{propertyName}' должно содержать корректную дату";
                
            if (errorMessage.Contains("maximum length") || errorMessage.Contains("The field {0} must be a string with a maximum length of {1}"))
                return $"Поле '{propertyName}' превышает максимально допустимую длину";
                
            if (errorMessage.Contains("minimum length") || errorMessage.Contains("The field {0} must be a string with a minimum length of {1}"))
                return $"Поле '{propertyName}' меньше минимально допустимой длины";
                
            if (errorMessage.Contains("matching the required pattern"))
                return $"Поле '{propertyName}' не соответствует требуемому формату";
                
            if (errorMessage.Contains("The field {0} must match the field {1}") || errorMessage.Contains("The password and confirmation password do not match"))
                return "Пароль и подтверждение пароля не совпадают";
                
            if (errorMessage.Contains("The field {0} must be between {1} and {2}"))
                return $"Значение поля '{propertyName}' должно быть в допустимом диапазоне";
                
            if (errorMessage.Contains("The {0} field is not a valid e-mail address"))
                return "Введите корректный email-адрес";
                
            // Возвращаем исходное сообщение, если не нашли подходящего перевода
            return errorMessage;
        }
    }
} 