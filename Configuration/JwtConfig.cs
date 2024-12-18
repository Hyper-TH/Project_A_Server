using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Project_A_Server.Configuration;

public static class JwtConfig
{
    public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");

        if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
        {
            throw new Exception("JWT SecretKey must be configured and at least 32 characters long.");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });
    }
}
