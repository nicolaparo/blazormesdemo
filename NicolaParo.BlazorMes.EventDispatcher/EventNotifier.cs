using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Azure.Data.Tables;
using NicolaParo.BlazorMes.Entities;
using Microsoft.Azure.WebJobs.Extensions.WebPubSub;
using NicolaParo.BlazorMes.Models.Payloads;

namespace NicolaParo.BlazorMes.EventDispatcher
{
    public static class EventNotifier
    {
        [FunctionName(nameof(EventNotifier))]
        public static async Task NotifyEventAsync(
            [ServiceBusTrigger("%ServiceBusTopicName%", "%ServiceBusNotifierSubscriptionName%", Connection = "ServiceBusConnectionString")] string rawpayload,
            [WebPubSub(Hub = "alarms", Connection = "WebPubSubConnectionString")] IAsyncCollector<WebPubSubAction> alarms,
            [WebPubSub(Hub = "telemetry", Connection = "WebPubSubConnectionString")] IAsyncCollector<WebPubSubAction> telemetry,
            [WebPubSub(Hub = "events", Connection = "WebPubSubConnectionString")] IAsyncCollector<WebPubSubAction> events,
            ILogger log)
        {
            var payload = Payloads.DeserializePayload(rawpayload);

            if (payload is null)
                return;

            if (payload is AlarmPayload)
            {
                await alarms.AddAsync(WebPubSubAction.CreateSendToAllAction(rawpayload));
                await alarms.FlushAsync();
            }
            else if (payload is TelemetryPayload)
            {
                await telemetry.AddAsync(WebPubSubAction.CreateSendToAllAction(rawpayload));
                await telemetry.FlushAsync();
            }
            else if(payload is EventPayload)
            {
                await events.AddAsync(WebPubSubAction.CreateSendToAllAction(rawpayload));
                await events.FlushAsync();

            }
        }
    }
}
