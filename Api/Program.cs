using Api.Configs;
using Api.ExceptionHandlers;
using Application.Interfaces;
using Application.Services;
using Infrastructure.Persistence.AppDbContext;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Services.Securities;
using Infrastructure.Services.Securities.Configs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.Services.AddDbContext<IdentityServiceDbContext>(option =>
{
    DbOption dbOption = builder.Configuration.GetSection(DbOption.SectionName).Get<DbOption>() ?? new DbOption();
    option.UseNpgsql(dbOption.DbConnectionString);
});
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddSingleton<ITokenProvider, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddOptions<JwtOptions>().BindConfiguration("Jwt");

var jwtOpt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? throw new Exception("Jwt config missing");

using var rsa = RSA.Create();
var contentRootPath = builder.Environment.ContentRootPath;
jwtOpt.PrivateKeyPath = Path.GetFullPath(jwtOpt.PrivateKeyPath, contentRootPath);
rsa.ImportFromPem(File.ReadAllText(Path.GetFullPath(jwtOpt.PublicKeyPath, contentRootPath)));

var publicKey = new RsaSecurityKey(rsa.ExportParameters(false));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(
    opt =>
    {
       
        opt.MapInboundClaims = false;
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOpt.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtOpt.Audience,

            ValidateLifetime = true,
            RequireExpirationTime = true,

            RequireSignedTokens = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = publicKey,

            ValidAlgorithms = new[] {
                SecurityAlgorithms.RsaSha256
            },
            ClockSkew = TimeSpan.Zero,

            RoleClaimType = ClaimTypes.Role

        };
        opt.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine(
                    $"JWT Authentication Failed: {context.Exception}");

                return Task.CompletedTask;
            },

            OnChallenge = context =>
            {
                Console.WriteLine(
                    $"JWT Challenge: {context.Error} - {context.ErrorDescription}");

                return Task.CompletedTask;
            },

            OnTokenValidated = context =>
            {
                Console.WriteLine("JWT Validated Successfully!");

                return Task.CompletedTask;
            },

            OnForbidden = context =>
            {
                Console.WriteLine("JWT Authorization Forbidden!");

                return Task.CompletedTask;
            }
        };
    }
    );
builder.Services.AddSingleton<ITokenProvider, TokenService>();
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();


app.Run();

public partial class Program { }

