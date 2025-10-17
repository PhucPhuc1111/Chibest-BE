using Chibest.Service.ModelDTOs.StrongTypedModels;
using Chibest.Repository;
using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Chibest.Service.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Chibest.API.Extensions;

public static class ServiceRegister
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

        services.AddDbContext<ChiBestDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
            options.EnableSensitiveDataLogging();
        });

        services.AddAuthorizeService(configuration);
        AddCorsToThisWeb(services);
        AddEnum(services);
        AddKebab(services);
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<IAccountService, AccountService>();
    }

    public static IServiceCollection AddAuthorizeService(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOps = new JwtSettings
        {
            Key = configuration["Jwt_Key"] ?? Environment.GetEnvironmentVariable("Jwt_Key") ?? string.Empty,
            Issuer = configuration["Jwt_Issuer"] ?? Environment.GetEnvironmentVariable("Jwt_Issuer") ?? string.Empty,
            Audience = configuration["Jwt_Audience"] ?? Environment.GetEnvironmentVariable("Jwt_Audience") ?? string.Empty,
            AccessTokenExpirationMinutes = int.TryParse(configuration["Jwt_AccessTokenExpirationMinutes"] ?? Environment.GetEnvironmentVariable("Jwt_AccessTokenExpirationMinutes"), out var m) ? m : 15,
            RefreshTokenExpirationDays = int.TryParse(configuration["Jwt_RefreshTokenExpirationDays"] ?? Environment.GetEnvironmentVariable("Jwt_RefreshTokenExpirationDays"), out var d) ? d : 7
        };

        //Register JwtSettings as a singleton
        services.AddSingleton(jwtOps);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOps.Issuer,
                ValidAudience = jwtOps.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOps.Key)),
                ClockSkew = TimeSpan.Zero // Không cho phép độ trễ
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers.Add("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                }
            };
        });

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ChiBest_API",
                Version = "v1",
                Description = "API for managing ChiBest app",
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "JWT Authentication",
                Description = "Enter your JWT token in this field",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
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

        });

        return services;
    }

    private static void AddCorsToThisWeb(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });
    }

    private static void AddEnum(IServiceCollection services)
    {
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
    }

    private static void AddKebab(IServiceCollection services)
    {
        services.AddControllers(opts =>
                opts.Conventions.Add(new RouteTokenTransformerConvention(new ToKebabParameterTransformer())))

                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                });
    }
}