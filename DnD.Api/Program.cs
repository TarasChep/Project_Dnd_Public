using System.Text;
using DnD.Application.Interfaces;
using DnD.Application.Services;
using DnD.Application.Validators;
using DnD.Domain.Entities;
using DnD.Domain.Interfaces;
using DnD.Domain.Services;
using DnD.Infrastructure.Persistence;
using DnD.Infrastructure.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Formatters.Xml;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Suppress EF Core polling logs
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("[error] Connection string  'DefaultConnection' not found");
    throw new InvalidOperationException("Connection string 'DefaultConnection' is null or empty");
}

// 1. Налаштування бази даних PostgreSQL
// Програма шукає рядок "DefaultConnection" у файлі appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CharacterValidator>();

// Цей метод автоматично створить HttpClient і прокине його у твій сервіс
builder.Services.AddHttpClient<IDiscordNotificationService, DiscordNotificationService>();
builder.Services.AddHttpClient<IDiscordWebhookService, DiscordWebhookService>();
builder.Services.AddHttpClient<IOAuthService, OAuthService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICharacterRepository, CharacterRepository>();
builder.Services.AddScoped<ICharacterService, CharacterService>();
builder.Services.AddSingleton<ICombatAnalyticsEngine, CombatAnalyticsEngine>();
builder.Services.AddScoped<IEncounterAnalyticsAdapter, EncounterAnalyticsAdapter>();
builder.Services.AddScoped<IEncounterAnalyzerService, EncounterAnalyzerService>();
builder.Services.AddScoped<IEncounterRepository, EncounterRepository>();
builder.Services.AddScoped<ICampaignRepository, CampaignRepository>();
builder.Services.AddScoped<ICampaignService, CampaignService>();
builder.Services.AddScoped<ICampaignEncounterService, CampaignEncounterService>();
builder.Services.AddScoped<ICombatService, CombatService>();

// 2. Додаємо підтримку контролерів (вони нам знадобляться для API)
builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        );
    });

// 3. Налаштування документації OpenAPI (Swagger/Scalar)
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer(
        (document, context, cancellationToken) =>
        {
            document.Info.Title = "DnD API";
            var scheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Name = "Authorization",
                In = ParameterLocation.Header,
                Scheme = "Bearer",
                BearerFormat = "Jwt",
                Description = "Enter your Jwt token",
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes.Add("Bearer", scheme);
            document.SecurityRequirements.Add(
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            },
                        },
                        Array.Empty<string>()
                    },
                }
            );
            return Task.CompletedTask;
        }
    );
});

builder
    .Services.AddIdentityCore<User>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;

        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"] ?? "fallback_secret_key_32_chars_long"
                )
            ),
        };
    });
builder.Services.AddAuthorization();

// Реєструємо політику CORS, щоб дозволити фронтенду звертатися до API
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowFrontend",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:3000", "http://localhost:5173") // Дозволяємо обидва порти Vite
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials(); // Важливо, якщо в майбутньому будуть куки
        }
    );
});

var app = builder.Build();

try
{
    Console.WriteLine("[Info] Connecting to Db");
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (context.Database.CanConnect())
        {
            System.Console.WriteLine("[SUCCESS] Connection to Db Esteblished successfully");
        }
        else
        {
            System.Console.WriteLine("[WARNING] Could not  connect to Db ");
        }
    }
}
catch (PostgresException ex)
{
    System.Console.WriteLine($"[DB ERROR] Db specific Error: {ex.Message}");
}
catch (Exception ex)
{
    System.Console.WriteLine($"[CRITICAL ERROR] Failed to initialize Web API: {ex.Message}");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
System.Console.WriteLine("[INFO] Web Api is starting ...");
app.Run();
