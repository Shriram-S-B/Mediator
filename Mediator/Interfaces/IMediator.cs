

namespace Mediator.Interfaces
{
    /// <summary>
    /// Define a mediator interface for sending requests and receiving responses asynchronously.
    /// </summary>
    public interface IMediator
    {
        /// <summary>
        /// Sends a request of type <typeparamref name="TResponse"/> and returns a response asynchronously.
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    }
}
