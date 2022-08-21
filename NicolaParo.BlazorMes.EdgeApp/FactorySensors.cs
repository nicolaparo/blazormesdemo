using NicolaParo.BlazorMes.Entities.Payloads;

namespace NicolaParo.BlazorMes.EdgeApp
{
    public class FactorySensors
    {
        private string productionOrder;
        private bool productionOrderSet;
        public string ProductionOrder
        {
            get => productionOrder;
            set
            {
                if (CanSetProductionOrder() && productionOrder != value)
                {
                    productionOrder = value;
                    productionOrderSet = true;
                }
            }
        }

        public string MachineName { get; set; }

        public bool CanSetProductionOrder()
        {
            return CurrentItemsPerMinute <= 0;
        }
        public bool IsRunning() => CurrentItemsPerMinute > 0;
        public bool HasData() => CurrentFanSpeedRpm > 0 || CurrentItemsPerMinute > 0;

        public int ItemsPerMinute { get; set; }
        public int FanSpeedRpm { get; set; }
        public TimeSpan UpdateFrequency { get; set; } = TimeSpan.FromSeconds(1 / 15d);
        public int TotalPieces => (int)currentTotalPieces;
        public int TotalRejects { get; private set; }
        public int TotalGoods => TotalPieces - TotalRejects;

        public double TemperatureCelsius => CurrentItemsPerMinute / 7f + 24 - CurrentFanSpeedRpm / 250f;
        public double ConsumptionKW => CurrentItemsPerMinute / 25f + CurrentFanSpeedRpm / 50f;
        private double currentTotalPieces;

        public double CurrentItemsPerMinute { get; private set; }
        public double CurrentFanSpeedRpm { get; private set; }

        public event Func<Task> OnUpdated;
        public event Func<AlarmType, Task> OnAlarm;
        public event Func<EventType, Task> OnEvent;

        public bool IsSafeTemperature()
        {
            return TemperatureCelsius >= 20 && TemperatureCelsius <= 40;
        }
        public bool IsDangerTemperature()
        {
            return TemperatureCelsius <= 10 || TemperatureCelsius >= 50;
        }
        private bool wasInDanger;
        private bool wasRunning;

        public async Task UpdateAsync()
        {
            CurrentItemsPerMinute = GoCloserTo(CurrentItemsPerMinute, ItemsPerMinute, 10 * UpdateFrequency.TotalSeconds, 20 * UpdateFrequency.TotalSeconds);
            CurrentFanSpeedRpm = GoCloserTo(CurrentFanSpeedRpm, FanSpeedRpm, 100 * UpdateFrequency.TotalSeconds, 500 * UpdateFrequency.TotalSeconds);

            if (IsRunning() && productionOrderSet)
            {
                productionOrderSet = false;
                if (OnEvent != null)
                    await OnEvent.Invoke(EventType.NewProductionOrder);
            }

            if (IsRunning() && !wasRunning)
            {
                if (OnEvent != null)
                    await OnEvent.Invoke(EventType.ProductionStarted);
            }
            if (!IsRunning() && wasRunning)
            {
                if (OnEvent != null)
                    await OnEvent.Invoke(EventType.ProductionHalted);
            }

            if (IsDangerTemperature() && !wasInDanger)
            {
                await NotifyAlarmAsync(AlarmType.TemperatureAlertStart);
                wasInDanger = true;
            }

            if (IsSafeTemperature() && wasInDanger)
            {
                await NotifyAlarmAsync(AlarmType.TemperatureAlertEnd);
                wasInDanger = false;
            }

            var producedPieces = CurrentItemsPerMinute * UpdateFrequency.TotalSeconds / 60;
            var newPieces = ((int)(currentTotalPieces + producedPieces)) - ((int)currentTotalPieces);

            for (var i = 0; i < newPieces; i++)
            {
                if (IsSafeTemperature() && Random.Shared.NextDouble() * 100 < 1)
                    TotalRejects++;
                else if (IsDangerTemperature() && Random.Shared.NextDouble() * 100 < 40)
                    TotalRejects++;
                else if (Random.Shared.NextDouble() * 100 < 10)
                    TotalRejects++;
            }

            currentTotalPieces += producedPieces;

            if (OnUpdated is not null)
                await OnUpdated.Invoke();

            wasRunning = IsRunning();

        }

        private async Task NotifyAlarmAsync(AlarmType alarmType)
        {
            if (OnAlarm is not null)
                await OnAlarm.Invoke(alarmType);
        }

        private static double GoCloserTo(double value, double targetValue, double increment, double decrement)
        {
            if (value < targetValue)
            {
                if (value + increment > targetValue)
                    return targetValue;
                else
                    return value + increment;
            }
            else
            {
                if (value - decrement < targetValue)
                    return targetValue;
                else
                    return value - decrement;
            }
        }

        public async Task EmergencyStopAsync()
        {
            ItemsPerMinute = 0;
            FanSpeedRpm = 0;

            await NotifyAlarmAsync(AlarmType.EmergencyPushButtonPressed);
        }

        private Task backgroundWorker;
        private CancellationTokenSource cts = new();

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
                    Task.Delay(UpdateFrequency, cancellationToken),
                    UpdateAsync()
                );
            }
        }
    }
}
