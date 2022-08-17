using Azure;
using Azure.Data.Tables;

namespace NicolaParo.BlazorMes.Entities.Models
{
    public record ProductionOrderData
    {
        public string CustomerName { get; set; }
        public int ItemsCount { get; set; }
    }

}