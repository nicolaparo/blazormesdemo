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
using Azure;
using NicolaParo.BlazorMes.Entities;
using NicolaParo.BlazorMes.Models;
using NicolaParo.BlazorMes.Models.Payloads;

namespace NicolaParo.BlazorMes.EventDispatcher
{
    public static partial class EventPersister
    {
        [FunctionName(nameof(EventPersister))]
        public static async Task PersistEventAsync(
            [ServiceBusTrigger("%ServiceBusTopicName%", "%ServiceBusPersisterSubscriptionName%", Connection = "ServiceBusConnectionString")] string rawpayload,
            ILogger log)
        {
            var payload = Payloads.DeserializePayload(rawpayload);

            if (payload is null)
                return;

            var connectionString = Environment.GetEnvironmentVariable("StorageConnectionString");

            var tableStorage = new TableClientManager(connectionString);

            ProductionOrderEntity order = null;

            try
            {
                order = (await tableStorage.Orders.GetEntityAsync<ProductionOrderEntity>("", ProductionOrderEntity.ComputeRowKey(payload.MachineName, payload.ProductionOrder))).Value;
            }
            catch (RequestFailedException e) { }

            var now = DateTimeOffset.UtcNow;

            var isNewOrder = order is null;
            if (isNewOrder)
            {
                order = new ProductionOrderEntity(new ProductionOrder()
                {
                    CreatedAt = now,
                    Id = payload.ProductionOrder,
                    MachineName = payload.MachineName,
                });
            }

            order.LastUpdatedAt = now;

            if (payload is TelemetryPayload telemetry)
            {
                await StoreEventHistoryAsync(tableStorage.Telemetry, TelemetryEntity.FromPayload(telemetry));

                if (isNewOrder)
                    order.StartedAt = now;

                order.UpdateWith(telemetry);
            }
            else if (payload is AlarmPayload alarm)
            {
                await StoreEventHistoryAsync(tableStorage.Alarms, AlarmEntity.FromPayload(alarm));

                if (isNewOrder)
                    order.StartedAt = now;

                order.UpdateWith(alarm);
            }
            else if (payload is EventPayload evt)
            {
                await StoreEventHistoryAsync(tableStorage.Events, EventEntity.FromPayload(evt));

                order.UpdateWith(evt);
            }
            else
            {
                return;
            }

            await tableStorage.Orders.UpsertEntityAsync(order);
        }

        private static async Task StoreEventHistoryAsync<T>(TableClient tableClient, T tableEntity) where T : ITableEntity
        {
            try
            {
                await tableClient.AddEntityAsync(tableEntity);
            }
            catch
            {
                try
                {
                    await tableClient.CreateIfNotExistsAsync();
                    await tableClient.AddEntityAsync(tableEntity);
                }
                catch
                {
                    throw;
                }
            }
        }
    }
}
