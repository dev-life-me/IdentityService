using Api.Config;
using Api.ExceptionHandlers;
using Application.Interfaces;
using Application.Services;
using Infrastructure.Persistence.AppDbContext;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Services.Securities;
using Infrastructure.Services.Securities.Configs;
using Microsoft.EntityFrameworkCore;

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
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

RSAGenerator.GenerateKey();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseExceptionHandler();
app.UseHttpsRedirection();



app.MapControllers();


app.Run();

public partial class Program { }

