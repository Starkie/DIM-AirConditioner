namespace Dim.AirConditioner.Logic.Fakes
{
    using System;

    /// <summary> Represents a working Air-Condintioner. </summary>
    public class FakeAirConditioner : IAirConditioner
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FakeAirConditioner"/> class.
        /// </summary>
        /// <param name="initialTemperature">
        ///     The initial room temperature. If none is specified, a random temperature is generated.
        /// </param>
        public FakeAirConditioner(decimal? initialTemperature = null)
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
        public decimal RoomTemperature { get; private set; }

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
        ///     Sets the air conditioner to cooling mode, for a target temperature. If the
        ///     <see cref="RoomTemperature"/> is lower than the target temperature, it does nothing.
        /// </summary>
        /// <param name="targetTemperature"> The temperature to achieve. </param>
        public void SetToCoolingMode(decimal targetTemperature)
        {
            if (!this.IsOn || this.RoomTemperature <= targetTemperature)
            {
                this.CurrentMode = AirConditionerMode.StandBy;

                return;
            }
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
    }
}