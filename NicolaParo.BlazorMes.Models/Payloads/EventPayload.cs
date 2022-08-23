namespace NicolaParo.BlazorMes.Models.Payloads
{
    public record EventPayload : BasePayload
    {
        public EventPayload() : base() { }
        public EventPayload(BasePayload original) : base(original) { }

        public override string Type => "Event";
        public EventType EventType { get; set; }
    }
}
