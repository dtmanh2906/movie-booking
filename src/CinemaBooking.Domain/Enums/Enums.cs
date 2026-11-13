namespace CinemaBooking.Domain.Enums;

public enum AgeRating
{
    P,
    K,
    T13,
    T16,
    T18
}

public enum SeatType
{
    Standard,
    VIP,
    Couple
}

public enum BookingStatus
{
    Pending,
    Paid,
    Cancelled,
    Expired
}

public enum PaymentStatus
{
    Pending,
    Success,
    Failed
}