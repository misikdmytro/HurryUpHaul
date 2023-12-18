using MediatR;

using Microsoft.Extensions.Logging;


namespace HurryUpHaul.Domain.Handlers
{
    internal abstract class BaseHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        protected readonly ILogger Logger;

        protected BaseHandler(ILogger logger)
        {
            Logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogDebug("Handling request {request}", request);
                var result = await HandleInternal(request, cancellationToken);
                Logger.LogInformation("Request {request} handled successfully. Result: {result}", request, result);
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error handling request {request}", request);
                throw;
            }
        }

        protected abstract Task<TResponse> HandleInternal(TRequest request, CancellationToken cancellationToken);
    }
}