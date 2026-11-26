using CinemaBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using CinemaBooking.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using CinemaBooking.Api.Auth;
using CinemaBooking.Application.Auth;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using CinemaBooking.Api.Errors;
using CinemaBooking.Api.Filters;
using CinemaBooking.Application.Genres;
using CinemaBooking.Application.Movies;
using CinemaBooking.Infrastructure.Services;
using FluentValidation;
using CinemaBooking.Application.Cinemas;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Đăng ký ASP.NET Core Identity cho Web API
builder.Services.AddIdentityCore<User>(options =>
    {
        // Mỗi email chỉ được sử dụng cho một tài khoản
        options.User.RequireUniqueEmail = true;

        // Mật khẩu tối thiểu 8 ký tự
        options.Password.RequiredLength = 8;

        // Giữ các quy tắc mật khẩu mặc định khác của Identity
    })
    .AddRoles<IdentityRole<int>>()
    .AddEntityFrameworkStores<AppDbContext>();

// Add services to the container.

builder.Services.AddControllers(options =>
{
    // Kiểm tra dữ liệu trước khi chạy Controller
    options.Filters.Add<FluentValidationFilter>();
})
.AddJsonOptions(options =>
{
    // Chuyển enum AgeRating thành chuỗi như T13, T16
    options.JsonSerializerOptions.Converters.Add(
        new JsonStringEnumConverter());
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Đọc cấu hình JWT từ appsettings và User Secrets
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

// Đăng ký service xử lý đăng ký, đăng nhập
builder.Services.AddScoped<IAuthService, AuthService>();

// Đăng ký service tạo JWT
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

// Đăng ký các Validator trong Application
builder.Services.AddValidatorsFromAssemblyContaining<
    UpsertMovieRequestValidator>();

// Đăng ký bộ xử lý lỗi toàn cục
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Đăng ký Service quản lý thể loại và phim
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<ICinemaService, CinemaService>();
// Lấy cấu hình JWT
var jwt = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>() ?? new JwtOptions();

// Kiểm tra khóa bí mật trước khi ứng dụng chạy
if (string.IsNullOrWhiteSpace(jwt.Key) ||
    Encoding.UTF8.GetByteCount(jwt.Key) < 32)
{
    throw new InvalidOperationException(
        "Thiếu Jwt:Key hoặc khóa JWT quá ngắn.");
}

// Cấu hình JWT Authentication
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Giữ nguyên tên claim: sub, role, name
        options.MapInboundClaims = false;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwt.Issuer,

                ValidateAudience = true,
                ValidAudience = jwt.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwt.Key)),

                ValidateLifetime = true,

                ClockSkew = TimeSpan.FromMinutes(1),

                NameClaimType = "name",
                RoleClaimType = "role"
            };
    });

// Đăng ký dịch vụ phân quyền
builder.Services.AddAuthorization();
// CORS cho ứng dụng React
const string CorsPolicy = "Frontend";

// Lấy danh sách Origin được phép từ appsettings.json
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Bắt và xử lý exception từ những middleware phía sau
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Cho phép frontend gọi API
app.UseCors(CorsPolicy);

// Xác thực JWT
app.UseAuthentication();

// Kiểm tra quyền
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

    // Khởi tạo vai trò Admin, Customer và tài khoản Admin mẫu
    await IdentitySeeder.SeedAsync(
        scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole<int>>>(),

        scope.ServiceProvider
            .GetRequiredService<UserManager<User>>(),

        "admin@cinema.local",

        builder.Configuration["Seed:AdminPassword"]
    );
}

app.Run();
