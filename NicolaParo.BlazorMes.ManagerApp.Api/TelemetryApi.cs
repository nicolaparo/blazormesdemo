using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Azure.Data.Tables;
using NicolaParo.BlazorMes.Entities;
using System.Linq;
using System.Text;
using NicolaParo.BlazorMes.Models.Payloads;

namespace NicolaParo.BlazorMes.ManagerApp.Api
{
    public class TelemetryApi
    {
        private readonly TableClientManager tableManager;

        public TelemetryApi()
        {
            var connectionString = Environment.GetEnvironmentVariable("StorageConnectionString");
            tableManager = new TableClientManager(connectionString);
        }

        [FunctionName(nameof(GetTelemetryAsync))]
        public async Task<IActionResult> GetTelemetryAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "telemetry")] HttpRequest req)
        {
            string machineName = req.Query[nameof(machineName)];
            string orderId = req.Query[nameof(orderId)];
            DateTimeOffset? fromDate = ConversionHelpers.ConvertToDateTimeOffset(req.Query[nameof(fromDate)]);
            DateTimeOffset? toDate = ConversionHelpers.ConvertToDateTimeOffset(req.Query[nameof(toDate)]);

            var filterBuilder = new StringBuilder();
            filterBuilder.Append($"PartitionKey eq '{TelemetryEntity.ComputePartitionKey(machineName, orderId)}'");

            if (fromDate.HasValue)
                filterBuilder.Append($" and RowKey ge '{TelemetryEntity.ComputeRowKey(machineName, orderId, fromDate)}'");

            if (toDate.HasValue)
                filterBuilder.Append($" and RowKey le '{TelemetryEntity.ComputeRowKey(machineName, orderId, toDate)}'");

            var entities = await tableManager.Telemetry.QueryAsync<TelemetryEntity>(filterBuilder.ToString())
                .AsPages()
                .Select(x => x.Values)
                .ToArrayAsync();

            return Responses.Ok(entities.SelectMany(x => x).Select(x => ((TelemetryPayload)x) with { }));
        }
    }

}
