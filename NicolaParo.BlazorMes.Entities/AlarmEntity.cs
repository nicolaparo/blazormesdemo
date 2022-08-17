using Azure;
using Azure.Data.Tables;
using NicolaParo.BlazorMes.Entities.Payloads;

namespace NicolaParo.BlazorMes.Entities
{
    public record AlarmEntity : AlarmPayload, ITableEntity
    {
        public AlarmEntity(AlarmPayload data) : base(data) { }

        public string PartitionKey { get => $"{MachineName}_{ProductionOrder}"; set { } }
        public new DateTimeOffset? Timestamp { get => base.Timestamp; set => base.Timestamp = value; }
        public string RowKey { get => $"{PartitionKey}_{Timestamp:yyyyMMddHHmmssfff}"; set { } }
        public ETag ETag { get; set; }

        public static AlarmEntity FromPayload(AlarmPayload payload)
        {
            return new AlarmEntity(payload);
        }

    }
}