using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace NicolaParo.BlazorMes.EdgeApp.Services
{
    public class ServiceBusTopicEventSender : IEventSender
    {
        private readonly ServiceBusSender sender;

        public ServiceBusTopicEventSender(string serviceBusConnectionString, string topicName)
        {
            sender = new ServiceBusClient(serviceBusConnectionString).CreateSender(topicName);
        }

        public async Task SendAsync(object payload)
        {
            var message = new ServiceBusMessage(JsonSerializer.Serialize(payload));
            await sender.SendMessageAsync(message);
        }
    }
}
