using Chibest.API.Extensions.CustomKebabCase;
using Chibest.Repository;
using Chibest.Repository.Models;
using Chibest.Service.Interface;
using Chibest.Service.ModelDTOs.StrongTypedModels;
using Chibest.Service.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using System.Text;
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
        ConfigKebabCase(services);
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<ISystemLogService, SystemLogService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductDetailService, ProductDetailService>();
        services.AddScoped<IBranchStockService, BranchStockService>();
        services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
        services.AddScoped<ITransferOrderService, TransferOrderService>();
        services.AddScoped<IPurchaseReturnService, PurchaseReturnService>();
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
                        context.Response.Headers["Token-Expired"] = "true";
                    }
                    return Task.CompletedTask;
                }
            };
        });

        // Config Bearer Auth in swagger
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ChiBest API",
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

    private static void ConfigKebabCase(IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Conventions.Add(new RouteTokenTransformerConvention(new KebabParameterTransformer()));
        }).AddNewtonsoftJson(options =>
        {//If using NewtonSoft in project then must orride default Naming rule of System.text
            options.SerializerSettings.ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new KebabCaseNamingStrategy()
            };
        });

        //Config Swagger to use KebabCase
        services.AddSwaggerGen(c => { c.SchemaFilter<KebabSwaggerSchema>(); });
    }
}