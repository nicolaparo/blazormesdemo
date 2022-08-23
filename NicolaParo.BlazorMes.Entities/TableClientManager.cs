using Azure.Data.Tables;
using System.Collections.Generic;

namespace NicolaParo.BlazorMes.Entities
{
    public class TableClientManager
    {
        private readonly string connectionString;
        private Dictionary<string, TableClient> tableClients = new();

        public TableClientManager(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public TableClient Alarms => GetTableClient("alarms");
        public TableClient Orders => GetTableClient("orders");
        public TableClient Telemetry => GetTableClient("telemetry");
        public TableClient Events => GetTableClient("events");

        private TableClient GetTableClient(string tableName)
        {
            if (!tableClients.TryGetValue(tableName, out var tableClient))
            {
                tableClient = new TableClient(connectionString, tableName);
                tableClient.CreateIfNotExists();
                tableClients[tableName] = tableClient;
            }
            return tableClient;
        }
    }
}
