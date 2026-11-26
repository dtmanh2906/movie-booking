using System.Linq;
using FluentValidation;

namespace CinemaBooking.Application.Cinemas;

public class CreateCinemaRequestValidator
    : AbstractValidator<CreateCinemaRequest>
{
    public CreateCinemaRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Code)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(20)
            .Must(code => !code.Any(char.IsWhiteSpace))
            .WithMessage("Cinema code must not contain whitespace.");

        RuleFor(x => x.Address)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.District)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Hotline)
            .MaximumLength(20);

        RuleFor(x => x.Email)
            .MaximumLength(100)
            .EmailAddress()
            .WithMessage("Invalid email address.")
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}

public class UpdateCinemaRequestValidator
    : AbstractValidator<UpdateCinemaRequest>
{
    public UpdateCinemaRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Code)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MaximumLength(20)
            .Must(code => !code.Any(char.IsWhiteSpace))
            .WithMessage("Cinema code must not contain whitespace.");

        RuleFor(x => x.Address)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.District)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Hotline)
            .MaximumLength(20);

        RuleFor(x => x.Email)
            .MaximumLength(100)
            .EmailAddress()
            .WithMessage("Invalid email address.")
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}