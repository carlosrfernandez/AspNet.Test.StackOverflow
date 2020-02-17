using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace WebApplication
{
    public class MessageBus : IMessageBus
    {
        private readonly IServiceProvider _serviceProvider;

        public MessageBus(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Publish<T>(T param)
            where T : INotification
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var mediator = scope.ServiceProvider.GetService<Publisher>();
                mediator.Publish(param, PublishStrategy.ParallelNoWait).ConfigureAwait(false);
            }
        }

        public async Task PublishAsync<T>(T param)
            where T : INotification
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var mediator = scope.ServiceProvider.GetService<Publisher>();
                await mediator.Publish(param, PublishStrategy.ParallelWhenAll).ConfigureAwait(false);
            }
        }

        public async Task<TOut> Send<TIn, TOut>(TIn request)
            where TIn : IRequest<TOut>
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var mediator = scope.ServiceProvider.GetService<IMediator>();
                var result = await mediator.Send(request).ConfigureAwait(false);
                return result;
            }
        }
    }
}