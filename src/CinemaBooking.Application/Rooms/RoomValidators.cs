using FluentValidation;

namespace CinemaBooking.Application.Rooms;

public class CreateRoomRequestValidator
    : AbstractValidator<CreateRoomRequest>
{
    public CreateRoomRequestValidator()
    {
        RuleFor(x => x.CinemaId)
            .GreaterThan(0);

        RuleFor(x => x.RoomNumber)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.RowCount)
            .InclusiveBetween(1, 26)
            .WithMessage("Row count must be between 1 and 26.");

        RuleFor(x => x.SeatsPerRow)
            .InclusiveBetween(1, 30)
            .WithMessage("Seats per row must be between 1 and 30.");
    }
}

public class UpdateRoomRequestValidator
    : AbstractValidator<UpdateRoomRequest>
{
    public UpdateRoomRequestValidator()
    {
        RuleFor(x => x.RoomNumber)
            .NotEmpty()
            .MaximumLength(20);
    }
}