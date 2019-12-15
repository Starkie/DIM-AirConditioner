namespace Dim.AirConditioner.Logic.Fakes
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    /// <summary> Represents a working Air-Condintioner. </summary>
    public class FakeAirConditioner : IAirConditioner
    {
        // The max temperature that the air conditioner can heat the room to.
        private const double MAX_TEMPERATURE = 40;

        // The min temperature that the air conditioner can cool the room to.
        private const double MIN_TEMPERATURE = -5;

        // Logger to output information about the execution of the air conditioner.
        private readonly ILogger logger;

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
        public FakeAirConditioner(ILogger logger, double? initialTemperature = null)
        {
            this.CurrentMode = AirConditionerMode.StandBy;
            this.IsOn = false;
            this.logger = logger;
            this.RoomTemperature = initialTemperature ?? this.GenerateInitialRoomTemperature();
        }

        /// <inheritdoc/>
        public AirConditionerMode CurrentMode { get; private set; }

        /// <inheritdoc/>
        public bool IsOn { get; private set; }

        /// <inheritdoc/>
        public double RoomTemperature { get; private set; }

        /// <inheritdoc/>
        public bool PowerOn()
        {
            this.IsOn = true;
            this.LogCurrentRoomTemperature();

            return true;
        }

        /// <inheritdoc/>
        public bool PowerOff()
        {
            this.IsOn = false;
            this.LogCurrentRoomTemperature();

            return true;
        }

        /// <summary>
        ///     Sets the air conditioner to cooling mode, to a target temperature. If the
        ///     <see cref="RoomTemperature"/> is lower than the target temperature, it does nothing.
        /// </summary>
        /// <param name="targetTemperature"> The temperature to achieve. </param>
        public async Task StartCoolingMode(double targetTemperature)
        {
            if (!this.IsOn || this.RoomTemperature <= targetTemperature)
            {
                this.CurrentMode = AirConditionerMode.StandBy;

                return;
            }

            this.LogCurrentRoomTemperature();

            // Cancel the current running process.
            if (this.currentTemperatureChangeProcess?.IsRunning() == true)
            {
                await this.currentTemperatureChangeProcess.Cancel();
            }

            await this.CoolRoom(Math.Max(targetTemperature, MIN_TEMPERATURE));
        }

        /// <summary>
        ///     Sets the air conditioner to heating mode, to a target temperature. If the
        ///     <see cref="RoomTemperature"/> is higher than the target temperature, it does nothing.
        /// </summary>
        /// <param name="targetTemperature"> The temperature to achieve. </param>
        public async Task StartHeatingMode(double targetTemperature)
        {
            if (!this.IsOn || this.RoomTemperature >= targetTemperature)
            {
                this.CurrentMode = AirConditionerMode.StandBy;

                return;
            }

            // Cancel the current running process.
            if (this.currentTemperatureChangeProcess?.IsRunning() == true)
            {
                await this.currentTemperatureChangeProcess.Cancel();
            }

            await this.HeatRoom(Math.Min(targetTemperature, MAX_TEMPERATURE));
        }

        /// <summary>
        ///     Generates an initial room temperature, for the air conditioner. The values returned
        ///     will be between 0ºc and 35ºc.
        /// </summary>
        /// <returns> A valid room temperature. </returns>
        private int GenerateInitialRoomTemperature()
        {
            Random random = new Random();

            return random.Next(0, 35);
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

                await Task.Delay(TimeSpan.FromSeconds(5));
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

                await Task.Delay(TimeSpan.FromSeconds(5));
            }

            this.CurrentMode = AirConditionerMode.StandBy;
            this.LogCurrentRoomTemperature();
        }

        private void LogCurrentRoomTemperature()
        {
            string modeName = "";

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

            this.logger.LogInformation(FakeAirConditionerResources.CurrentRoomTemperature, modeName, this.RoomTemperature);
        }
    }
}