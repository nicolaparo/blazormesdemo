namespace NicolaParo.BlazorMes.Entities.Payloads
{
    public record EventPayload : BasePayload
    {
        public EventPayload() : base() { }
        public EventPayload(BasePayload original) : base(original) { }

        public override string Type => "Event";
        public string EventType { get; set; }
    }
}
