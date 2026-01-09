using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProjectManager.Application.DTO;

namespace ProjectManager.Filters
{
    public sealed class ApiExceptionFilter : IExceptionFilter
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ApiExceptionFilter> _logger;

        public ApiExceptionFilter(IWebHostEnvironment environment, ILogger<ApiExceptionFilter> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            if (context.ExceptionHandled)
            {
                return;
            }

            var httpStatus = HttpStatusCode.InternalServerError;
            var errorMessage = "An unexpected error occurred.";

            // Here you can map custom domain exceptions to specific status codes/messages
            // Example:
            // if (context.Exception is NotFoundException)
            // {
            //     httpStatus = HttpStatusCode.NotFound;
            //     errorMessage = context.Exception.Message;
            // }

            _logger.LogError(
                context.Exception,
                "Unhandled exception while processing request {Path}",
                context.HttpContext.Request.Path);

            var traceId = context.HttpContext.TraceIdentifier;

            var response = new ErrorResponseDTO
            {
                StatusCode = (int)httpStatus,
                Error = errorMessage,
                Details = _environment.IsDevelopment() ? context.Exception.Message : null,
                TraceId = traceId
            };

            context.Result = new ObjectResult(response)
            {
                StatusCode = (int)httpStatus
            };

            context.ExceptionHandled = true;
        }
    }
}