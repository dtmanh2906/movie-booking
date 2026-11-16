using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Persistence;

public class AppDbContext
    : IdentityDbContext<User, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Cinema> Cinemas => Set<Cinema>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Showtime> Showtimes => Set<Showtime>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingDetail> BookingDetails
        => Set<BookingDetail>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void ConfigureConventions(
        ModelConfigurationBuilder builder)
    {
        base.ConfigureConventions(builder);

        // 1. Tiền VND không có phần thập phân
        builder.Properties<decimal>()
            .HavePrecision(12, 0);

        // 2. Chuẩn hóa thời gian UTC
        builder.Properties<DateTime>()
            .HaveConversion<UtcDateTimeConverter>();

        builder.Properties<DateTime?>()
            .HaveConversion<NullableUtcDateTimeConverter>();

        // 3. Lưu enum thành chuỗi
        builder.Properties<AgeRating>()
            .HaveConversion<string>()
            .HaveMaxLength(10);

        builder.Properties<SeatType>()
            .HaveConversion<string>()
            .HaveMaxLength(20);

        builder.Properties<BookingStatus>()
            .HaveConversion<string>()
            .HaveMaxLength(20);

        builder.Properties<PaymentStatus>()
            .HaveConversion<string>()
            .HaveMaxLength(20);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Bắt buộc để Identity cấu hình các bảng của nó.
        base.OnModelCreating(modelBuilder);
        // Cấu hình entity User
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(x => x.FullName)
                .HasMaxLength(100)
                .IsRequired();
        });
        // Cấu hình entity Genre.
        modelBuilder.Entity<Genre>(entity =>
        {
            entity.Property(x => x.Name)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(x => x.Name)
                .IsUnique();
        });
        // Cấu hình entity Movie
        modelBuilder.Entity<Movie>(entity =>
        {
            // 1. Giới hạn độ dài các thuộc tính
            entity.Property(x => x.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Slug)
                .HasMaxLength(220)
                .IsRequired();

            entity.Property(x => x.Director)
                .HasMaxLength(100);

            entity.Property(x => x.Actors)
                .HasMaxLength(500);

            entity.Property(x => x.MovieFormat)
                .HasMaxLength(50);

            entity.Property(x => x.Language)
                .HasMaxLength(50);

            entity.Property(x => x.BannerUrl)
                .HasMaxLength(255);

            entity.Property(x => x.PosterUrl)
                .HasMaxLength(255);

            entity.Property(x => x.TrailerUrl)
                .HasMaxLength(255);

            // 2. Mỗi phim có một Slug duy nhất
            entity.HasIndex(x => x.Slug)
                .IsUnique();

            // 3. Index hỗ trợ tìm kiếm phim
            entity.HasIndex(x => new
            {
                x.IsShowing,
                x.ReleaseDate
            });

            // 4. Quan hệ nhiều-nhiều Movie và Genre
            entity.HasMany(x => x.Genres)
            .WithMany(g => g.Movies)
            .UsingEntity<Dictionary<string, object>>(
             "GenreMovie",

                right => right.HasOne<Genre>()
                .WithMany()
                .HasForeignKey("GenresId")
                .OnDelete(DeleteBehavior.Restrict),

                left => left.HasOne<Movie>()
                 .WithMany()
                .HasForeignKey("MoviesId")
                .OnDelete(DeleteBehavior.Restrict)
    );
        });
        // Cấu hình entity Cinema
        modelBuilder.Entity<Cinema>(entity =>
        {
            entity.Property(x => x.Name)
                .HasMaxLength(100).IsRequired();

            entity.Property(x => x.Code)
                .HasMaxLength(20).IsRequired();

            entity.Property(x => x.Address)
                .HasMaxLength(255).IsRequired();

            entity.Property(x => x.District)
                .HasMaxLength(100).IsRequired();

            entity.Property(x => x.City)
                .HasMaxLength(100).IsRequired();

            entity.Property(x => x.Hotline)
                .HasMaxLength(20);

            entity.Property(x => x.Email)
                .HasMaxLength(100);

            entity.HasIndex(x => x.Code).IsUnique();

            entity.HasIndex(x => new
            {
                x.City,
                x.District
            });
        });
        // Cấu hình entity Room
        modelBuilder.Entity<Room>(entity =>
        {
            entity.Property(x => x.RoomNumber)
                .HasMaxLength(20).IsRequired();

            entity.HasIndex(x => new
            {
                x.CinemaId,
                x.RoomNumber
            }).IsUnique();
        });
        // Cấu hình entity Seat
        modelBuilder.Entity<Seat>(entity =>
        {
            entity.Property(x => x.SeatRow)
                .HasMaxLength(5).IsRequired();

            entity.HasIndex(x => new
            {
                x.RoomId,
                x.SeatRow,
                x.SeatNumber
            }).IsUnique();
        });
        // Cấu hình entity Showtime
        modelBuilder.Entity<Showtime>(entity =>
        {
            // Hỗ trợ tìm suất chiếu theo phim và thời gian
            entity.HasIndex(x => new
            {
                x.MovieId,
                x.StartTime
            });

            // Hỗ trợ truy vấn lịch chiếu của một phòng
            entity.HasIndex(x => new
            {
                x.RoomId,
                x.StartTime,
                x.EndTime
            });
        });
        // Cấu hình entity Booking
        modelBuilder.Entity<Booking>(entity =>
        {
            // 1. Mã đặt vé là duy nhất
            entity.Property(x => x.BookingCode)
                .HasMaxLength(20)
                .IsRequired();

            entity.HasIndex(x => x.BookingCode)
                .IsUnique();

            // 2. Phát hiện cập nhật đồng thời
            entity.Property(x => x.RowVersion)
                .IsRowVersion();

            // 3. Hỗ trợ tìm kiếm đơn hàng
            entity.HasIndex(x => new
            {
                x.UserId,
                x.Status
            });

            // Hỗ trợ tìm các đơn hết hạn
            entity.HasIndex(x => new
            {
                x.Status,
                x.ExpiresAt
            });

            // 4. Quan hệ Booking - User
            entity.HasOne(x => x.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(x => x.UserId);

            // 5. Quan hệ Booking - Showtime
            entity.HasOne(x => x.Showtime)
                .WithMany(s => s.Bookings)
                .HasForeignKey(x => x.ShowtimeId);
        });
        // Cấu hình BookingDetail
        modelBuilder.Entity<BookingDetail>(entity =>
        {
            // Chống đặt trùng ghế trong cùng một suất chiếu
            entity.HasIndex(x => new
            {
                x.ShowtimeId,
                x.SeatId
            })
            .IsUnique()
            .HasFilter("[IsActive] = 1")
            .HasDatabaseName(
                "UX_BookingDetails_Showtime_Seat_Active");

            // Quan hệ BookingDetail - Showtime
            entity.HasOne(x => x.Showtime)
                .WithMany()
                .HasForeignKey(x => x.ShowtimeId);

            // Quan hệ BookingDetail - Seat
            entity.HasOne(x => x.Seat)
                .WithMany(s => s.BookingDetails)
                .HasForeignKey(x => x.SeatId);
        });
        // Cấu hình entity Payment
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(x => x.PaymentMethod)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.TxnRef)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(x => x.TransactionId)
                .HasMaxLength(100);

            entity.Property(x => x.ResponseCode)
                .HasMaxLength(10);

            // Mỗi lần thanh toán có mã tham chiếu duy nhất
            entity.HasIndex(x => x.TxnRef)
                .IsUnique();

            // Hỗ trợ tìm các lần thanh toán theo đơn hàng
            entity.HasIndex(x => x.BookingId);
        });
        // Mặc định: các khóa ngoại nghiệp vụ dùng Restrict
        // để tránh xóa dây chuyền dữ liệu.
        var domainNamespace = typeof(Movie).Namespace;

        foreach (var fk in modelBuilder.Model.GetEntityTypes()
            .Where(t => t.ClrType.Namespace == domainNamespace)
            .SelectMany(t => t.GetForeignKeys()))
        {
            fk.DeleteBehavior = DeleteBehavior.Restrict;
        }

        // Ngoại lệ: Xóa Booking thì xóa BookingDetail liên quan.
        modelBuilder.Entity<BookingDetail>()
            .HasOne(x => x.Booking)
            .WithMany(x => x.BookingDetails)
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}