using System.Text;
using ChatApi.Data;
using ChatApi.Endpoints;
using ChatApi.Hubs;
using ChatApi.Models;
using ChatApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.FileProviders;

 DotNetEnv.Env.Load("../.env"); // если запускаешь локально, .env должен лежать выше
var builder = WebApplication.CreateBuilder(args);

// Определяем окружение и домен
var domain =  builder.Configuration["YOUR_DOMAIN"] ?? "http://localhost:4200";
var backendDomain =  builder.Configuration["YOUR_DOMAIN"] ?? "http://localhost:5000";

// 🔐 Чтение настроек JWT
var jwtSection = builder.Configuration.GetSection("JWTSetting");
jwtSection["ValidAudience"] = domain;
jwtSection["ValidIssuer"] = backendDomain;
var securityKey = jwtSection.GetValue<string>("SecurityKey");

if (string.IsNullOrEmpty(securityKey))
    throw new Exception("JWT SecurityKey is not set. Please set JWTSetting:SecurityKey in config or environment.");

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(domain)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddOpenApi();
builder.Services.AddSignalR();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<TokenService>();

// ✅ JWT Аутентификация
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey)),
        ValidateIssuer = false,
        ValidateAudience = false
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddIdentityCore<AppUser>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "uploads")),
            RequestPath = "/uploads"
        });

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Use(async (context, next) =>
{
    Console.WriteLine($"Request Path: {context.Request.Path}, Method: {context.Request.Method}");
    await next.Invoke();
});

app.MapHub<ChatHub>("hubs/chat");
app.MapHub<VideoChatHub>("hubs/video");
app.MapAccountEndpoint();

app.Run();
