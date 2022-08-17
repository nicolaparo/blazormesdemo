using System.Text.Json;

namespace NicolaParo.BlazorMes.EdgeApp.Services
{
    public class ConsoleOutEventSender : IEventSender
    {
        public Task SendAsync(object payload)
        {
            Console.WriteLine($"[{DateTimeOffset.Now}] - {JsonSerializer.Serialize(payload)}");
            return Task.CompletedTask;
        }
    }

}
