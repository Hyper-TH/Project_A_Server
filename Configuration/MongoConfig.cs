using MongoDB.Driver;
using Microsoft.Extensions.Options;
using Project_A_Server.Models.DatabaseSettings;
using Project_A_Server.Models.Meetings;
using Project_A_Server.Models;
using Project_A_Server.Models.Availabilities;

namespace Project_A_Server.Configuration;

public static class MongoConfig
{
    public static void AddMongoConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IMongoClient>(sp =>
        {
            var dbSettings = configuration.GetSection("ProjectADatabase").Get<DBSettings>();
            if (dbSettings == null || string.IsNullOrEmpty(dbSettings.ConnectionString))
            {
                throw new Exception("ProjectA Database ConnectionString is not configured.");
            }

            return new MongoClient(dbSettings.ConnectionString);
        });

        services.Configure<DBSettings>(configuration.GetSection("ProjectADatabase"));
    }

    public static void AddMongoCollections(this IServiceCollection services)
    {
        services.AddScoped(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<DBSettings>>().Value;
            var client = sp.GetRequiredService<IMongoClient>();
            var database = client.GetDatabase(settings.DatabaseName);
            return database.GetCollection<User>(settings.UsersCollectionName);
        });

        services.AddScoped(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<DBSettings>>().Value;
            var client = sp.GetRequiredService<IMongoClient>();
            var database = client.GetDatabase(settings.DatabaseName);
            return database.GetCollection<Meeting>(settings.MeetingsCollectionName);
        });

        services.AddScoped(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<DBSettings>>().Value;
            var client = sp.GetRequiredService<IMongoClient>();
            var database = client.GetDatabase(settings.DatabaseName);
            return database.GetCollection<Attendees>(settings.AttendeesCollectionName);
        });

        services.AddScoped(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<DBSettings>>().Value;
            var client = sp.GetRequiredService<IMongoClient>();
            var database = client.GetDatabase(settings.DatabaseName);
            return database.GetCollection<UserMeetings>(settings.UserMeetingsCollectionName);
        });

        services.AddScoped(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<DBSettings>>().Value;
            var client = sp.GetRequiredService<IMongoClient>();
            var database = client.GetDatabase(settings.DatabaseName);
            return database.GetCollection<Availability>(settings.AvailabilitiesCollectionName);
        });

        services.AddScoped(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<DBSettings>>().Value;
            var client = sp.GetRequiredService<IMongoClient>();
            var database = client.GetDatabase(settings.DatabaseName);
            return database.GetCollection<UserAvailabilities>(settings.UserAvailabilitiesCollectionName);
        });

        services.AddScoped(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<DBSettings>>().Value;
            var client = sp.GetRequiredService<IMongoClient>();
            var database = client.GetDatabase(settings.DatabaseName);
            return database.GetCollection<UserGroups>(settings.UserGroupsCollectionName);
        });

        services.AddScoped(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<DBSettings>>().Value;
            var client = sp.GetRequiredService<IMongoClient>();
            var database = client.GetDatabase(settings.DatabaseName);
            return database.GetCollection<Group>(settings.GroupsCollectionName);
        });

        services.AddScoped(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<DBSettings>>().Value;
            var client = sp.GetRequiredService<IMongoClient>();
            var database = client.GetDatabase(settings.DatabaseName);
            return database.GetCollection<GroupAvailabilities>(settings.GroupAvailabilitiesCollectionName);
        });
    }
}
