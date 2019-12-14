namespace Dim.AirConditioner.Logic
{
    /// <summary>
    ///     Interface representing an air conditioner. Provides all the supported actions to control
    ///     the air conditioning system.
    /// </summary>
    public interface IAirConditioner
    {
        /// <summary>
        ///     Gets a value indicating whether the air conditioner is currently powered on.
        /// </summary>
        bool IsOn { get; }

        /// <summary>
        ///     Gets the temperature detected by the air conditioner. Measured in centigrades.
        /// </summary>
        decimal RoomTemperature { get; }

        /// <summary> Powers on the air conditioner. </summary>
        /// <returns> A boolean indicating if the air conditioner was powered off successfully. </returns>
        bool PowerOff();

        /// <summary> Powers off the air conditioner. </summary>
        /// <returns> A boolean indicating if the air conditioner was powered off successfully. </returns>
        bool PowerOn();
    }
}