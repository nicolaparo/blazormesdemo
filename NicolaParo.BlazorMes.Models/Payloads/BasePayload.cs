namespace NicolaParo.BlazorMes.Models.Payloads
{
    public abstract record BasePayload
    {
        public abstract string Type { get; }
        public DateTimeOffset? Timestamp { get; set; }
        public string MachineName { get; set; }
        public string ProductionOrder { get; set; }
    }
}
