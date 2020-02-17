using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : Controller
    {
        private readonly IMessageBus _messageBus;

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public WeatherForecastController(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WeatherForecast>>> Get()
        {
            var weatherRequest = new WeatherRequest("Hi");

            var resultTask = await GetWeather(weatherRequest);

            return Ok(resultTask);
        }

        private Task<IEnumerable<WeatherForecast>> GetWeather(WeatherRequest weatherRequest)
        {
            return _messageBus.Send<WeatherRequest, IEnumerable<WeatherForecast>>(weatherRequest);
        }
    }
}