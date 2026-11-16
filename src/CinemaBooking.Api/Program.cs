using CinemaBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Chỉ tạo dữ liệu mẫu trong môi trường Development
if (app.Environment.IsDevelopment())
{
    // Tạo một phạm vi sử dụng dịch vụ (Scope)
    using var scope = app.Services.CreateScope();

    // Lấy AppDbContext từ Dependency Injection
    var db = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    // Tạo dữ liệu mẫu
    await DbSeeder.SeedAsync(db);
}

app.Run();
