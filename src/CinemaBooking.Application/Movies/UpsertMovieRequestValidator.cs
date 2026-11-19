using System;
using FluentValidation;

namespace CinemaBooking.Application.Movies;

public class UpsertMovieRequestValidator
    : AbstractValidator<UpsertMovieRequest>
{
    public UpsertMovieRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Tên phim không được để trống.")
            .MaximumLength(200)
            .WithMessage("Tên phim tối đa 200 ký tự.");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Mô tả tối đa 2000 ký tự.");

        RuleFor(x => x.Director)
            .MaximumLength(100)
            .WithMessage("Tên đạo diễn tối đa 100 ký tự.");

        RuleFor(x => x.Actors)
            .MaximumLength(500)
            .WithMessage("Danh sách diễn viên tối đa 500 ký tự.");

        RuleFor(x => x.Duration)
            .InclusiveBetween(1, 500)
            .WithMessage("Thời lượng phải từ 1 đến 500 phút.");

        RuleFor(x => x.ReleaseDate)
            .NotEqual(default(DateOnly))
            .WithMessage("Ngày khởi chiếu không hợp lệ.");

        RuleFor(x => x.MovieFormat)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Language)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.AgeRating)
            .IsInEnum()
            .WithMessage("Nhãn độ tuổi không hợp lệ.");

        RuleFor(x => x.GenreIds)
            .NotEmpty()
            .WithMessage("Phải chọn ít nhất một thể loại.");

        RuleFor(x => x.BannerUrl)
            .MaximumLength(255)
            .Must(BeEmptyOrHttpUrl)
            .WithMessage("Banner phải là đường dẫn http hoặc https hợp lệ.");

        RuleFor(x => x.PosterUrl)
            .MaximumLength(255)
            .Must(BeEmptyOrHttpUrl)
            .WithMessage("Poster phải là đường dẫn http hoặc https hợp lệ.");

        RuleFor(x => x.TrailerUrl)
            .MaximumLength(255)
            .Must(BeEmptyOrHttpUrl)
            .WithMessage("Trailer phải là đường dẫn http hoặc https hợp lệ.");
    }

    // Chỉ chấp nhận URL http hoặc https
    private static bool BeEmptyOrHttpUrl(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            || (Uri.TryCreate(value, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp
                    || uri.Scheme == Uri.UriSchemeHttps));
    }
}