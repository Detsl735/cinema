using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File("logs/app-log.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7) 
    .CreateLogger();




// PostgreSQL
builder.Services.AddSingleton<DatabaseHelper>();

// –епозитории и сервисы
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

builder.Services.AddScoped<AuthService>();


// MinIO
builder.Services.Configure<MinioSettings>(builder.Configuration.GetSection("Minio"));
builder.Services.AddSingleton<MinioService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddAuthentication(options =>
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddAuthorization();

string corsPolicy = "FrontendPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:5174")   
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();                   
    });
});


builder.Services.AddSwaggerGen(c =>
{

    c.SwaggerDoc("v1", new OpenApiInfo { Title = "OnlineCinema API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "¬ведите токен в формате: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    

    c.OperationFilter<SwaggerFileOperationFilter>();
});

builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 2L * 1024 * 1024 * 1024; 
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Host.UseSerilog();

var app = builder.Build();

app.UseCors(corsPolicy);

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
