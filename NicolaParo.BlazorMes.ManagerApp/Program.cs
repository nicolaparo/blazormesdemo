using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using NicolaParo.BlazorMes.ManagerApp;
using NicolaParo.BlazorMes.ManagerApp.Services;
using NicolaParo.BlazorMes.Models.Payloads;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var accesskey = @"7Qf3DML1HgqGbjc-38Ttx9SkhcPqoqZZxbxianUg6WPWAzFuPIwlKQ==";

builder.Services.AddSingleton<DataRepositoryService>(_ => new("https://blazormes-managerappapi.azurewebsites.net/api/", accesskey));
builder.Services.AddSingleton<EventDataReceiver<AlarmPayload>>(_ => new("https://blazormes-managerappapi.azurewebsites.net/api/alarms/negotiate", accesskey));
builder.Services.AddSingleton<EventDataReceiver<TelemetryPayload>>(_ => new("https://blazormes-managerappapi.azurewebsites.net/api/telemetry/negotiate", accesskey));
builder.Services.AddSingleton<EventDataReceiver<EventPayload>>(_ => new("https://blazormes-managerappapi.azurewebsites.net/api/events/negotiate", accesskey));

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<BrowserInterop>();

var webAssemblyHost = builder.Build();

var cts = new CancellationTokenSource();

await Task.WhenAll(
    webAssemblyHost.Services.GetRequiredService<EventDataReceiver<AlarmPayload>>().ConnectAsync(cts.Token),
    webAssemblyHost.Services.GetRequiredService<EventDataReceiver<EventPayload>>().ConnectAsync(cts.Token),
    webAssemblyHost.Services.GetRequiredService<EventDataReceiver<TelemetryPayload>>().ConnectAsync(cts.Token)
);

await webAssemblyHost.RunAsync();
