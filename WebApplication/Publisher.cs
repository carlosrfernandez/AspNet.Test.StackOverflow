using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace WebApplication
{
    public class Publisher
    {
        private readonly IDictionary<PublishStrategy, IMediator> _publishStrategies = new Dictionary<PublishStrategy, IMediator>();

        public Publisher(ServiceFactory serviceFactory)
        {
            _publishStrategies[PublishStrategy.Async] = new CustomMediator(serviceFactory, AsyncContinueOnException);
            _publishStrategies[PublishStrategy.ParallelNoWait] = new CustomMediator(serviceFactory, ParallelNoWait);
            _publishStrategies[PublishStrategy.ParallelWhenAll] = new CustomMediator(serviceFactory, ParallelWhenAll);
            _publishStrategies[PublishStrategy.ParallelWhenAny] = new CustomMediator(serviceFactory, ParallelWhenAny);
            _publishStrategies[PublishStrategy.SyncContinueOnException] =
                new CustomMediator(serviceFactory, SyncContinueOnException);
            _publishStrategies[PublishStrategy.SyncStopOnException] =
                new CustomMediator(serviceFactory, SyncStopOnException);
        }

        private PublishStrategy DefaultStrategy { get; } = PublishStrategy.SyncContinueOnException;

        public Task Publish<TNotification>(TNotification notification)
        {
            return Publish(notification, DefaultStrategy, default);
        }

        public Task Publish<TNotification>(TNotification notification, PublishStrategy strategy)
        {
            return Publish(notification, strategy, default);
        }

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken)
        {
            return Publish(notification, DefaultStrategy, cancellationToken);
        }

        public Task Publish<TNotification>(TNotification notification, PublishStrategy strategy, CancellationToken cancellationToken)
        {
            if (!_publishStrategies.TryGetValue(strategy, out var mediator))
            {
                throw new ArgumentException($"Unknown strategy: {strategy}");
            }

            return mediator.Publish(notification, cancellationToken);
        }

        private Task ParallelWhenAll(
            IEnumerable<Func<INotification, CancellationToken, Task>> handlers,
            INotification notification,
            CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();

            foreach (var handler in handlers)
            {
                tasks.Add(Task.Run(() => handler(notification, cancellationToken)));
            }

            return Task.WhenAll(tasks);
        }

        private Task ParallelWhenAny(
            IEnumerable<Func<INotification, CancellationToken, Task>> handlers,
            INotification notification,
            CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();

            foreach (var handler in handlers)
            {
                tasks.Add(Task.Run(() => handler(notification, cancellationToken)));
            }

            return Task.WhenAny(tasks);
        }

        private Task ParallelNoWait(
            IEnumerable<Func<INotification, CancellationToken, Task>> handlers,
            INotification notification,
            CancellationToken cancellationToken)
        {
            foreach (var handler in handlers)
            {
                Task.Run(() => handler(notification, cancellationToken), cancellationToken);
            }

            return Task.CompletedTask;
        }

        private async Task AsyncContinueOnException(
            IEnumerable<Func<INotification, CancellationToken, Task>> handlers,
            INotification notification,
            CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();
            var exceptions = new List<Exception>();

            foreach (var handler in handlers)
            {
                try
                {
                    tasks.Add(handler(notification, cancellationToken));
                }
                catch (Exception ex) when (!(ex is OutOfMemoryException || ex is StackOverflowException))
                {
                    exceptions.Add(ex);
                }
            }

            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (AggregateException ex)
            {
                exceptions.AddRange(ex.Flatten().InnerExceptions);
            }
            catch (Exception ex) when (!(ex is OutOfMemoryException || ex is StackOverflowException))
            {
                exceptions.Add(ex);
            }

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }

        private async Task SyncStopOnException(
            IEnumerable<Func<INotification, CancellationToken, Task>> handlers,
            INotification notification,
            CancellationToken cancellationToken)
        {
            foreach (var handler in handlers)
            {
                await handler(notification, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task SyncContinueOnException(
            IEnumerable<Func<INotification, CancellationToken, Task>> handlers,
            INotification notification,
            CancellationToken cancellationToken)
        {
            var exceptions = new List<Exception>();

            foreach (var handler in handlers)
            {
                try
                {
                    await handler(notification, cancellationToken).ConfigureAwait(false);
                }
                catch (AggregateException ex)
                {
                    exceptions.AddRange(ex.Flatten().InnerExceptions);
                }
                catch (Exception ex) when (!(ex is OutOfMemoryException || ex is StackOverflowException))
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }
        }
    }
}