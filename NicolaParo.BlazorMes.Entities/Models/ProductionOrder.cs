using NicolaParo.BlazorMes.Entities.Payloads;

namespace NicolaParo.BlazorMes.Entities.Models
{
    public record ProductionOrder : ProductionOrderData
    {
        public ProductionOrder() { }
        public ProductionOrder(ProductionOrderData data) : base(data) { }

        public string Id { get; set; }
        public string MachineName { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? LastUpdatedAt { get; set; }
        public DateTimeOffset? StartedAt { get; set; }
        public int Goods { get; set; }
        public int Rejects { get; set; }
        public DateTimeOffset? LastAlarmAt { get; set; }
        public AlarmType LastAlarmType { get; set; }
    }
}