using Application.Exceptions;
using Infrastructure.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;


namespace Api.ExceptionHandlers
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IProblemDetailsService _problemDetailsService;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IProblemDetailsService problemDetailsService)
        {
            _logger = logger;
            _problemDetailsService = problemDetailsService;
        }
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (code, title) =  (exception) switch
            {
                ArgumentException => (StatusCodes.Status400BadRequest, "Invalid Request"),

                NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),

                ConflictException => (StatusCodes.Status409Conflict, "Conflict"),

                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
            };

            if(StatusCodes.Status500InternalServerError == code)
            {
                _logger.LogError(exception, "An unhandled exception occurred.");

            }

            httpContext.Response.StatusCode = code;
            var problemDetails = new ProblemDetails
            {
                Status = code,
                Title = title,
                Detail = code == StatusCodes.Status500InternalServerError ? "An internal error occurred." : exception.Message,
                Instance = httpContext.Request.Path
            };
            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
            return await _problemDetailsService.TryWriteAsync( new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails,
                Exception = exception
            });
        }
    
    }
}
