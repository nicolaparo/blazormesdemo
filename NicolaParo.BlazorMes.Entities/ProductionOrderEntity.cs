using Azure;
using Azure.Data.Tables;
using NicolaParo.BlazorMes.Entities.Models;
using NicolaParo.BlazorMes.Entities.Payloads;

namespace NicolaParo.BlazorMes.Entities
{
    public record ProductionOrderEntity : ProductionOrder, ITableEntity
    {
        public ProductionOrderEntity() { }
        public ProductionOrderEntity(ProductionOrder data) : base(data) { }

        public string PartitionKey { get => ""; set { } }
        public string RowKey { get => ComputeRowKey(MachineName, Id); set { } }
        public DateTimeOffset? Timestamp { get => CreatedAt; set { } }
        public ETag ETag { get; set; }

        public static string ComputeRowKey(string machineName, string orderId)
        {
            return $"{machineName}_{orderId}";
        }
    }
}