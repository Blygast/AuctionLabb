using AuctionApi.Core.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AuctionApi.Common.Middleware;

public class DomainExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case NotFoundException nf:
                context.Result = new NotFoundObjectResult(nf.Message);
                context.ExceptionHandled = true;
                break;
            case ValidationException ve:
                context.Result = new BadRequestObjectResult(ve.Message);
                context.ExceptionHandled = true;
                break;
            case ConflictException ce:
                context.Result = new ConflictObjectResult(ce.Message);
                context.ExceptionHandled = true;
                break;
            case ForbiddenException fe:
                var status = context.HttpContext.User?.Identity?.IsAuthenticated == true
                    ? StatusCodes.Status403Forbidden
                    : StatusCodes.Status401Unauthorized;
                context.Result = new ObjectResult(fe.Message) { StatusCode = status };
                context.ExceptionHandled = true;
                break;
        }
    }
}
