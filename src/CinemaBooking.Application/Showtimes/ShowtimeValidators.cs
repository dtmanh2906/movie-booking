using FluentValidation;

namespace CinemaBooking.Application.Showtimes;

public class CreateShowtimeRequestValidator : AbstractValidator<CreateShowtimeRequest>
{
    public CreateShowtimeRequestValidator()
    {
        RuleFor(x => x.MovieId).GreaterThan(0);
        RuleFor(x => x.RoomId).GreaterThan(0);
        RuleFor(x => x.StartTime).Must(t => t > DateTimeOffset.UtcNow)
            .WithMessage("Thời gian bắt đầu phải ở tương lai.");
        RuleFor(x => x.TicketPrice).Must(ValidPrice)
            .WithMessage("Giá vé phải là số nguyên dương, tối đa 12 chữ số.");
    }

    public static bool ValidPrice(decimal price) =>
        price > 0 && price <= 999999999999m && decimal.Truncate(price) == price;
}

public class UpdateShowtimeRequestValidator : AbstractValidator<UpdateShowtimeRequest>
{
    public UpdateShowtimeRequestValidator()
    {
        RuleFor(x => x).Must(x => x.StartTime.HasValue || x.TicketPrice.HasValue)
            .WithMessage("Cần gửi StartTime hoặc TicketPrice.");
        RuleFor(x => x.StartTime).Must(t => t != default(DateTimeOffset))
            .When(x => x.StartTime.HasValue);
        RuleFor(x => x.TicketPrice).Must(p => CreateShowtimeRequestValidator.ValidPrice(p!.Value))
            .When(x => x.TicketPrice.HasValue)
            .WithMessage("Giá vé phải là số nguyên dương, tối đa 12 chữ số.");
    }
}
