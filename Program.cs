using Microsoft.AspNetCore.Authentication.JwtBearer;
using MongoDB.Driver;
using Project_A_Server.Services.Redis;
using Project_A_Server.Services;
using StackExchange.Redis;
using System.Text;
using Project_A_Server.Models.DatabaseSettings;
using Microsoft.Extensions.Options;
using Project_A_Server.Models;
using Project_A_Server.Services.MongoDB.Utils;
using Project_A_Server.Models.Meetings;
using Project_A_Server.Services.MongoDB.Meetings;
using Project_A_Server.Interfaces;
using Project_A_Server.Repositories;
using Project_A_Server.Models.Availabilities;
using Project_A_Server.Services.MongoDB.Availabilities;

var builder = WebApplication.CreateBuilder(args);

// -------------------
// Redis Configuration
// -------------------
var redisConfig = builder.Configuration.GetSection("Redis");
var redisUrl = redisConfig["Url"];
var redisToken = builder.Configuration["UPSTASH_REDIS_TOKEN"];

if (string.IsNullOrEmpty(redisUrl) || string.IsNullOrEmpty(redisToken))
{
    var missingValues = new List<string>();
    if (string.IsNullOrEmpty(redisUrl)) missingValues.Add("Redis URL");
    if (string.IsNullOrEmpty(redisToken)) missingValues.Add("Redis Token");

    throw new InvalidOperationException($"Missing configuration values: {string.Join(", ", missingValues)}");
}

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = new ConfigurationOptions
    {
        EndPoints = { redisUrl },
        Password = redisToken,   
        Ssl = true,              
        AbortOnConnectFail = false
    };

    return ConnectionMultiplexer.Connect(configuration);
}).AddSingleton<RedisService>();

// -----------------------------
// Register MongoDB Dependencies
// -----------------------------
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var dBSettings = builder.Configuration.GetSection("ProjectADatabase").Get<DBSettings>();
    if (dBSettings == null || string.IsNullOrEmpty(dBSettings.ConnectionString))
    {
        throw new Exception("ProjectA Database ConnectionString is not configured.");
    }

    return new MongoClient(dBSettings.ConnectionString);
});

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISessionService, SessionService>();

// -----------------
// JWT Authentication
// -----------------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY"); ;

        if (string.IsNullOrEmpty(secretKey))
        {
            throw new Exception("JWT SecretKey is not configured.");
        }

        if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
        {
            throw new Exception("JWT SecretKey must be at least 32 characters long.");
        }


        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

// -----------------------------
// Configure Database Settings
// -----------------------------
builder.Services.Configure<DBSettings>(
    builder.Configuration.GetSection("ProjectADatabase"));

// -----------------------------
// Register MongoDB Collections
// -----------------------------
// Users
builder.Services.AddScoped(sp =>
{
    var settings = sp.GetRequiredService<IOptions<DBSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    var database = client.GetDatabase(settings.DatabaseName);
    return database.GetCollection<User>(settings.UsersCollectionName);
});

// Meetings
builder.Services.AddScoped(sp =>
{
    var settings = sp.GetRequiredService<IOptions<DBSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    var database = client.GetDatabase(settings.DatabaseName);
    return database.GetCollection<Meeting>(settings.MeetingsCollectionName);
});

// Attendees
builder.Services.AddScoped(sp =>
{
    var settings = sp.GetRequiredService<IOptions<DBSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    var database = client.GetDatabase(settings.DatabaseName);
    return database.GetCollection<Attendees>(settings.AttendeesCollectionName);
});

// UserMeetings
builder.Services.AddScoped(sp =>
{
    var settings = sp.GetRequiredService<IOptions<DBSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    var database = client.GetDatabase(settings.DatabaseName);
    return database.GetCollection<UserMeetings>(settings.UserMeetingsCollectionName);
});

// Availabilities
builder.Services.AddScoped(sp =>
{
    var settings = sp.GetRequiredService<IOptions<DBSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    var database = client.GetDatabase(settings.DatabaseName);
    return database.GetCollection<Availability>(settings.AvailabilitiesCollectionName);
});

// UserAvailabilities
builder.Services.AddScoped(sp =>
{
    var settings = sp.GetRequiredService<IOptions<DBSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    var database = client.GetDatabase(settings.DatabaseName);
    return database.GetCollection<UserAvailabilities>(settings.UserAvailabilitiesCollectionName);
});

// Groups
builder.Services.AddScoped(sp =>
{
    var settings = sp.GetRequiredService<IOptions<DBSettings>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();
    var database = client.GetDatabase(settings.DatabaseName);
    return database.GetCollection<Group>(settings.GroupsCollectionName);
});



// -----------------------------
// Register Generic Repositories
// -----------------------------
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// -----------------
// Register Services
// -----------------
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<MeetingsService>();
builder.Services.AddScoped<AttendeesService>();
builder.Services.AddScoped<UserMeetingsService>();
builder.Services.AddScoped<UnregisterUsers>();
builder.Services.AddScoped<UserAvailabilitiesService>();
builder.Services.AddScoped<AvailabilitiesService>();
builder.Services.AddScoped<GroupsService>();

// ------------------
// Add CORS and Swagger
// ------------------
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins, policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// -----------------
// Configure Middleware
// -----------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(MyAllowSpecificOrigins);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();