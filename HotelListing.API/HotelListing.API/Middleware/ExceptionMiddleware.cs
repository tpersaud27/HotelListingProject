using HotelListing.API.Exceptions;
using Microsoft.AspNetCore.Http.HttpResults;
using Newtonsoft.Json;
using System.Net;

namespace HotelListing.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        // Request delegate will intercept the request so we can do things to it
        // This will be intercepting the request pipeline
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // awaiting the result of the next operation from the request
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong while processing {context.Request.Path}");
                // Handle the exception if we get any
                await HandleExecptionAsync(context, ex);
            }
        }

        /// <summary>
        /// This will handle the variety of exceptions that can be throw
        /// Essentially global exception handling
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        private Task HandleExecptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            var errorDetails = new ErrorDetails
            {
                ErrorType = "Failure",
                ErrorMessage = ex.Message
            };

            // These are custom exceptions
            switch(ex)
            {
                case NotFoundException notFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    errorDetails.ErrorType = "Not Found";
                    break;
                default:
                    break;
            }

            // Serialize the error details into json string response
            string response = JsonConvert.SerializeObject(errorDetails);
            // Convert the code to an int
            context.Response.StatusCode = (int)statusCode;

            return context.Response.WriteAsync(response);

        }

    }

    public class ErrorDetails 
    {
        // This is the type of error (not found, null, etc)
        public string ErrorType { get; set; }
        // This is the message associated to the error
        public string ErrorMessage { get; set; } 

    }

}
