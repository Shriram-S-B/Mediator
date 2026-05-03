using Mediator.Interfaces;
using System.Reflection;

namespace Mediator
{
    /// <summary>
    /// Mediator implementation using dependecy injection container to resolve dependencies and manage communication between components.
    /// </summary>
    public class Mediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;

        public Mediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Sends a request to the appropriate handler and returns a response. The method is asynchronous and can be cancelled using a CancellationToken.
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<TResponse> Send<TResponse>(
                IRequest<TResponse> request,
                CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            var requestType = request.GetType();
            var responseType = typeof(TResponse);

            // Build the handler type : IRequestHandler<TRequest, TResponse>
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);

            //Resolve the handler from the container
            var handler = _serviceProvider.GetService(handlerType);

            if (handler == null)
            {
                throw new InvalidOperationException($"No handler found for request of type {requestType} and response of type {responseType}");
            }

            // Invoke the Handle method on the handler
            var handleMethod = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle));

            if (handleMethod == null)
            {
                throw new InvalidOperationException(
                    $"Handler {handler.GetType().Name} does not implement Handle method");
            }

            //Invoke the handler
            try
            {
                var result = handleMethod.Invoke(handler, new object[] { request, cancellationToken });
                if (result is Task<TResponse> task)
                {
                    return await task;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Handler {handler.GetType().Name} returned an invalid result. Expected Task<{responseType}> but got {result?.GetType().Name ?? "null"}");
                }
            }
            catch (TargetInvocationException ex)
            {
                // Unwrap the original exception thrown by the handler
                throw ex.InnerException ?? ex;
            }
        }
    }
}


