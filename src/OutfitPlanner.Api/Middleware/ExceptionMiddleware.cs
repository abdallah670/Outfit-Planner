
using Microsoft.AspNetCore.Http;
using OutfitPlanner.Application.Exceptions;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
namespace OutfitPlanner.Api.Middleware{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private  ErrorDetails errorDetails;
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }
        private async Task HandleExceptionAsync(HttpContext httpContext, Exception ex)
        {
            httpContext.Response.ContentType = "application/json";
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            string result = JsonSerializer.Serialize(new ErrorDetails 
                { 
                    ErrorMessage = ex.Message, 
                    ErrorType = "Failure" 
                });

            switch (ex)
            {
                case BadRequestException badRequestException:
                    statusCode = HttpStatusCode.BadRequest;
                    break;
                case ValidationException validationException:
                    statusCode = HttpStatusCode.BadRequest;
                    result = JsonSerializer.Serialize(validationException.Errors);
                    break;
                case NotFoundException notFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    break;
                // case DbUpdateConcurrencyException concurrencyEx:
                //     statusCode = HttpStatusCode.Conflict;
                //     errorDetails.ErrorType = "ConcurrencyError";
                //     errorDetails.ErrorMessage = "The data you are trying to update has been modified or deleted by another user. Please refresh and try again.";
                //     result = JsonSerializer.Serialize(errorDetails);
                //     _logger.LogWarning(concurrencyEx, "Concurrency conflict detected during update.");
                //     break;
                default:
                    break;
            }
            httpContext.Response.StatusCode = (int)statusCode;
            await httpContext.Response.WriteAsync(result);
            
        }
    }
    public class ErrorDetails
    {
        public string ErrorType { get; set; }
        public string ErrorMessage { get; set; }
    }

}



