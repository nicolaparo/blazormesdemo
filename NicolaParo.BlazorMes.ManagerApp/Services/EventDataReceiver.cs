using System.Text.Json;

namespace NicolaParo.BlazorMes.ManagerApp.Services
{
    public class EventDataReceiver<TEventData> : IDisposable
    {
        private readonly WebPubSubClient pubsubclient;

        public EventDataReceiver(string negotiatorFunctionUri)
        {
            pubsubclient = new WebPubSubClient(negotiatorFunctionUri);
            pubsubclient.OnMessage += HandleMessageAsync;
        }

        private async Task HandleMessageAsync(string message)
        {
            TEventData eventData = default;
            bool parsed = false;
            try
            {
                eventData = JsonSerializer.Deserialize<TEventData>(message);
                parsed = true;
            }
            catch { }

            if (parsed && OnEvent is not null)
                await OnEvent.Invoke(eventData);
        }

        public event Func<TEventData, Task> OnEvent;

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            await pubsubclient.ConnectAsync(cancellationToken);
        }

        public void Dispose()
        {
            pubsubclient.OnMessage -= HandleMessageAsync;
            pubsubclient.Dispose();
        }
    }
}
