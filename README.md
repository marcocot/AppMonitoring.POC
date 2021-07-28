# AppMonitoring - POC

The purpose of this repository is to demonstrate an example of a backend 
application that implements some modern traceability and observability solutions.

## Structured Logging - Serilog

To add `Serilog` to the solution:

1. Add the following packages to the project:
    - `Serilog.AspNetCore`
    - `Serilog.Enrichers.Environment`
    - `Serilog.Enrichers.Process`
    - `Serilog.Enrichers.Thread`
    - `Serilog.Settings.Configuration`
    - `Serilog.Sinks.File`
2. `Serilog`, thanks to `Serilog.Settings.Configuration`, can be configured via 
   `appsettings.json`, e.g:
```json
"Serilog": {
   "Using": [],
   "MinimumLevel": {
      "Default": "Information",
      "Override": {
         "Microsoft": "Warning",
         "System": "Warning"
      }
   },
   "WriteTo": [
      {
      "Name": "File",
      "Args": {
         "path": "logs/log.json",
         "formatter": "Serilog.Formatting.Compact.RenderedCompactJsonFormatter, Serilog.Formatting.Compact"
         }
      }
   ],
   "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithProcessId",
      "WithThreadId"
   ],
   "Properties": {
      "ApplicationName": "AppName"
   }
}
```
   
3. If you want to activate `AspNetCore` request tracking you just need
   to activate the middleware:
```c#
public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
 ...
 app.UseSerilogRequestLogging();
...
}
```

4. Enable `Serilog` configuration:
```c#
public static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
          .ConfigureWebHostDefaults(webBuilder =>
          {
              webBuilder.UseStartup<Startup>();
          }).UseSerilog((hostingContext, services, configuration) => {
              configuration.ReadFrom.Configuration(hostingContext.Configuration);
          }, writeToProviders: true);
}
```

5. At this point you just need to consume the `ILogger<>` interface of dotnet:
```c#
public class MyController(ILogger<MyController> logger) {
  logger.LogError("Everything is on fire");
}
```

## AppMetrics

To add `AppMetrics` to the solution:

1. Add the following packages to the project:
  - `App.Metrics.AspNetCore.All`
  - `App.Metrics.Prometheus` (or any other reporting library)
2. Configure the library via `appsettings.json`:
```json
{
 "MetricsOptions": {
    "DefaultContextLabel": "MyMvcApplication",
    "Enabled": true
  },
  "MetricEndpointsOptions": {
    "MetricsEndpointEnabled": true,
    "MetricsTextEndpointEnabled": true,
    "EnvironmentInfoEndpointEnabled": true
   }
}
```
3. Enable the library:
```c#
// Startup.cs

...
services.AddMvcCore(options => options.EnableEndpointRouting = false).AddMetricsCore();
...
app.UseMvc(); 
```
4. Create a `MetricsRegistry` to centralise the definition of the metrics to be tracked:
```c#
//MetricsRegistry.cs
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
```

5. At this point all you have to do is pass an instance of `IMetrics` to your class and consume the metrics you are interested in:

```c#
public class MyController 
{
     public void GetByCountryCity([FromService]IMetrics metrics)
     {
         using (metrics.Measure.Timer.Time(MetricsRegistry.TimerWeatherForecastGeneration))
         {
             await MyVeryLongMethod();
         }
     }    
}
```