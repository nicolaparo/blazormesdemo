using Microsoft.Azure.Devices.Client;
using System.Text;
using System.Text.Json;

namespace NicolaParo.BlazorMes.EdgeApp.Services
{

    public class IotHubEventSender : IEventSender
    {
        private readonly DeviceClient deviceClient;

        public IotHubEventSender(string iotEdgeDeviceConnectionString)
        {
            deviceClient = DeviceClient.CreateFromConnectionString(iotEdgeDeviceConnectionString);
        }

        public async Task SendAsync(object payload)
        {
            var message = CreateMessage(payload);
            await deviceClient.SendEventAsync(message);
        }

        private static Message CreateMessage(object payload)
        {
            var serializedPayload = JsonSerializer.Serialize(payload);
            var bytes = Encoding.UTF8.GetBytes(serializedPayload);
            return new Message(bytes);
        }
    }

}
