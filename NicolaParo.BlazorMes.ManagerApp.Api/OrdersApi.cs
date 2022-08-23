using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Azure.Data.Tables;
using NicolaParo.BlazorMes.Entities;
using System.Collections.Generic;
using System.Linq;
using Azure;
using NicolaParo.BlazorMes.Models;

namespace NicolaParo.BlazorMes.ManagerApp.Api
{
    public class OrdersApi
    {
        private readonly TableClientManager tableManager;
        public OrdersApi()
        {
            var connectionString = Environment.GetEnvironmentVariable("StorageConnectionString");
            tableManager = new TableClientManager(connectionString);
        }

        private static async Task<T> ParseRequestBodyAsync<T>(HttpRequest req)
        {
            var rawBody = await req.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(rawBody);
        }
        private static string CreateOrderId()
        {
            return $"ORD{DateTimeOffset.Now.ToUnixTimeSeconds():X}";
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

            await tableManager.Orders.AddEntityAsync(new ProductionOrderEntity(productionOrder));

            return Responses.Ok(productionOrder.Id);
        }

        [FunctionName(nameof(UpdateOrderAsync))]
        public async Task<IActionResult> UpdateOrderAsync([HttpTrigger(AuthorizationLevel.Function, "put", Route = "orders/{machineName}/{orderId}")] HttpRequest req, string machineName, string orderId)
        {
            var body = await ParseRequestBodyAsync<ProductionOrderData>(req);

            var previousOrder = (await tableManager.Orders.GetEntityAsync<ProductionOrderEntity>(
                partitionKey: "", rowKey: ProductionOrderEntity.ComputeRowKey(machineName, orderId)
            )).Value;

            previousOrder.LastUpdatedAt = DateTimeOffset.Now;
            previousOrder.CustomerName = body.CustomerName;
            previousOrder.ItemsCount = body.ItemsCount;

            await tableManager.Orders.UpdateEntityAsync(new ProductionOrderEntity(previousOrder), ETag.All);

            return Responses.Ok(previousOrder.Id);
        }

        [FunctionName(nameof(DeleteOrderAsync))]
        public async Task<IActionResult> DeleteOrderAsync([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "orders/{machineName}/{orderId}")] HttpRequest req, string machineName, string orderId)
        {
            await tableManager.Orders.DeleteEntityAsync(
                partitionKey: "", rowKey: ProductionOrderEntity.ComputeRowKey(machineName, orderId)
            );

            return Responses.NoContent();
        }

        [FunctionName(nameof(GetOrdersAsync))]
        public async Task<IActionResult> GetOrdersAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "orders")] HttpRequest req)
        {
            var entities = await tableManager.Orders.QueryAsync<ProductionOrderEntity>()
                .AsPages()
                .Select(x => x.Values)
                .ToArrayAsync();

            return Responses.Ok(entities.SelectMany(x => x).Select(x => ((ProductionOrder)x) with { }));
        }

        [FunctionName(nameof(GetOrderByIdAsync))]
        public async Task<IActionResult> GetOrderByIdAsync([HttpTrigger(AuthorizationLevel.Function, "get", Route = "orders/{machineName}/{orderId}")] HttpRequest req, string machineName, string orderId)
        {
            var entity = (await tableManager.Orders.GetEntityAsync<ProductionOrderEntity>(
                partitionKey: "", rowKey: ProductionOrderEntity.ComputeRowKey(machineName, orderId)
            )).Value;
            return Responses.Ok(((ProductionOrder)entity) with { });
        }

    }

}
