namespace NicolaParo.BlazorMes.Models.Payloads
{
    public record TelemetryPayload : BasePayload
    {
        public TelemetryPayload() : base() { }
        public TelemetryPayload(BasePayload original) : base(original) { }

        public override string Type => "Telemetry";
        public int Items { get; set; }
        public int Goods { get; set; }
        public int Rejects { get; set; }
        public double TemperatureCelsius { get; set; }
        public double ConsumptionKW { get; set; }
        public int TimespanSeconds { get; set; }
    }
}
