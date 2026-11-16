using CinemaBooking.Domain.Entities;
using CinemaBooking.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(
        AppDbContext db,
        CancellationToken cancellationToken = default)
    {
        // Danh sách thể loại phim mẫu
        string[] genreNames =
        [
            "Action",
            "Comedy",
            "Drama"
        ];

        foreach (var name in genreNames)
        {
            // Kiểm tra thể loại đã tồn tại chưa
            bool exists = await db.Genres.AnyAsync(
                g => g.Name == name,
                cancellationToken);

            if (!exists)
            {
                db.Genres.Add(new Genre
                {
                    Name = name
                });
            }
        }

        // Dữ liệu rạp phim mẫu (địa chỉ giả lập)
        var cinemas = new[]
        {
            new Cinema
            {
                Name = "MovieBooking Hà Đông",
                Code = "MB-HN01",
                Address = "Địa chỉ demo 01",
                District = "Hà Đông",
                City = "Hà Nội",
                IsActive = true
            },
            new Cinema
            {
                Name = "MovieBooking Cầu Giấy",
                Code = "MB-HN02",
                Address = "Địa chỉ demo 02",
                District = "Cầu Giấy",
                City = "Hà Nội",
                IsActive = true
            },
            new Cinema
            {
                Name = "MovieBooking Quận 1",
                Code = "MB-HCM01",
                Address = "Địa chỉ demo 03",
                District = "Quận 1",
                City = "TP. Hồ Chí Minh",
                IsActive = true
            }
        };

        // Không thêm rạp đã tồn tại
        foreach (var cinema in cinemas)
        {
            bool exists = await db.Cinemas.AnyAsync(
                c => c.Code == cinema.Code,
                cancellationToken);

            if (!exists)
            {
                db.Cinemas.Add(cinema);
            }
        }

        // Chỉ lưu những bản ghi mới được thêm
        await db.SaveChangesAsync(cancellationToken);

        // Lấy các rạp đã được lưu trong database
        var cinemaCodes = new[]
        {
            "MB-HN01",
            "MB-HN02",
            "MB-HCM01"
        };

        var savedCinemas = await db.Cinemas
            .Where(c => cinemaCodes.Contains(c.Code))
            .ToListAsync(cancellationToken);

        // Đảm bảo đủ 3 rạp trước khi tạo phòng
        if (savedCinemas.Count != 3)
        {
            throw new InvalidOperationException(
                "Chưa có đủ 3 rạp mẫu để tạo phòng chiếu.");
        }

        // Mỗi rạp có 2 phòng
        foreach (var cinema in savedCinemas)
        {
            for (int number = 1; number <= 2; number++)
            {
                string roomNumber = number.ToString("D2");

                // Mỗi rạp chỉ có một phòng mang số này
                bool exists = await db.Rooms.AnyAsync(
                    r => r.CinemaId == cinema.Id
                         && r.RoomNumber == roomNumber,
                    cancellationToken);

                if (!exists)
                {
                    db.Rooms.Add(new Room
                    {
                        CinemaId = cinema.Id,
                        RoomNumber = roomNumber,
                        RowCount = 8,
                        SeatsPerRow = 8,
                        IsActive = true
                    });
                }
            }
        }

        // Lưu 6 phòng chiếu
        await db.SaveChangesAsync(cancellationToken);

        // Lấy đúng 6 phòng mẫu của 3 rạp
        var cinemaIds = savedCinemas.Select(c => c.Id).ToArray();

        var savedRooms = await db.Rooms
            .Where(r => cinemaIds.Contains(r.CinemaId)
                && (r.RoomNumber == "01" || r.RoomNumber == "02"))
            .ToListAsync(cancellationToken);

        if (savedRooms.Count != 6)
        {
            throw new InvalidOperationException(
                "Chưa có đủ 6 phòng mẫu để tạo ghế.");
        }

        // Lấy ghế đã tồn tại chỉ với một truy vấn
        var existingSeats = await db.Seats
            .Where(s => savedRooms.Select(r => r.Id).Contains(s.RoomId))
            .Select(s => new { s.RoomId, s.SeatRow, s.SeatNumber })
            .ToListAsync(cancellationToken);

        // HashSet giúp kiểm tra trùng ghế nhanh trong bộ nhớ
        var seatKeys = existingSeats
            .Select(s => (s.RoomId, s.SeatRow, s.SeatNumber))
            .ToHashSet();

        foreach (var room in savedRooms)
        {
            for (int rowIndex = 0; rowIndex < room.RowCount; rowIndex++)
            {
                // 0 -> A, 1 -> B, ..., 7 -> H
                string seatRow = ((char)('A' + rowIndex)).ToString();

                for (int number = 1; number <= room.SeatsPerRow; number++)
                {
                    // Nếu ghế đã tồn tại thì bỏ qua
                    if (!seatKeys.Add((room.Id, seatRow, number)))
                    {
                        continue;
                    }

                    SeatType seatType = rowIndex switch
                    {
                        <= 3 => SeatType.Standard,
                        <= 6 => SeatType.VIP,
                        _ => SeatType.Couple
                    };

                    decimal extraPrice = seatType switch
                    {
                        SeatType.Standard => 0m,
                        SeatType.VIP => 20000m,
                        SeatType.Couple => 40000m,
                        _ => 0m
                    };

                    db.Seats.Add(new Seat
                    {
                        RoomId = room.Id,
                        SeatRow = seatRow,
                        SeatNumber = number,
                        SeatType = seatType,
                        ExtraPrice = extraPrice,
                        IsActive = true
                    });
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        // 6 phim giả lập phục vụ kiểm thử
        var movies = new[]
        {
            new Movie
            {
                Title = "Hành Trình Sao Bắc",
                Slug = "hanh-trinh-sao-bac",
                Description = "Cuộc phiêu lưu khám phá không gian.",
                Director = "Đạo diễn Demo A",
                Actors = "Diễn viên Demo A, Demo B",
                Duration = 120,
                ReleaseDate = new DateOnly(2026, 10, 1),
                MovieFormat = "2D",
                Language = "Tiếng Việt",
                AgeRating = AgeRating.T13,
                IsShowing = true
            },
            new Movie
            {
                Title = "Thành Phố Trong Mưa",
                Slug = "thanh-pho-trong-mua",
                Actors = "Diễn viên Demo C",
                Duration = 105,
                ReleaseDate = new DateOnly(2026, 10, 5),
                MovieFormat = "2D",
                Language = "Tiếng Việt",
                AgeRating = AgeRating.P,
                IsShowing = true
            },
            new Movie
            {
                Title = "Bí Mật Nhà Ga",
                Slug = "bi-mat-nha-ga",
                Actors = "Diễn viên Demo D",
                Duration = 115,
                ReleaseDate = new DateOnly(2026, 10, 10),
                MovieFormat = "2D",
                Language = "Tiếng Việt",
                AgeRating = AgeRating.T16,
                IsShowing = true
            },
            new Movie
            {
                Title = "Ngày Hội Robot",
                Slug = "ngay-hoi-robot",
                Actors = "Diễn viên Demo E",
                Duration = 95,
                ReleaseDate = new DateOnly(2026, 10, 15),
                MovieFormat = "2D",
                Language = "Lồng tiếng Việt",
                AgeRating = AgeRating.P,
                IsShowing = true
            },
            new Movie
            {
                Title = "Mật Mã Đại Dương",
                Slug = "mat-ma-dai-duong",
                Actors = "Diễn viên Demo F",
                Duration = 125,
                ReleaseDate = new DateOnly(2026, 11, 1),
                MovieFormat = "2D",
                Language = "Tiếng Anh - Phụ đề Việt",
                AgeRating = AgeRating.T13,
                IsShowing = true
            },
            new Movie
            {
                Title = "Đêm Trên Đồi Thông",
                Slug = "dem-tren-doi-thong",
                Actors = "Diễn viên Demo G",
                Duration = 110,
                ReleaseDate = new DateOnly(2026, 11, 5),
                MovieFormat = "2D",
                Language = "Tiếng Việt",
                AgeRating = AgeRating.T16,
                IsShowing = true
            }
        };

        // Lấy các Slug đã có trong database
        var existingSlugs = (await db.Movies
            .Select(m => m.Slug)
            .ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Chỉ thêm phim chưa tồn tại
        foreach (var movie in movies)
        {
            if (existingSlugs.Add(movie.Slug))
            {
                db.Movies.Add(movie);
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        // Gắn thể loại cho 6 phim mẫu
        var movieGenreMap = new Dictionary<string, string[]>
        {
            ["hanh-trinh-sao-bac"] = ["Action", "Drama"],
            ["thanh-pho-trong-mua"] = ["Drama"],
            ["bi-mat-nha-ga"] = ["Horror", "Drama"],
            ["ngay-hoi-robot"] = ["Comedy"],
            ["mat-ma-dai-duong"] = ["Action"],
            ["dem-tren-doi-thong"] = ["Horror"]
        };

        // Include tải cả thể loại đã gắn với mỗi phim
        var savedMovies = await db.Movies
            .Include(m => m.Genres)
            .ToListAsync(cancellationToken);

        var savedGenres = await db.Genres
            .ToListAsync(cancellationToken);

        foreach (var item in movieGenreMap)
        {
            var movie = savedMovies.SingleOrDefault(
                m => m.Slug == item.Key)
                ?? throw new InvalidOperationException(
                    $"Không tìm thấy phim: {item.Key}");

            foreach (var genreName in item.Value)
            {
                var genre = savedGenres.SingleOrDefault(
                    g => g.Name == genreName)
                    ?? throw new InvalidOperationException(
                        $"Không tìm thấy thể loại: {genreName}");

                // Tránh tạo liên kết trùng khi chạy lại Seeder
                if (!movie.Genres.Any(g => g.Id == genre.Id))
                {
                    movie.Genres.Add(genre);
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        //tạo 18 suất chiếu mẫu cho 6 phòng và 6 phim, mỗi phòng có 3 suất chiếu
        // Giữ nguyên ngày seed nếu đã có suất chiếu,
        // tránh tạo thêm lịch mới mỗi lần khởi động API.
        DateTime? firstShowtime = await db.Showtimes
        .OrderBy(s => s.StartTime)
        .Select(s => (DateTime?)s.StartTime)
        .FirstOrDefaultAsync(cancellationToken);

        DateTime seedDateUtc = firstShowtime?.Date
            ?? DateTime.UtcNow.Date.AddDays(1);

        // 03:00, 07:00, 11:00 UTC = 10:00, 14:00, 18:00 Việt Nam
        int[] startHoursUtc = [3, 7, 11];

        var roomsForSeed = savedRooms.OrderBy(r => r.Id).ToList();
        var moviesForSeed = savedMovies.OrderBy(m => m.Id).ToList();

        if (roomsForSeed.Count != 6 || moviesForSeed.Count != 6)
        {
            throw new InvalidOperationException(
                "Cần đủ 6 phòng và 6 phim để tạo suất chiếu.");
        }

        // Tải lịch hiện có để kiểm tra trùng giờ
        var existingShowtimes = await db.Showtimes
            .AsNoTracking()
            .Select(s => new
            {
                s.RoomId,
                s.StartTime,
                s.EndTime
            })
            .ToListAsync(cancellationToken);

        // Tạo lịch chiếu cho 3 ngày liên tiếp
        for (int dayOffset = 0; dayOffset < 3; dayOffset++)
        {
            DateTime currentDateUtc = seedDateUtc.AddDays(dayOffset);

            foreach (var room in roomsForSeed)
            {
                for (int slotIndex = 0; slotIndex < startHoursUtc.Length; slotIndex++)
                {
                    // Luân phiên phim giữa các phòng và khung giờ
                    int movieIndex = (room.Id + slotIndex + dayOffset)
                        % moviesForSeed.Count;

                    var movie = moviesForSeed[movieIndex];

                    DateTime startTime = currentDateUtc
                        .AddHours(startHoursUtc[slotIndex]);

                    DateTime endTime = startTime
                        .AddMinutes(movie.Duration);

                    // Kiểm tra trùng thời gian trong cùng phòng
                    bool overlaps = existingShowtimes.Any(s =>
                        s.RoomId == room.Id
                        && s.StartTime < endTime
                        && s.EndTime > startTime);

                    if (overlaps)
                    {
                        continue;
                    }

                    db.Showtimes.Add(new Showtime
                    {
                        MovieId = movie.Id,
                        RoomId = room.Id,
                        StartTime = startTime,
                        EndTime = endTime,
                        TicketPrice = 80000m,
                        IsCancelled = false
                    });
                }
            }
        }

        // Giữ lại dòng SaveChangesAsync có sẵn bên dưới
        await db.SaveChangesAsync(cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
    }
}
