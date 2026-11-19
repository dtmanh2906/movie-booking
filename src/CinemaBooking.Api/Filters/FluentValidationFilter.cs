
using System;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CinemaBooking.Api.Filters;

public class FluentValidationFilter(IServiceProvider services)
    : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        // Duyệt qua các tham số mà Controller nhận được
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
                continue;

            // Tìm kiểu IValidator tương ứng với request
            var validatorType = typeof(IValidator<>)
                .MakeGenericType(argument.GetType());

            // Nếu không có validator thì bỏ qua
            if (services.GetService(validatorType)
                is not IValidator validator)
            {
                continue;
            }

            // Thực hiện kiểm tra dữ liệu
            var result = await validator.ValidateAsync(
                new ValidationContext<object>(argument));

            if (result.IsValid)
                continue;

            // Đưa từng lỗi vào ModelState
            foreach (var error in result.Errors)
            {
                context.ModelState.AddModelError(
                    error.PropertyName,
                    error.ErrorMessage);
            }

            // Trả HTTP 400 kèm danh sách lỗi
            context.Result = new BadRequestObjectResult(
                new ValidationProblemDetails(context.ModelState));

            return;
        }

        // Dữ liệu hợp lệ: cho Controller tiếp tục chạy
        await next();
    }
}