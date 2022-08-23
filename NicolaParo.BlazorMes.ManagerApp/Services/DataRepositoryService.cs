using NicolaParo.BlazorMes.ManagerApp.Extensions;
using NicolaParo.BlazorMes.Models;
using NicolaParo.BlazorMes.Models.Payloads;
using System.Text.Json;

namespace NicolaParo.BlazorMes.ManagerApp.Services
{
    public class DataRepositoryService
    {
        private readonly HttpClient client;

        public DataRepositoryService(string baseApiUri, string accesskey = null)
        {
            client = new HttpClient()
            {
                BaseAddress = new Uri(baseApiUri)
            };

            if (accesskey is not null)
                client.DefaultRequestHeaders.Add("x-functions-key", accesskey);
        }

        public async Task<string> CreateOrderAsync(string machineName, ProductionOrderData data)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"orders/{machineName}")
            {
                Content = new JsonContent(data)
            };
            var response = await client.SendAsync(request);
            var result = await response.Content.ReadAsJsonAsync<string>();

            return result;
        }
        public async Task UpdateOrderAsync(string machineName, string orderId, ProductionOrderData data)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"orders/{machineName}/{orderId}")
            {
                Content = new JsonContent(data)
            };
            var response = await client.SendAsync(request);
        }
        public async Task DeleteOrderAsync(string machineName, string orderId)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"orders/{machineName}/{orderId}");
            var response = await client.SendAsync(request);
        }
        public async Task<ProductionOrder> GetOrderByIdAsync(string machineName, string orderId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"orders/{machineName}/{orderId}");
            var response = await client.SendAsync(request);
            var result = await response.Content.ReadAsJsonAsync<ProductionOrder>();
            return result;
        }
        public async Task<ProductionOrder[]> ListOrdersAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"orders");
            var response = await client.SendAsync(request);
            var result = await response.Content.ReadAsJsonAsync<ProductionOrder[]>();
            return result;
        }

        public async Task<TelemetryPayload[]> ListTelemetryAsync(string machineName, string orderId, DateTimeOffset? fromDate = null, DateTimeOffset? toDate = null)
        {
            var builder = new QueryParamBuilder();

            builder.Add("machineName", machineName);
            builder.Add("orderId", orderId);

            if (fromDate.HasValue)
                builder.Add("fromDate", fromDate.ToString());

            if (toDate.HasValue)
                builder.Add("toDate", toDate.ToString());

            var request = new HttpRequestMessage(HttpMethod.Get, $"telemetry{builder.Build()}");
            var response = await client.SendAsync(request);
            var result = await response.Content.ReadAsJsonAsync<TelemetryPayload[]>();
            return result;
        }
        public async Task<AlarmPayload[]> ListAlarmsAsync(string machineName, string orderId, DateTimeOffset? fromDate = null, DateTimeOffset? toDate = null)
        {
            var builder = new QueryParamBuilder();

            builder.Add("machineName", machineName);
            builder.Add("orderId", orderId);

            if (fromDate.HasValue)
                builder.Add("fromDate", fromDate.ToString());

            if (toDate.HasValue)
                builder.Add("toDate", toDate.ToString());

            var request = new HttpRequestMessage(HttpMethod.Get, $"alarms{builder.Build()}");
            var response = await client.SendAsync(request);
            var result = await response.Content.ReadAsJsonAsync<AlarmPayload[]>();
            return result;
        }

        public class QueryParamBuilder
        {
            private readonly Dictionary<string, string> _fields = new();
            public QueryParamBuilder Add(string key, string value)
            {
                _fields.Add(key, value);
                return this;
            }
            public string Build()
            {
                return $"?{String.Join("&", _fields.Select(pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}"))}";
            }
            public static QueryParamBuilder New => new();
        }
    }

}
