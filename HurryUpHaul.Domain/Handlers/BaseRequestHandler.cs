using MediatR;

using Microsoft.Extensions.Logging;

namespace HurryUpHaul.Domain.Handlers
{
    internal abstract class BaseRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        protected readonly ILogger Logger;

        protected BaseRequestHandler(ILogger logger)
        {
            Logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogDebug("Handling request {@Request}", request);
                var result = await HandleInternal(request, cancellationToken);
                Logger.LogInformation("Request {@Request} handled successfully. Result: {@Result}", request, result);
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error handling request {@Request}", request);
                throw;
            }
        }

        protected abstract Task<TResponse> HandleInternal(TRequest request, CancellationToken cancellationToken);
    }
}