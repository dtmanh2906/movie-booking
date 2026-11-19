using FluentValidation;

namespace CinemaBooking.Application.Genres;

public class UpsertGenreRequestValidator
    : AbstractValidator<UpsertGenreRequest>
{
    public UpsertGenreRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Tên thể loại không được để trống.")
            .MaximumLength(50)
            .WithMessage("Tên thể loại tối đa 50 ký tự.");
    }
}