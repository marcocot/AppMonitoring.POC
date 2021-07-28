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
