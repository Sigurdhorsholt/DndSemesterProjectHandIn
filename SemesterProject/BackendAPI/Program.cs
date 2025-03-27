using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using BackendAPI.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;





var builder = WebApplication.CreateBuilder(args);
// Add controller support with JSON serialization options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Preserves object references to avoid circular references in JSON
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
         // Optional: makes the JSON response more readable
        options.JsonSerializerOptions.WriteIndented = true;
    });


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Configure CORS to allow all origins, methods, and headers (open policy)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()  // Allows requests from any origin
            .AllowAnyMethod()  // Allows any HTTP method (GET, POST, PUT, etc.)
            .AllowAnyHeader(); // Allows any headers
    });
});

// Configure JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "yourdomain.com",
        ValidAudience = "yourdomain.com",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ThisIsASecretKeyWithAtLeast32Characters"))
    };
});

// Path to SQLite database file
var filePath = "C:\\Users\\sigur\\OneDrive\\Dokumenter\\GitHub\\DndSemesterProjectHandIn\\SemesterProject\\SqliteDB\\DataBase.sqlite";

if (!File.Exists(filePath))
{
    Console.WriteLine($"File not found: {filePath}");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite($"Data Source={filePath}"));



/*
 *
 * SETTING UP APP BELOW
 */

// Build the application

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");


app.UseHttpsRedirection();
// Enable authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();
// Map controllers to endpoints
app.MapControllers();

// Middleware to log incoming HTTP requests
app.Use(async (context, next) =>
{
    Console.WriteLine($"{context.Request.Method} {context.Request.Path}");
    await next.Invoke();
});






app.Run();
