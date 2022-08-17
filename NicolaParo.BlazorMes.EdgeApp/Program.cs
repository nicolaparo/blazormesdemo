using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using NicolaParo.BlazorMes.EdgeApp;
using NicolaParo.BlazorMes.EdgeApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<FactorySensors>(); 
builder.Services.AddSingleton<ServiceBusTopicEventSender>(_ => new ServiceBusTopicEventSender(builder.Configuration["ServiceBusConnectionString"], builder.Configuration["ServiceBusTopicName"]));
builder.Services.AddSingleton<ConsoleOutEventSender>();
builder.Services.AddSingleton<IEventSender, MergedEventSender<ServiceBusTopicEventSender, ConsoleOutEventSender>>();
builder.Services.AddSingleton<ReportingService>();

var app = builder.Build();

app.UseExceptionHandler("/Error");
app.UseHsts();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

var sensors = app.Services.GetRequiredService<FactorySensors>();
var reporter = app.Services.GetRequiredService<ReportingService>();

sensors?.Start();
reporter?.Start();
app.Run();

await Task.WhenAll(sensors?.StopAsync(), reporter?.StopAsync());
