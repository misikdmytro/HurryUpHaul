using HurryUpHaul.Contracts.Http;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HurryUpHaul.Api.Filters
{
    internal class AppExceptionFilter : IAsyncExceptionFilter
    {
        private readonly ILogger<AppExceptionFilter> _logger;

        public AppExceptionFilter(ILogger<AppExceptionFilter> logger)
        {
            _logger = logger;
        }

        public Task OnExceptionAsync(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Unhandled exception");

            context.Result = new ObjectResult(new ErrorResponse
            {
                Errors = ["An unexpected error occurred"]
            })
            {
                StatusCode = 500
            };

            return Task.CompletedTask;
        }
    }
}