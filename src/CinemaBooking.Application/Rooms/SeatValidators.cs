using System;
using FluentValidation;

namespace CinemaBooking.Application.Rooms;

public class UpdateSeatRequestValidator
    : AbstractValidator<UpdateSeatRequest>
{
    public UpdateSeatRequestValidator()
    {
        RuleFor(x => x.SeatType)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(type => type.HasValue
                && Enum.IsDefined(type.Value))
            .WithMessage("Invalid seat type.");

        RuleFor(x => x.ExtraPrice)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(price => price.HasValue
                && price.Value >= 0
                && price.Value <= 999999999999m
                && decimal.Truncate(price.Value) == price.Value)
            .WithMessage(
                "Extra price must be a non-negative integer "
                + "with at most 12 digits.");
    }
}