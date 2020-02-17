using System.Collections.Generic;
using MediatR;

namespace WebApplication
{
    public class WeatherRequest : IRequest<IEnumerable<WeatherForecast>>
    {
        public WeatherRequest(string id)
        {
            Id = id;
        }

        public string Id { get; }
    }
}