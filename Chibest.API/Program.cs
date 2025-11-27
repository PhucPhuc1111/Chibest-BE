using Chibest.API.Extensions;
using Chibest.API.Middleware;
using Chibest.Repository.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

const long DefaultUploadLimitBytes = 1L * 1024 * 1024 * 1024; // 1 GB
var maxUploadBytes = builder.Configuration.GetValue<long?>("UploadLimits:MaxRequestBodySizeBytes") ?? DefaultUploadLimitBytes;

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = maxUploadBytes;
});

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = maxUploadBytes;
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = maxUploadBytes;
    options.ValueLengthLimit = int.MaxValue;
});

DotNetEnv.Env.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));
builder.Configuration.AddJsonFile("excel-mappings.json", optional: true, reloadOnChange: true);

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// Swagger (UI sẽ bật mọi môi trường)
builder.Services.AddSwaggerGen();

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });

builder.Services.AddMemoryCache();

// Đăng ký DI, CORS, JWT, Swagger… (trong ServiceRegister đã tạo policy "FrontendCors")
ServiceRegister.RegisterServices(builder.Services, builder.Configuration);

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();

// Console notes
Console.WriteLine("For develop environment, ensure .env is in the same folder as Program.cs then restart app.");
Console.WriteLine("Make sure .env is at path: " + Directory.GetCurrentDirectory());

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ChiBestDbContext>();
    await dbContext.Database.MigrateAsync();
}

// Swagger trước để tiện debug
app.UseSwagger();
app.UseSwaggerUI();

// === CORS phải chạy SỚM, trước auth ===
// Dùng đúng tên policy đã cấu hình trong ServiceRegister: "FrontendCors"
app.UseCors("FrontendCors");

// (Tùy chọn) Cho OPTIONS pass nhanh để preflight không bị auth chặn
app.Use(async (context, next) =>
{
    if (HttpMethods.IsOptions(context.Request.Method))
    {
        context.Response.StatusCode = StatusCodes.Status200OK;
        return;
    }
    await next();
});

// Global exception middleware (đặt sau CORS để header CORS đã có sẵn)
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

// Auth phải sau CORS
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();