using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Models.Errors;

namespace Middlewares
{

    /// <summary>
    /// Middleware that handles unhandled exceptions, logs them, and returns standardized error responses.
    /// Also measures and logs request duration.
    /// </summary>
    internal class ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> _logger)
    {
        
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            { 
                await next(httpContext);
            }
            catch (NotFoundException e)
            {
                await SetResponse(e, httpContext, HttpStatusCode.NotFound);
            }
            catch (ValidationException e)
            {
                await SetResponse(e, httpContext, HttpStatusCode.BadRequest);
            }
            catch(Exception e)
            {
                await SetResponse(e, httpContext, HttpStatusCode.InternalServerError);
            }
        }

         private async Task SetResponse(Exception e, HttpContext httpContext, HttpStatusCode code)
        {
            if (httpContext.Response.HasStarted)
            {
                _logger.LogWarning("Response has already started, cannot write error details");
                return;
            }

            var response = e is ResourceException resourceException
                ? new 
                {
                    resourceException.ResourceType,
                    resourceException.ResourceId,
                    Detail = GetMessage(resourceException, code),
                    Title = code.ToString(),
                }
                : new 
                {
                    ResourceType = "",
                    ResourceId = "",
                    Detail = e.Message,
                    Title = code.ToString()
                };

            httpContext.Response.StatusCode = (int)code;
            httpContext.Response.ContentType = "application/json";

            var content = JsonSerializer.Serialize(response); // pascal case
            await httpContext.Response.WriteAsync(content);
        }

        private string GetMessage(ResourceException e, HttpStatusCode code)
        {
            return code switch
            {
                HttpStatusCode.NotFound => !string.IsNullOrEmpty(e.Message) ? e.Message : $"{e.ResourceType} not found",
                HttpStatusCode.Conflict => !string.IsNullOrEmpty(e.Message) ? e.Message : $"{e.ResourceType} conflict occurred",
                HttpStatusCode.Unauthorized => !string.IsNullOrEmpty(e.Message) ? e.Message : "Unauthorized access",
                HttpStatusCode.Forbidden => !string.IsNullOrEmpty(e.Message) ? e.Message : "Access forbidden",
                HttpStatusCode.BadRequest => !string.IsNullOrEmpty(e.Message) ? e.Message : "Invalid request",
                _ => !string.IsNullOrEmpty(e.Message) ? e.Message : "An error occurred"
            };
        }

    }   
}