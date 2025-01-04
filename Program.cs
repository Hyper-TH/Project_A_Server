using Project_A_Server.Services.Redis;
using Project_A_Server.Services;
using Project_A_Server.Services.MongoDB.Meetings;
using Project_A_Server.Interfaces;
using Project_A_Server.Repositories;
using Project_A_Server.Services.MongoDB.Availabilities;
using Project_A_Server.Configuration;
using Project_A_Server.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRedisConfiguration(builder.Configuration);
builder.Services.AddMongoConfiguration(builder.Configuration);
builder.Services.AddMongoCollections();
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISessionService, SessionService>();

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
builder.Services.AddScoped<RemoveGroup>();
builder.Services.AddScoped<UserAvailabilitiesService>();
builder.Services.AddScoped<UserGroupsService>();
builder.Services.AddScoped<AvailabilitiesService>();
builder.Services.AddScoped<GroupsService>();
builder.Services.AddScoped<GroupAvailabilitiesService>();

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
        policy.WithOrigins("http://localhost:5174")
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