using App.Metrics.ReservoirSampling.Uniform;
using App.Metrics.Timer;

namespace AppMonitoring.POC
{
    public static class MetricsRegistry
    {
        private const string Context = "Weather";

        public static readonly TimerOptions TimerWeatherForecastGeneration = new TimerOptions
        {
            Context = Context,
            Name = "weather-forecast-generation",
            Reservoir = () => new DefaultAlgorithmRReservoir(),
        };
    }
}