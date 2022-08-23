using Azure;
using Azure.Data.Tables;
using NicolaParo.BlazorMes.Models.Payloads;

namespace NicolaParo.BlazorMes.Entities
{
    public record AlarmEntity : AlarmPayload, ITableEntity
    {
        public AlarmEntity() { }
        public AlarmEntity(AlarmPayload data) : base(data) { }

        public string PartitionKey { get => ComputePartitionKey(MachineName, ProductionOrder); set { } }
        public new DateTimeOffset? Timestamp { get => base.Timestamp; set => base.Timestamp = value; }
        public string RowKey { get => ComputeRowKey(MachineName, ProductionOrder, Timestamp); set { } }
        public ETag ETag { get; set; }

        public static string ComputePartitionKey(string machineName, string productionOrder)
        {
            return $"{machineName}_{productionOrder}";
        }
        public static string ComputeRowKey(string machineName, string productionOrder, DateTimeOffset? timestamp)
        {
            return $"{ComputePartitionKey(machineName, productionOrder)}_{timestamp:yyyyMMddHHmmssfff}";
        }

        public static AlarmEntity FromPayload(AlarmPayload payload)
        {
            return new AlarmEntity(payload);
        }

    }
}