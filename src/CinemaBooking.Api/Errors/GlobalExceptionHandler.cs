
using System;
using System.Threading;
using System.Threading.Tasks;
using CinemaBooking.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CinemaBooking.Api.Errors;

public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Chuyển loại exception thành mã HTTP
        var (status, title) = exception switch
        {
            NotFoundException =>
                (StatusCodes.Status404NotFound,
                 "Không tìm thấy dữ liệu."),

            ConflictException =>
                (StatusCodes.Status409Conflict,
                 "Dữ liệu bị xung đột."),

            BadRequestException =>
                (StatusCodes.Status400BadRequest,
                 "Yêu cầu không hợp lệ."),

            // Lỗi vi phạm unique index của SQL Server
            DbUpdateException
            {
                InnerException: SqlException
                {
                    Number: 2601 or 2627
                }
            } =>
                (StatusCodes.Status409Conflict,
                 "Dữ liệu bị trùng."),

            // Các exception chưa được định nghĩa
            _ =>
                (StatusCodes.Status500InternalServerError,
                 "Đã xảy ra lỗi hệ thống.")
        };

        // Ghi log để developer theo dõi lỗi
        if (status >= 500)
        {
            logger.LogError(
                exception,
                "Lỗi không mong đợi tại {Path}",
                httpContext.Request.Path);
        }
        else
        {
            logger.LogWarning(
                "Yêu cầu bị từ chối ({Status}): {Message}",
                status,
                exception.Message);
        }

        // Chỉ cho phép hiển thị nội dung lỗi do ta định nghĩa
        var isOurError = exception is
            NotFoundException or
            ConflictException or
            BadRequestException;

        httpContext.Response.StatusCode = status;

        // Trả về JSON theo chuẩn ProblemDetails
        return await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Status = status,
                    Title = title,
                    Detail = isOurError
                        ? exception.Message
                        : null
                }
            });
    }
}
