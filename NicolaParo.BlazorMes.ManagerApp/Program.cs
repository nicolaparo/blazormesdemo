using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using NicolaParo.BlazorMes.Entities.Payloads;
using NicolaParo.BlazorMes.ManagerApp;
using NicolaParo.BlazorMes.ManagerApp.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<ProductionOrderRepositoryService>(_ => new("http://localhost:7071/api/"));
builder.Services.AddSingleton<EventDataReceiver<AlarmPayload>>(_ => new("http://localhost:7071/api/alarms/negotiate"));
builder.Services.AddSingleton<EventDataReceiver<TelemetryPayload>>(_ => new("http://localhost:7071/api/telemetry/negotiate"));

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

var webAssemblyHost = builder.Build();

var cts = new CancellationTokenSource();

await Task.WhenAll(
    webAssemblyHost.Services.GetRequiredService<EventDataReceiver<AlarmPayload>>().ConnectAsync(cts.Token),
    webAssemblyHost.Services.GetRequiredService<EventDataReceiver<TelemetryPayload>>().ConnectAsync(cts.Token)
);

await webAssemblyHost.RunAsync();
