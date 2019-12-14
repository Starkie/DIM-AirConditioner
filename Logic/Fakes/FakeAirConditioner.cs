namespace Dim.AirConditioner.Logic.Fakes
{
    using System;

    /// <summary> Represents a working Air-Condintioner. </summary>
    public class FakeAirConditioner
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FakeAirConditioner"/> class.
        /// </summary>
        public FakeAirConditioner()
        {
            this.RoomTemperature = this.GenerateInitialRoomTemperature();
        }

        /// <summary>
        ///     Gets a value indicating whether the air conditioner is currently powered on.
        /// </summary>
        public bool IsOn { get; private set; }

        /// <summary> Gets the temperature detected by the air conditioner. </summary>
        public decimal RoomTemperature { get; private set; }

        /// <summary> Powers on the air conditioner. </summary>
        /// <returns> A boolean indicating if the air conditioner was powered off successfully. </returns>
        public bool PowerOn()
        {
            this.IsOn = true;

            return true;
        }

        /// <summary> Powers off the air conditioner. </summary>
        /// <returns> A boolean indicating if the air conditioner was powered off successfully. </returns>
        public bool PowerOff()
        {
            this.IsOn = false;

            return true;
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