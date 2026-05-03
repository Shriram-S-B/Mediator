namespace Mediator.Interfaces
{
    /// <summary>
    /// Represents a request with a response of type <typeparamref name="TResponse"/>.
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    public interface IRequest<out TResponse>
    {
    }
}
