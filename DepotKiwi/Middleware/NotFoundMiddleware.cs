using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using DepotKiwi.RequestModels;

namespace DepotKiwi.Middleware {
    public class NotFoundMiddleware {
        public NotFoundMiddleware(RequestDelegate next) {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context) {
            await _next(context);

            if (!context.Response.HasStarted && context.Response.StatusCode == StatusCodes.Status404NotFound) {
                await context.Response.WriteAsJsonAsync(new StatusResponse {
                    Success = false,
                    Message = "content does not exist on this server."
                });
            }
        }

        private readonly RequestDelegate _next;
    }
}