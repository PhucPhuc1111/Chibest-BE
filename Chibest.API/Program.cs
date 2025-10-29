using Chibest.API.Extensions;
using Chibest.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load(Path.Combine(Directory.GetCurrentDirectory(),".env"));
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
 {
     options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
 });
builder.Services.AddMemoryCache();
ServiceRegister.RegisterServices(builder.Services, builder.Configuration);
builder.Services.AddAuthorization();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Config public hosting
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000);
});
Console.WriteLine("For develop environtment, make sure put file .env in this folder same as file program.cs. Then restart app to reload on start.");
Console.WriteLine("Make sure put file .env in this path ("+Directory.GetCurrentDirectory()+"). Then restart app to reload on start.");

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseCors("AllowAll");

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

//Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
