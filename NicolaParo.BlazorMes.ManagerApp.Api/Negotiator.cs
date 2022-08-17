using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.WebPubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NicolaParo.BlazorMes.ManagerApp.Api
{
    public class Negotiator
    {
        [FunctionName(nameof(NegotiateAlarms))]
        public static WebPubSubConnection NegotiateAlarms(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "alarms/negotiate")] HttpRequest req,
        [WebPubSubConnection(Hub = "alarms", Connection = "WebPubSubConnectionString")] WebPubSubConnection connection)
        {
            return connection;
        }

        [FunctionName(nameof(NegotiateTelemetry))]
        public static WebPubSubConnection NegotiateTelemetry(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "telemetry/negotiate")] HttpRequest req,
        [WebPubSubConnection(Hub = "telemetry", Connection = "WebPubSubConnectionString")] WebPubSubConnection connection)
        {
            return connection;
        }
    }
}
