

namespace Mediator.Interfaces
{
    /// <summary>
    /// Defines a handler for processing requests of type <typeparamref name="TRequest"/> and returning a response of type <typeparamref name="TResponse"/>.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public interface IRequestHandler<in TRequest, TResponse> 
        where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// Handles the given request and returns a response asynchronously.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }
}
