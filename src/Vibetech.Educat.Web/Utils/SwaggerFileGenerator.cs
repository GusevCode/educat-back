using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Writers;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Vibetech.Educat.Web.Utils
{
    public static class SwaggerFileGenerator
    {
        public static async Task GenerateSwaggerJsonAsync(IApplicationBuilder app, string outputPath = "swagger.json")
        {
            try
            {
                // Получаем сервис генерации Swagger документации
                using var serviceScope = app.ApplicationServices.CreateScope();
                var swaggerProvider = serviceScope.ServiceProvider.GetRequiredService<ISwaggerProvider>();
                
                // Получаем спецификацию Swagger
                var swagger = swaggerProvider.GetSwagger("v1");
                
                // Создаем файл для записи
                using var streamWriter = File.CreateText(outputPath);
                
                // Создаем JSON writer
                var jsonWriter = new OpenApiJsonWriter(streamWriter);
                
                // Записываем спецификацию Swagger в JSON файл
                swagger.SerializeAsV3(jsonWriter);
                
                await streamWriter.FlushAsync();
                
                // Выводим информацию об успешной генерации файла
                var fullPath = Path.GetFullPath(outputPath);
                Console.WriteLine($"Swagger JSON файл успешно создан: {fullPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании файла Swagger JSON: {ex.Message}");
            }
        }
    }
} 