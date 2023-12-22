using System.Net;
using System.Text.Json;

using HurryUpHaul.Contracts.Http;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace HurryUpHaul.Api.Helpers
{
    public class AppAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

        public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
        {
            if (authorizeResult.Forbidden)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                context.Response.ContentType = "application/json";

                var result = JsonSerializer.Serialize(new ErrorResponse
                {
                    Errors = ["You are not authorized to access this resource."]
                });

                await context.Response.WriteAsync(result);

                return;
            }

            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}