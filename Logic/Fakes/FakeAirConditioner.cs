namespace Dim.AirConditioner.Logic.Fakes
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    /// <summary> Represents a working Air-Condintioner. </summary>
    public class FakeAirConditioner : IAirConditioner
    {
        // Logger to output information about the execution of the air conditioner.
        private readonly ILogger logger;

        // Represents the seconds that must pass between temperature changes.
        private readonly int secondsToTemperatureChange;

        // Represents the current working operation of the fake air conditioner. For example, a
        // cooling process.
        private TemperatureChangeProcess currentTemperatureChangeProcess;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FakeAirConditioner"/> class.
        /// </summary>
        /// <param name="initialTemperature">
        ///     The initial room temperature. If none is specified, a random temperature is generated.
        /// </param>
        /// <param name="logger"> Instance of a logger. </param>
        /// <param name="secondsToTemperatureChange">
        ///     (Optional) Configures the seconds that must pass for the
        ///     <see cref="RoomTemperature"/> to change during a <see cref="TemperatureChangeProcess"/>.
        /// </param>
        public FakeAirConditioner(ILogger<IAirConditioner> logger, double? initialTemperature = null, int secondsToTemperatureChange = 5)
        {
            this.CurrentMode = AirConditionerMode.StandBy;
            this.IsOn = false;
            this.logger = logger;
            this.RoomTemperature = initialTemperature ?? this.GenerateInitialRoomTemperature();
            this.secondsToTemperatureChange = secondsToTemperatureChange;
        }

        /// <inheritdoc/>
        public AirConditionerMode CurrentMode { get; private set; }

        /// <inheritdoc/>
        public double MinTemperature => 5;

        /// <inheritdoc/>
        public double MaxTemperature => 40;

        /// <inheritdoc/>
        public bool IsOn { get; private set; }

        /// <inheritdoc/>
        public double RoomTemperature { get; private set; }

        /// <inheritdoc/>
        public bool PowerOn()
        {
            this.IsOn = true;
            this.logger.LogInformation(FakeAirConditionerResources.AirConditionerPoweredOn);
            this.LogCurrentRoomTemperature();

            return true;
        }

        /// <inheritdoc/>
        public bool PowerOff()
        {
            this.IsOn = false;

            // Cancel the current running process.
            if (this.currentTemperatureChangeProcess?.IsRunning() == true)
            {
                Task.WaitAll(this.currentTemperatureChangeProcess.Cancel());
            }

            this.logger.LogInformation(FakeAirConditionerResources.AirConditionerPoweredOff);
            this.LogCurrentRoomTemperature();

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> StartCoolingMode(double targetTemperature)
        {
            if (!this.IsOn || this.RoomTemperature <= targetTemperature)
            {
                this.CurrentMode = AirConditionerMode.StandBy;

                return false;
            }

            this.LogCurrentRoomTemperature();

            // Cancel the current running process.
            if (this.currentTemperatureChangeProcess?.IsRunning() == true)
            {
                await this.currentTemperatureChangeProcess.Cancel();
            }

            await this.CoolRoom(Math.Max(targetTemperature, this.MinTemperature));

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> StartHeatingMode(double targetTemperature)
        {
            if (!this.IsOn || this.RoomTemperature >= targetTemperature)
            {
                this.CurrentMode = AirConditionerMode.StandBy;

                return false;
            }

            // Cancel the current running process.
            if (this.currentTemperatureChangeProcess?.IsRunning() == true)
            {
                await this.currentTemperatureChangeProcess.Cancel();
            }

            await this.HeatRoom(Math.Min(targetTemperature, this.MaxTemperature));

            return true;
        }

        /// <summary>
        ///     Generates an initial room temperature, for the air conditioner. The values returned
        ///     will be between <see cref="MinTemperature"/> and <see cref="MaxTemperature"/>.
        /// </summary>
        /// <returns> A valid room temperature. </returns>
        private int GenerateInitialRoomTemperature()
        {
            Random random = new Random();

            return random.Next((int)this.MinTemperature + 1, (int)this.MaxTemperature - 1);
        }

        /// <summary> Process that cools down the current room to a target temperature. </summary>
        /// <param name="targetTemperature"> The target temperature to achieve. </param>
        /// <returns> An empty task that enables this method to be awaited. </returns>
        private async Task CoolRoom(double targetTemperature)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Task coolRoomTask = Task.Run(async () =>
            {
                await this.CoolRoomTask(targetTemperature, cancellationToken);
            });

            this.currentTemperatureChangeProcess = new TemperatureChangeProcess(coolRoomTask, cancellationTokenSource);
        }

        private async Task CoolRoomTask(double targetTemperature, CancellationToken cancellationToken)
        {
            this.CurrentMode = AirConditionerMode.Cooling;

            while (this.RoomTemperature > targetTemperature)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                this.RoomTemperature -= 0.5;
                this.LogCurrentRoomTemperature();

                await Task.Delay(TimeSpan.FromSeconds(this.secondsToTemperatureChange));
            }

            this.CurrentMode = AirConditionerMode.StandBy;
            this.LogCurrentRoomTemperature();
        }

        /// <summary> Process that heats up the current room to a target temperature. </summary>
        /// <param name="targetTemperature"> The target temperature to achieve. </param>
        /// <returns> An empty task that enables this method to be awaited. </returns>
        private async Task HeatRoom(double targetTemperature)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Task coolRoomTask = Task.Run(async () =>
            {
                await this.HeatRoomTask(targetTemperature, cancellationToken);
            });

            this.currentTemperatureChangeProcess = new TemperatureChangeProcess(coolRoomTask, cancellationTokenSource);
        }

        private async Task HeatRoomTask(double targetTemperature, CancellationToken cancellationToken)
        {
            this.CurrentMode = AirConditionerMode.Heating;

            while (this.RoomTemperature < targetTemperature)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                this.RoomTemperature += 0.5;
                this.LogCurrentRoomTemperature();

                await Task.Delay(TimeSpan.FromSeconds(this.secondsToTemperatureChange));
            }

            this.CurrentMode = AirConditionerMode.StandBy;
            this.LogCurrentRoomTemperature();
        }

        private void LogCurrentRoomTemperature()
        {
            string power = this.IsOn ? "ON" : "OFF";

            string modeName = string.Empty;

            switch (this.CurrentMode)
            {
                case AirConditionerMode.Cooling:
                    modeName = "COOLING";

                    break;
                case AirConditionerMode.Heating:
                    modeName = "HEATING";

                    break;
                case AirConditionerMode.StandBy:
                    modeName = "STANDBY";
                    break;
            }

            this.logger.LogInformation(FakeAirConditionerResources.CurrentRoomTemperature, power, modeName, this.RoomTemperature);
        }
    }
}