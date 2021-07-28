using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Metrics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AppMonitoring.POC.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IMetrics _metrics;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IMetrics metrics)
        {
            _logger = logger;
            _metrics = metrics;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("{city:alpha}")]
        public WeatherForecast GetByCity(string city)
        {
            this._logger.LogInformation("Weather requested for: {City}", city);
            return new WeatherForecast
            {
                Date = DateTime.Now,
                TemperatureC = 40,
                Summary = Summaries[0]
            };
        }

        [HttpGet("{country}/{city}")]
        public async Task<WeatherForecast> GetByCountryCity(string country, string city)
        {
            using (_metrics.Measure.Timer.Time(MetricsRegistry.TimerWeatherForecastGeneration))
            {
                await Task.Delay(TimeSpan.FromSeconds(2), HttpContext.RequestAborted);
            }

            return new WeatherForecast
            {
                Date = DateTime.Now,
                Summary = Summaries[0],
                TemperatureC = 45
            };
        }

        [HttpGet("crash")]
        public void CrashIt()
        {
            throw new ApplicationException("Nothing is working");
        }
    }
}
