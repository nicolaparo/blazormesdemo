using NicolaParo.BlazorMes.Entities.Models;
using NicolaParo.BlazorMes.ManagerApp.Extensions;
using System.Text.Json;

namespace NicolaParo.BlazorMes.ManagerApp.Services
{
    public class ProductionOrderRepositoryService
    {
        private readonly HttpClient client;

        public ProductionOrderRepositoryService(string baseApiUri)
        {
            client = new HttpClient()
            {
                BaseAddress = new Uri(baseApiUri)
            };
        }

        public async Task<string> CreateAsync(string machineName, ProductionOrderData data)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"orders/{machineName}")
            {
                Content = new JsonContent(data)
            };
            var response = await client.SendAsync(request);
            var result = await response.Content.ReadAsJsonAsync<string>();

            return result;
        }
        public async Task UpdateAsync(string machineName, string orderId, ProductionOrderData data)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"orders/{machineName}/{orderId}")
            {
                Content = new JsonContent(data)
            };
            var response = await client.SendAsync(request);
        }
        public async Task DeleteAsync(string machineName, string orderId)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"orders/{machineName}/{orderId}");
            var response = await client.SendAsync(request);
        }
        public async Task<ProductionOrder> GetByIdAsync(string machineName, string orderId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"orders/{machineName}/{orderId}");
            var response = await client.SendAsync(request);
            var result = await response.Content.ReadAsJsonAsync<ProductionOrder>();
            return result;
        }
        public async Task<ProductionOrder[]> ListAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"orders");
            var response = await client.SendAsync(request);
            var result = await response.Content.ReadAsJsonAsync<ProductionOrder[]>();
            return result;
        }
    }

}
