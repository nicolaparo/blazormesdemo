using NicolaParo.BlazorMes.Models.Payloads;

namespace NicolaParo.BlazorMes.Models
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
        public DateTimeOffset? CompletedAt { get; set; }
        public int Goods { get; set; }
        public int Rejects { get; set; }
        public int TotalProducedItems => Goods + Rejects;
        public DateTimeOffset? LastAlarmAt { get; set; }
        public AlarmType LastAlarmType { get; set; }
        public int MetricsSamplesCount { get; set; }
        public double? MinTemperatureCelsius { get; set; }
        public double? MaxTemperatureCelsius { get; set; }
        public double AverageTemperatureCelsius { get; set; }
        public double AverageConsumptionKW { get; set; }
        public int ActualProductionTimeSeconds { get; set; }

        public TimeSpan GetProductionTime()
        {
            return TimeSpan.FromSeconds(ActualProductionTimeSeconds);
        }
        public TimeSpan GetDuration()
        {
            if (!StartedAt.HasValue)
                return TimeSpan.Zero;

            if (!CompletedAt.HasValue)
                return DateTimeOffset.UtcNow - StartedAt.Value;

            return CompletedAt.Value - StartedAt.Value;
        }

        public double GetEfficiencyPercentage()
        {
            if (GetDuration().TotalSeconds <= 0)
                return 0;

            return GetProductionTime() / GetDuration() * 100;
        }

        public float GetProgressPercentage()
        {
            if (Goods <= 0)
                return 0;

            return Goods * 100f / ItemsCount;
        }
        public float GetWastePercentage()
        {
            return Rejects * 100f / TotalProducedItems;
        }

        public double GetRawMaterialCost() => 0.6 * TotalProducedItems;
        public double GetEnergyCost() => 0.3 * AverageConsumptionKW * GetProductionTime().TotalHours;
        public double GetEmployeesCost() => 1200 * 3 * GetDuration() / TimeSpan.FromHours(8) / 22;

        public double GetTotalCost()
        {
            return GetRawMaterialCost() + GetEnergyCost() + GetEmployeesCost();
        }

        public void UpdateWith(TelemetryPayload telemetry)
        {
            Goods += telemetry.Goods;
            Rejects += telemetry.Rejects;

            if (!MinTemperatureCelsius.HasValue || telemetry.TemperatureCelsius < MinTemperatureCelsius.Value)
                MinTemperatureCelsius = telemetry.TemperatureCelsius;

            if (!MaxTemperatureCelsius.HasValue || telemetry.TemperatureCelsius > MaxTemperatureCelsius.Value)
                MaxTemperatureCelsius = telemetry.TemperatureCelsius;

            AverageTemperatureCelsius = (AverageTemperatureCelsius * MetricsSamplesCount + telemetry.TemperatureCelsius) / (MetricsSamplesCount + 1);
            AverageConsumptionKW = (AverageConsumptionKW * MetricsSamplesCount + telemetry.ConsumptionKW) / (MetricsSamplesCount + 1);

            ActualProductionTimeSeconds += telemetry.TimespanSeconds;

            MetricsSamplesCount++;
        }
        public void UpdateWith(AlarmPayload alarmPayload)
        {
            LastAlarmAt = alarmPayload.Timestamp;
            LastAlarmType = alarmPayload.AlarmType;
        }
        public void UpdateWith(EventPayload eventPayload)
        {
            if (eventPayload.EventType is EventType.NewProductionOrder && !StartedAt.HasValue)
            {
                StartedAt = eventPayload.Timestamp;
            }
            else if (eventPayload.EventType is EventType.ProductionStarted && Goods > ItemsCount)
            {
                CompletedAt = null;
            }
            else if (eventPayload.EventType is EventType.ProductionHalted && Goods > ItemsCount)
            {
                CompletedAt = eventPayload.Timestamp;
            }
        }

        public OrderStatus GetOrderStatus()
        {
            if (!StartedAt.HasValue)
                return OrderStatus.Idle;
            if (!CompletedAt.HasValue)
                return OrderStatus.InProgress;
            return OrderStatus.Completed;
        }

        public enum OrderStatus
        {
            None,
            Idle,
            InProgress,
            Completed
        }
    }
}