using System.Threading.Tasks;
using MediatR;
using WebApplication;

namespace SO.Tests
{
    public class TestMessageBus : IMessageBus
    {
        private readonly IMediator _mediator;

        public TestMessageBus(IMediator mediator)
        {
            _mediator = mediator;
        }

        public void Publish<T>(T param)
            where T : INotification
        {
            _mediator.Publish(param).Wait();
        }

        public async Task PublishAsync<T>(T param)
            where T : INotification
        {
            await _mediator.Publish(param).ConfigureAwait(false);
        }

        public Task<TOut> Send<TIn, TOut>(TIn request)
            where TIn : IRequest<TOut>
        {
            return _mediator.Send(request);
        }
    }
}