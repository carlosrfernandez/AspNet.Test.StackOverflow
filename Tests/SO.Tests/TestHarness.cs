using System;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using WebApplication;
using WebApplication.Controllers;

namespace SO.Tests
{
    public class TestHarness
    {
        private readonly ServiceCollection _serviceCollection;

        public TestHarness()
        {
            _serviceCollection = new ServiceCollection();
            ConfigureMessaging();
            _serviceCollection.AddScoped<WeatherForecastController>();
            ServiceProvider = _serviceCollection.BuildServiceProvider();
        }

        public ServiceProvider ServiceProvider { get; }

        private void ConfigureMessaging()
        {
            _serviceCollection.AddSingleton<IMessageBus, TestMessageBus>();
            _serviceCollection.AddSingleton<Publisher>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                _serviceCollection.AddMediatR(assembly);
            }
        }
    }
}