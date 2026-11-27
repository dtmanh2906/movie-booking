namespace CinemaBooking.Application.Rooms;

public interface IRoomService
{
    Task<List<RoomDto>> GetAllAsync(int? cinemaId = null, CancellationToken cancellationToken = default);
    Task<RoomDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<RoomDto> CreateAsync(CreateRoomRequest request, CancellationToken cancellationToken = default);
    Task<RoomDto> UpdateAsync(int id, UpdateRoomRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<List<SeatDto>> GetSeatsAsync(int roomId, CancellationToken cancellationToken = default);
    Task<SeatDto> UpdateSeatAsync(int roomId, int seatId, UpdateSeatRequest request, CancellationToken cancellationToken = default);
}
