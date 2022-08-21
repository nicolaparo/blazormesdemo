namespace NicolaParo.BlazorMes.Entities.Payloads
{
    public record AlarmPayload : BasePayload
    {
        public AlarmPayload() : base() { }
        public AlarmPayload(BasePayload original) : base(original) { }

        public override string Type => "Alarm";
        public AlarmType AlarmType { get; set; }
    }


    public enum AlarmType
    {
        None,
        EmergencyPushButtonPressed,
        TemperatureAlertStart,
        TemperatureAlertEnd,
    }

}
