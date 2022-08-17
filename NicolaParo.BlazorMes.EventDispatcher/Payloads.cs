using NicolaParo.BlazorMes.Entities.Payloads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NicolaParo.BlazorMes.EventDispatcher
{
    public static class Payloads
    {
        public static BasePayload DeserializePayload(string serializedPayload)
        {
            var payload = JsonSerializer.Deserialize<JsonElement>(serializedPayload);

            try
            {
                var targetType = payload.GetProperty("Type").GetString() switch
                {
                    "Telemetry" => typeof(TelemetryPayload),
                    "Event" => typeof(EventPayload),
                    "Alarm" => typeof(AlarmPayload),
                    _ => null
                };

                if (targetType is not null)
                    return (BasePayload)JsonSerializer.Deserialize(serializedPayload, targetType);

            }
            catch { }

            return null;
        }
    }
}
