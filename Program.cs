using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Redis Config
var redisConfig = builder.Configuration.GetSection("Redis");
var redisUrl = redisConfig["Url"];
var redisToken = Environment.GetEnvironmentVariable("UPSTASH_REDIS_TOKEN");

if (string.IsNullOrEmpty(redisToken))
{
    throw new InvalidOperationException("Redis token not found in environment variables.");
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
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapGet("/redis-test", async (IConnectionMultiplexer redis) =>
{
    var db = redis.GetDatabase();

    // Set a value
    await db.StringSetAsync("foo", "bar");

    // Get the value
    var value = await db.StringGetAsync("foo");
    return Results.Ok(new { Key = "foo", Value = value.ToString() });
});


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
