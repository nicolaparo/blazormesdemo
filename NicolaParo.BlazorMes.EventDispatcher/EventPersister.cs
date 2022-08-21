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
using NicolaParo.BlazorMes.Entities.Payloads;
using System.Collections.Generic;
using NicolaParo.BlazorMes.Entities.Models;
using Azure;

namespace NicolaParo.BlazorMes.EventDispatcher
{
    public static class EventPersister
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

                order.Goods += telemetry.Goods;
                order.Rejects += telemetry.Rejects;
            }
            else if (payload is AlarmPayload alarm)
            {
                await StoreEventHistoryAsync(tableStorage.Alarms, AlarmEntity.FromPayload(alarm));

                if (isNewOrder)
                    order.StartedAt = now;

                order.LastAlarmAt = now;
                order.LastAlarmType = alarm.AlarmType;
            }
            else if (payload is EventPayload evt)
            {
                if (evt.EventType is EventType.NewProductionOrder)
                {
                    order.StartedAt = now;
                }
            }
            else
            {
                return;
            }

            await tableStorage.Orders.UpsertEntityAsync(order);
        }

        public class TableClientManager
        {
            private readonly string connectionString;
            private Dictionary<string, TableClient> tableClients = new();

            public TableClientManager(string connectionString)
            {
                this.connectionString = connectionString;
            }

            public TableClient Alarms => GetTableClient("alarms");
            public TableClient Orders => GetTableClient("orders");
            public TableClient Telemetry => GetTableClient("telemetry");

            private TableClient GetTableClient(string tableName)
            {
                if (!tableClients.TryGetValue(tableName, out var tableClient))
                {
                    tableClient = new TableClient(connectionString, tableName);
                    tableClients[tableName] = tableClient;
                }
                return tableClient;
            }
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
