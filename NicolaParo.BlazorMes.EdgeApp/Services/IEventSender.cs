namespace NicolaParo.BlazorMes.EdgeApp.Services
{
    public interface IEventSender
    {
        Task SendAsync(object payload);
    }

    public class MergedEventSender<TEventSender1, TEventSender2> : IEventSender where TEventSender1 : IEventSender where TEventSender2 : IEventSender
    {
        private readonly IEventSender[] senders;

        public MergedEventSender(TEventSender1 first, TEventSender2 second)
        {
            senders = new IEventSender[] { first, second };
        }

        public Task SendAsync(object payload)
        {
            return Task.WhenAll(senders.Select(s => s.SendAsync(payload)).ToArray());
        }
    }
}
