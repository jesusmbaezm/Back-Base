using Controllers.Extensions;
using Controllers.Middleware;
using Data.Contexts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Services.Mappings;
using Services.Settings;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Connection");


builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

// HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// DbContext con interceptor
builder.Services.AddDbContext<AppDbContext>((provider, options) =>
{
    options.UseSqlServer(connectionString);
});

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings!.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<ParameterProfile>();
    cfg.AddProfile<RoleProfile>();
    cfg.AddProfile<UserProfile>();
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter the JWT token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

builder.Services.AddPermissionAuthorization();
builder.Services.AddRepositories();
builder.Services.AddApplicationServices();

var app = builder.Build();

// 1. Swagger
app.UseSwagger();
app.UseSwaggerUI();

// 2. Middlewares custom
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<ForbiddenResponseMiddleware>();

// 3. Archivos estáticos (Angular) — antes del routing
app.UseDefaultFiles();
app.UseStaticFiles();

// 4. Pipeline en orden correcto
app.UseHttpsRedirection();
app.UseRouting();          // ← primero
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();    // ← entre UseRouting y MapControllers

// 5. Endpoints
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();