using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using NicolaParo.BlazorMes.Entities.Models;
using Azure.Data.Tables;
using NicolaParo.BlazorMes.Entities;
using System.Collections.Generic;
using System.Linq;
using Azure;

namespace NicolaParo.BlazorMes.ManagerApp.Api
{
    public class OrdersApi
    {
        private readonly ILogger log;
        private readonly TableClient tableClient;

        public OrdersApi()
        {
            var connectionString = Environment.GetEnvironmentVariable("StorageConnectionString");
            tableClient = new TableClient(connectionString, "orders");
            tableClient.CreateIfNotExists();
        }

        private static async Task<T> ParseRequestBodyAsync<T>(HttpRequest req)
        {
            var rawBody = await req.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(rawBody);
        }
        private static string CreateOrderId()
        {
            return $"0{DateTimeOffset.Now.ToUnixTimeSeconds():X}";
        }

        [FunctionName(nameof(CreateOrderAsync))]
        public async Task<IActionResult> CreateOrderAsync([HttpTrigger(AuthorizationLevel.Function, "post", Route = "orders/{machineName}")] HttpRequest req, string machineName)
        {
            var body = await ParseRequestBodyAsync<ProductionOrderData>(req);

            var productionOrder = new ProductionOrder(body) with
            {
                Id = CreateOrderId(),
                MachineName = machineName,
                CreatedAt = DateTimeOffset.Now
            };

            await tableClient.AddEntityAsync(new ProductionOrderEntity(productionOrder));

            return new OkObjectResult(productionOrder.Id);
        }

        [FunctionName(nameof(UpdateOrderAsync))]
        public async Task<IActionResult> UpdateOrderAsync([HttpTrigger(AuthorizationLevel.Function, "put", Route = "orders/{machineName}/{orderId}")] HttpRequest req, string machineName, string orderId)
        {
            var body = await ParseRequestBodyAsync<ProductionOrderData>(req);

            var previousOrder = (await tableClient.GetEntityAsync<ProductionOrderEntity>(
                partitionKey: "", rowKey: ProductionOrderEntity.ComputeRowKey(machineName, orderId)
            )).Value;

            var productionOrder = new ProductionOrder(body) with
            {
                Id = previousOrder.Id,
                MachineName = previousOrder.MachineName,
                CreatedAt = previousOrder.CreatedAt,
                LastUpdatedAt = DateTimeOffset.Now
            };

            await tableClient.UpdateEntityAsync(new ProductionOrderEntity(productionOrder), ETag.All);

            return new OkObjectResult(productionOrder.Id);
        }

        [FunctionName(nameof(DeleteOrderAsync))]
        public async Task<IActionResult> DeleteOrderAsync([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "orders/{machineName}/{orderId}")] HttpRequest req, string machineName, string orderId)
        {
            await tableClient.DeleteEntityAsync(
                partitionKey: "", rowKey: ProductionOrderEntity.ComputeRowKey(machineName, orderId)
            );

            return new NoContentResult();
        }

        [FunctionName(nameof(GetOrdersAsync))]
        public async Task<IActionResult> GetOrdersAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "orders")] HttpRequest req)
        {
            var entities = await tableClient.QueryAsync<ProductionOrderEntity>()
                .AsPages()
                .Select(x => x.Values)
                .ToArrayAsync();

            return new OkObjectResult(entities.SelectMany(x => x).Select(x => ((ProductionOrder)x) with { }));
        }

        [FunctionName(nameof(GetOrderByIdAsync))]
        public async Task<IActionResult> GetOrderByIdAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "orders/{machineName}/{orderId}")] HttpRequest req, string machineName, string orderId)
        {
            var entity = (await tableClient.GetEntityAsync<ProductionOrderEntity>(
                partitionKey: "", rowKey: ProductionOrderEntity.ComputeRowKey(machineName, orderId)
            )).Value;
            return new OkObjectResult(((ProductionOrder)entity) with { });
        }

    }

}
