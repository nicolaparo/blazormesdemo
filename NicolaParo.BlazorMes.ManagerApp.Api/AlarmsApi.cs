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
    public class AlarmsApi
    {
        private readonly TableClientManager tableManager;

        public AlarmsApi()
        {
            var connectionString = Environment.GetEnvironmentVariable("StorageConnectionString");
            tableManager = new TableClientManager(connectionString);
        }

        [FunctionName(nameof(GetAlarmsAsync))]
        public async Task<IActionResult> GetAlarmsAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "alarms")] HttpRequest req)
        {
            string machineName = req.Query[nameof(machineName)];
            string orderId = req.Query[nameof(orderId)];
            DateTimeOffset? fromDate = ConversionHelpers.ConvertToDateTimeOffset(req.Query[nameof(fromDate)]);
            DateTimeOffset? toDate = ConversionHelpers.ConvertToDateTimeOffset(req.Query[nameof(toDate)]);

            var filterBuilder = new StringBuilder();
            filterBuilder.Append($"PartitionKey eq '{AlarmEntity.ComputePartitionKey(machineName, orderId)}'");

            if (fromDate.HasValue)
                filterBuilder.Append($" and RowKey ge '{AlarmEntity.ComputeRowKey(machineName, orderId, fromDate)}'");

            if (toDate.HasValue)
                filterBuilder.Append($" and RowKey le '{AlarmEntity.ComputeRowKey(machineName, orderId, toDate)}'");

            var entities = await tableManager.Alarms.QueryAsync<AlarmEntity>(filterBuilder.ToString())
                .AsPages()
                .Select(x => x.Values)
                .ToArrayAsync();

            return Responses.Ok(entities.SelectMany(x => x).Select(x => ((AlarmPayload)x) with { }));
        }
    }

}
