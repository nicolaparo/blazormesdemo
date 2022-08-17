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
    }
}