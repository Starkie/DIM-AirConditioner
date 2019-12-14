namespace Dim.AirConditioner.Logic.Fakes
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary> Represents a working Air-Condintioner. </summary>
    public class FakeAirConditioner : IAirConditioner
    {
        // Represents the current working operation of the fake air conditioner. For example, a
        // cooling process.
        private TemperatureChangeProcess currentTemperatureChangeProcess;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FakeAirConditioner"/> class.
        /// </summary>
        /// <param name="initialTemperature">
        ///     The initial room temperature. If none is specified, a random temperature is generated.
        /// </param>
        public FakeAirConditioner(double? initialTemperature = null)
        {
            this.CurrentMode = AirConditionerMode.StandBy;
            this.IsOn = false;
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

            return true;
        }

        /// <inheritdoc/>
        public bool PowerOff()
        {
            this.IsOn = false;

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

            // Cancel the current running process.
            if (this.currentTemperatureChangeProcess?.IsRunning() == true)
            {
                await this.currentTemperatureChangeProcess.Cancel();
            }

            await this.CoolRoom(targetTemperature);
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

            await this.HeatRoom(targetTemperature);
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
                await CoolRoomTask(targetTemperature, cancellationToken);
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

                await Task.Delay(TimeSpan.FromSeconds(5));
            }

            this.CurrentMode = AirConditionerMode.StandBy;
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
                await HeatRoomTask(targetTemperature, cancellationToken);
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

                await Task.Delay(TimeSpan.FromSeconds(5));
            }

            this.CurrentMode = AirConditionerMode.StandBy;
        }
    }
}