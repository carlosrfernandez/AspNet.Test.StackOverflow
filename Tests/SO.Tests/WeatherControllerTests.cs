using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using WebApplication.Controllers;
using Xunit;

namespace SO.Tests
{
    public class WeatherControllerTests
    {
        [Fact]
        public async Task WeatherControllerReturnsShit()
        {
            var testHarness = new TestHarness();
            using var scope = testHarness.ServiceProvider.CreateScope();
            var controller = scope.ServiceProvider.GetService<WeatherForecastController>();

            var result = await controller.Get();
            result.Should().BeOfType<OkObjectResult>();
            result.Value.Should().NotBeEmpty();
        }
    }
}