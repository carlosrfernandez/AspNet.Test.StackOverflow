using System.Threading.Tasks;
using MediatR;

namespace WebApplication
{
    public interface IMessageBus
    {
        /// <summary>
        /// Publishes the message asynchronously without waiting for handlers to finish.
        /// </summary>
        /// <typeparam name="T">Type of the message.</typeparam>
        /// <param name="param">Message object.</param>
        /// <remarks>The exceptions of handlers need to be handled internally and the caller will not be notified about them.</remarks>
        void Publish<T>(T param)
            where T : INotification;

        /// <summary>
        /// Publishes the message asynchronously.
        /// </summary>
        /// <typeparam name="T">Type of the message.</typeparam>
        /// <param name="param">Message object.</param>
        /// <returns>The task that lasts until when all handlers are finished.</returns>
        Task PublishAsync<T>(T param)
            where T : INotification;

        /// <summary>
        /// Sends a request expecting a result.
        /// </summary>
        /// <typeparam name="TIn">The request type.</typeparam>
        /// <typeparam name="TOut">The result type.</typeparam>
        /// <param name="request">The request.</param>
        /// <returns>The result.</returns>
        Task<TOut> Send<TIn, TOut>(TIn request)
            where TIn : IRequest<TOut>;
    }
}