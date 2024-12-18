using Project_A_Server.Services.Redis;
using StackExchange.Redis;

namespace Project_A_Server.Configuration;

public static class RedisConfig
{
    public static void AddRedisConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConfig = configuration.GetSection("Redis");
        var redisUrl = redisConfig["Url"];
        var redisToken = configuration["UPSTASH_REDIS_TOKEN"];

        if (string.IsNullOrEmpty(redisUrl) || string.IsNullOrEmpty(redisToken))
        {
            var missingValues = new List<string>();
            if (string.IsNullOrEmpty(redisUrl)) missingValues.Add("Redis URL");
            if (string.IsNullOrEmpty(redisToken)) missingValues.Add("Redis Token");

            throw new InvalidOperationException($"Missing configuration values: {string.Join(", ", missingValues)}");
        }

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configurationOptions = new ConfigurationOptions
            {
                EndPoints = { redisUrl },
                Password = redisToken,
                Ssl = true,
                AbortOnConnectFail = false
            };

            return ConnectionMultiplexer.Connect(configurationOptions);
        }).AddSingleton<RedisService>();
    }
}
