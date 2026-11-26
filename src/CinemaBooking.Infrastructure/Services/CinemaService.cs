using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CinemaBooking.Application.Cinemas;
using CinemaBooking.Application.Exceptions;
using CinemaBooking.Domain.Entities;
using CinemaBooking.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Infrastructure.Services;

public class CinemaService(AppDbContext db) : ICinemaService
{
    // 1. Lấy danh sách rạp
    public async Task<List<CinemaDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await db.Cinemas
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CinemaDto
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                Address = c.Address,
                District = c.District,
                City = c.City,
                Hotline = c.Hotline,
                Email = c.Email,
                IsActive = c.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    // 2. Lấy rạp theo ID
    public async Task<CinemaDto> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var cinema = await db.Cinemas
            .AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.Id == id,
                cancellationToken)
            ?? throw new NotFoundException(
                $"Không tìm thấy rạp có id {id}.");

        return ToDto(cinema);
    }

    // 3. Tạo rạp mới
    public async Task<CinemaDto> CreateAsync(
        CreateCinemaRequest request,
        CancellationToken cancellationToken = default)
    {
        var code = request.Code.Trim();

        if (await db.Cinemas.AnyAsync(
            c => c.Code == code,
            cancellationToken))
        {
            throw new ConflictException(
                $"Mã rạp '{code}' đã tồn tại.");
        }

        var cinema = new Cinema
        {
            Name = request.Name.Trim(),
            Code = code,
            Address = request.Address.Trim(),
            District = request.District.Trim(),
            City = request.City.Trim(),
            Hotline = NormalizeOptional(request.Hotline),
            Email = NormalizeOptional(request.Email),
            IsActive = true
        };

        db.Cinemas.Add(cinema);

        await SaveCinemaAsync(code, cancellationToken);

        return ToDto(cinema);
    }

    // 4. Cập nhật thông tin rạp
    public async Task<CinemaDto> UpdateAsync(
        int id,
        UpdateCinemaRequest request,
        CancellationToken cancellationToken = default)
    {
        var cinema = await FindAsync(
            id, cancellationToken);

        var code = request.Code.Trim();

        // Không tính chính rạp đang cập nhật
        if (await db.Cinemas.AnyAsync(
            c => c.Code == code && c.Id != id,
            cancellationToken))
        {
            throw new ConflictException(
                $"Mã rạp '{code}' đã tồn tại.");
        }

        // Không cho vô hiệu hóa qua PUT
        // nếu rạp còn suất chiếu tương lai
        if (request.IsActive == false)
        {
            await EnsureCanDeactivateAsync(
                id, cancellationToken);
        }

        cinema.Name = request.Name.Trim();
        cinema.Code = code;
        cinema.Address = request.Address.Trim();
        cinema.District = request.District.Trim();
        cinema.City = request.City.Trim();
        cinema.Hotline = NormalizeOptional(request.Hotline);
        cinema.Email = NormalizeOptional(request.Email);

        // Không gửi IsActive thì giữ nguyên trạng thái
        cinema.IsActive = request.IsActive ?? cinema.IsActive;

        await SaveCinemaAsync(code, cancellationToken);

        return ToDto(cinema);
    }

    // 5. Xóa mềm rạp
    public async Task DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var cinema = await FindAsync(
            id, cancellationToken);

        await EnsureCanDeactivateAsync(
            id, cancellationToken);

        cinema.IsActive = false;

        await db.SaveChangesAsync(cancellationToken);
    }

    // Tìm entity để cập nhật hoặc xóa mềm
    private async Task<Cinema> FindAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await db.Cinemas.FirstOrDefaultAsync(
            c => c.Id == id,
            cancellationToken)
            ?? throw new NotFoundException(
                $"Không tìm thấy rạp có id {id}.");
    }

    // Kiểm tra rạp còn suất chiếu tương lai hay không
    private async Task EnsureCanDeactivateAsync(
        int cinemaId,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var hasFutureShowtimes = await db.Showtimes.AnyAsync(
            s => s.Room.CinemaId == cinemaId
                && !s.IsCancelled
                && s.StartTime >= now,
            cancellationToken);

        if (hasFutureShowtimes)
        {
            throw new ConflictException(
                "Không thể vô hiệu hóa rạp còn suất chiếu tương lai.");
        }
    }

    // Bắt lỗi vi phạm unique index từ SQL Server
    private async Task SaveCinemaAsync(
        string code,
        CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is SqlException sqlException
                && (sqlException.Number is 2601 or 2627))
        {
            throw new ConflictException(
                $"Mã rạp '{code}' đã tồn tại.");
        }
    }

    // Chuẩn hóa dữ liệu không bắt buộc
    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    // Chuyển Entity thành DTO
    private static CinemaDto ToDto(Cinema cinema) => new()
    {
        Id = cinema.Id,
        Name = cinema.Name,
        Code = cinema.Code,
        Address = cinema.Address,
        District = cinema.District,
        City = cinema.City,
        Hotline = cinema.Hotline,
        Email = cinema.Email,
        IsActive = cinema.IsActive
    };
}