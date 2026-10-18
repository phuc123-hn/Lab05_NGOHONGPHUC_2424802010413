using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PricingService.Infrastructure;

/// <summary>Đổi lỗi validate thành HTTP 400, sản phẩm không tồn tại thành HTTP 404.</summary>
public class ValidationExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case ValidationException validationException:
                context.Result = new BadRequestObjectResult(validationException.Errors
                    .Select(e => new { e.PropertyName, e.ErrorMessage }));
                context.ExceptionHandled = true;
                break;
            case KeyNotFoundException notFound:
                context.Result = new NotFoundObjectResult(notFound.Message);
                context.ExceptionHandled = true;
                break;
        }
    }
}
