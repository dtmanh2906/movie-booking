using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CinemaBooking.Application.Cinemas;

public interface ICinemaService
{
    Task<List<CinemaDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<CinemaDto> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<CinemaDto> CreateAsync(
        CreateCinemaRequest request,
        CancellationToken cancellationToken = default);

    Task<CinemaDto> UpdateAsync(
        int id,
        UpdateCinemaRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);
}