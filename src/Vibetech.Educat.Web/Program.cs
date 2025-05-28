using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Vibetech.Educat.DataAccess;
using Vibetech.Educat.DataAccess.Models;
using Vibetech.Educat.Services.Services;
using Vibetech.Educat.Web.Middleware;
using Vibetech.Educat.Web.Utils;
using Vibetech.Educat.API.Mappers;

var builder = WebApplication.CreateBuilder(args);

// Добавляем DbContext
builder.Services.AddDbContext<EducatDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавляем AutoMapper
builder.Services.AddAutoMapper(typeof(ApiMappingProfile));

// Настраиваем Identity
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<EducatDbContext>()
.AddErrorDescriber<Vibetech.Educat.Web.Utils.RussianIdentityErrorDescriber>()
.AddDefaultTokenProviders();

// Настраиваем JWT аутентификацию
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    
    // Добавляем возможность принимать токены без префикса Bearer
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Если токен передан без префикса Bearer, добавляем его
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (authHeader != null && !authHeader.StartsWith("Bearer ") && 
                authHeader.Length > 10 && authHeader.Contains('.'))
            {
                context.Token = authHeader;
                context.Request.Headers["Authorization"] = $"Bearer {authHeader}";
            }
            return Task.CompletedTask;
        },
        // Добавляем обработку ошибок авторизации для улучшения диагностики
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(context.Exception, "Ошибка аутентификации: {Message}", context.Exception.Message);
            
            if (context.Exception is SecurityTokenExpiredException)
            {
                context.Response.Headers.Add("Token-Expired", "true");
                context.Response.Headers.Add("Access-Control-Expose-Headers", "Token-Expired");
            }
            
            return Task.CompletedTask;
        }
    };
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "DefaultDevKeyForTesting8d3a097e-09a2-495a-bd55"))
    };
});

// Добавляем сервисы
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TeacherService>();
builder.Services.AddScoped<StudentService>();
builder.Services.AddScoped<ReviewService>();

// Добавляем контроллеры
builder.Services.AddControllers(options =>
{
    // Добавляем фильтр для локализации сообщений об ошибках валидации
    options.Filters.Add<Vibetech.Educat.Web.Filters.ModelValidationFilter>();
})
    .AddApplicationPart(typeof(Vibetech.Educat.API.Controllers.AuthController).Assembly)
    .AddJsonOptions(options =>
    {
        // Настройка форматирования JSON
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never; // Всегда включать все свойства
        
        // Используем JsonConverterFactory для более гибкой настройки сериализации дат
        // DateTimeJsonConverter имеет приоритет над DateOnlyJsonConverter
        options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
    });

// Добавляем CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    
    // Production CORS policy
    options.AddPolicy("Production", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "https://localhost" };
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
    
    // Development CORS policy with credentials - максимально открытая политика
    options.AddPolicy("Development", policy =>
    {
        policy.SetIsOriginAllowed(_ => true) // Разрешаем все источники
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Content-Disposition", "Token-Expired", "Content-Length")
              .AllowCredentials();
    });
});

// Добавляем Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Vibetech Educat API",
        Version = "v1",
        Description = "API для платформы репетиторов и студентов",
        Contact = new OpenApiContact
        {
            Name = "Vibetech",
            Email = "support@vibetech.ru"
        }
    });

    // Добавляем поддержку JWT в Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Включаем аннотации Swagger
    c.EnableAnnotations();
    
    // Добавляем фильтр для форматирования дат в примерах
    c.SchemaFilter<SwaggerDateFilter>();

    // Настройка группировки контроллеров по тегам
    c.TagActionsBy(api => 
    {
        if (api.GroupName != null)
            return new[] { api.GroupName };

        if (api.ActionDescriptor.RouteValues.TryGetValue("controller", out var controller))
        {
            return new[] { controller };
        }

        return new[] { "Other" };
    });
});

var app = builder.Build();

// Используем собственный middleware для обработки ошибок
app.UseMiddleware<ErrorHandlingMiddleware>();

// Добавляем middleware для обработки JWT токенов без префикса Bearer
app.UseMiddleware<JwtMiddleware>();

// Настройка пайплайна HTTP-запросов
app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Vibetech Educat API v1");
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    c.DefaultModelsExpandDepth(-1); // Скрываем раздел моделей
});

app.UseHttpsRedirection();

// Разрешаем запросы от любых источников
app.UseCors("Development");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Инициализация ролей при запуске приложения
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    var roles = new[] { "STUDENT", "TEACHER" };
    
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<int>(role));
        }
    }
    
    // Инициализация предметов и программ подготовки
    var context = scope.ServiceProvider.GetRequiredService<EducatDbContext>();
    await DbInitializer.SeedDataAsync(context);
}

// Генерируем swagger.json файл при запуске приложения
await SwaggerFileGenerator.GenerateSwaggerJsonAsync(app, "swagger.json");

app.Run();
