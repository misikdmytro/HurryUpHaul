using System.Text.Json;

using HurryUpHaul.Contracts.Http;

using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace HurryUpHaul.Api.Helpers
{
    public static class JwtBearerHelpers
    {
        public static Task OnChallenge(JwtBearerChallengeContext context)
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var result = JsonSerializer.Serialize(new ErrorResponse
            {
                Errors = ["You are not authorized to access this resource."]
            });

            return context.Response.WriteAsync(result);
        }
    }
}