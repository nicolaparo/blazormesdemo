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

            if (payload is TelemetryPayload telemetry)
            {
                var telemetryTable = "telemetry";
                var tableClient = new TableClient(connectionString, telemetryTable);
                var entity = TelemetryEntity.FromPayload(telemetry);
                await SaveDataAsync(tableClient, entity);
            }
            else if (payload is AlarmPayload alarm)
            {
                var alarmTable = "alarms";
                var tableClient = new TableClient(connectionString, alarmTable);
                var entity = AlarmEntity.FromPayload(alarm);
                await SaveDataAsync(tableClient, entity);
            }
        }

        private static async Task SaveDataAsync<T>(TableClient tableClient, T tableEntity) where T : ITableEntity
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
