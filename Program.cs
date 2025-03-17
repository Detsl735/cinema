using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File("logs/app-log.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7) 
    .CreateLogger();




// PostgreSQL
builder.Services.AddSingleton<DatabaseHelper>();

// Репозитории и сервисы
builder.Services.AddScoped<MovieRepository>();
builder.Services.AddScoped<MovieService>();

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<UserService>();

builder.Services.AddScoped<ReviewRepository>();
builder.Services.AddScoped<ReviewService>();

builder.Services.AddScoped<WatchHistoryRepository>();
builder.Services.AddScoped<WatchHistoryService>();

builder.Services.AddScoped<FileRepository>();
builder.Services.AddScoped<FileService>();
builder.Services.AddScoped<FFmpegService>();


// MinIO
builder.Services.Configure<MinioSettings>(builder.Configuration.GetSection("Minio"));
builder.Services.AddSingleton<MinioService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "OnlineCinema API", Version = "v1" });

    c.OperationFilter<SwaggerFileOperationFilter>();
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Host.UseSerilog();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.MapControllers();
app.Run();
