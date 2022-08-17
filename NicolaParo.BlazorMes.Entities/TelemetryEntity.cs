using Azure;
using Azure.Data.Tables;
using NicolaParo.BlazorMes.Entities.Payloads;

namespace NicolaParo.BlazorMes.Entities
{
    public record TelemetryEntity : TelemetryPayload, ITableEntity
    {
        public TelemetryEntity(TelemetryPayload data) : base(data) { }

        public string PartitionKey { get => $"{MachineName}_{ProductionOrder}"; set { } }
        public new DateTimeOffset? Timestamp { get => base.Timestamp; set => base.Timestamp = value; }
        public string RowKey { get => $"{PartitionKey}_{Timestamp:yyyyMMddHHmmssfff}"; set { } }
        public ETag ETag { get; set; }

        public static TelemetryEntity FromPayload(TelemetryPayload payload)
        {
            return new TelemetryEntity(payload);
        }
    }
}