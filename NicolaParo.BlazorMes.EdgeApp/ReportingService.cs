using Microsoft.Azure.Devices.Client;
using NicolaParo.BlazorMes.EdgeApp.Services;
using NicolaParo.BlazorMes.Models;
using NicolaParo.BlazorMes.Models.Payloads;
using System.Text;
using System.Text.Json;

namespace NicolaParo.BlazorMes.EdgeApp
{
    public class ReportingService
    {
        private readonly FactorySensors sensors;
        private readonly IEventSender eventSender;

        public ReportingService(FactorySensors sensors, IEventSender eventSender)
        {
            this.sensors = sensors;
            this.eventSender = eventSender;
            this.sensors.OnAlarm += OnAlarmAsync;
            this.sensors.OnEvent += OnEventAsync;
        }

        public TimeSpan ReportingFrequency { get; set; } = TimeSpan.FromSeconds(5);

        private CancellationTokenSource cts = new();
        private Task backgroundWorker;

        private int lastReportedTotalPieces;
        private int lastReportedTotalGoods;
        private int lastReportedTotalRejects;

        private async Task OnAlarmAsync(AlarmType alarmType)
        {
            var payload = new AlarmPayload
            {
                Timestamp = DateTimeOffset.UtcNow,
                AlarmType = alarmType,
                ProductionOrder = sensors.ProductionOrder,
                MachineName = sensors.MachineName,
            };

            await eventSender.SendAsync(payload);
        }
        private async Task OnEventAsync(EventType eventType)
        {
            if (eventType is EventType.NewProductionOrder)
            {
                lastReportedTotalPieces = 0;
                lastReportedTotalGoods = 0;
                lastReportedTotalRejects = 0;
            }

            var payload = new EventPayload
            {
                Timestamp = DateTimeOffset.UtcNow,
                EventType = eventType,
                ProductionOrder = sensors.ProductionOrder,
                MachineName = sensors.MachineName,
            };

            await eventSender.SendAsync(payload);
        }
        private async Task ReportAsync()
        {
            if (!sensors.HasData())
                return;

            var totalPieces = sensors.TotalPieces;
            var totalGoods = sensors.TotalGoods;
            var totalRejects = sensors.TotalRejects;

            var payload = new TelemetryPayload
            {
                Timestamp = DateTimeOffset.UtcNow,
                Items = totalPieces - lastReportedTotalPieces,
                Goods = totalGoods - lastReportedTotalGoods,
                Rejects = totalRejects - lastReportedTotalRejects,
                ProductionOrder = sensors.ProductionOrder,
                MachineName = sensors.MachineName,
                TemperatureCelsius = sensors.TemperatureCelsius,
                ConsumptionKW = sensors.ConsumptionKW,
                TimespanSeconds = (int)ReportingFrequency.TotalSeconds,
            };

            lastReportedTotalPieces = totalPieces;
            lastReportedTotalGoods = totalGoods;
            lastReportedTotalRejects = totalRejects;

            await eventSender.SendAsync(payload);

        }

        public void Start()
        {
            backgroundWorker = RunAsync(cts.Token);
        }
        public async Task StopAsync()
        {
            cts.Cancel();
            await backgroundWorker;
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.WhenAll(
                    Task.Delay(ReportingFrequency, cancellationToken),
                    ReportAsync()
                );
            }
        }
    }

}
