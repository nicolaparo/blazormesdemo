﻿namespace NicolaParo.BlazorMes.Entities.Payloads
{
    public record AlarmPayload : BasePayload
    {
        public AlarmPayload() : base() { }
        public AlarmPayload(BasePayload original) : base(original) { }

        public override string Type => "Alarm";
        public string AlarmType { get; set; }
    }
}
