using System.Text;
using AuctionApi.Common;
using AuctionApi.Common.Middleware;
using AuctionApi.Data;
using AuctionApi.Data.Auth;
using AuctionApi.Data.Repositories;
using AuctionApi.Data.Storage;
using AuctionApi.Core.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuctionApi;

/// <summary>
/// Composition-root extensions. Each method owns one concern (persistence,
/// auth, swagger, etc.) so <see cref="Program"/> stays a thin wiring file.
/// </summary>
internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        // File storage rooted at <ContentRoot>/Uploads. Resolved at startup so
        // the directory exists before the first upload.
        services.AddSingleton<IFileStorage>(_ =>
            new LocalFileStorage(Path.Combine(AppContext.BaseDirectory, "Uploads")));

        return services;
    }

    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddScoped<Core.Services.AuthService>();
        services.AddScoped<Core.Services.AuctionService>();
        services.AddScoped<Core.Services.BidService>();
        services.AddScoped<Core.Services.AttachmentService>();
        services.AddScoped<Core.Services.AdminService>();
        return services;
    }

    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

        services.AddControllers(options =>
        {
            options.Filters.Add<DomainExceptionFilter>();
        });

        services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = 50 * 1024 * 1024;
        });

        return services;
    }

    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration config)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),
                    ValidateIssuer = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = config["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                };
            });

        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Auction API", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                Name = "Authorization",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
            });
            c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer",
                        },
                    },
                    Array.Empty<string>()
                },
            });
        });
        return services;
    }

    public static IServiceCollection AddCors(this IServiceCollection services, string[] allowedOrigins)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowClient", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });
        return services;
    }
}
