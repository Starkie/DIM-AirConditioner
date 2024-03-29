﻿namespace Dim.AirConditioner.Logic
{
    using System.Threading.Tasks;

    /// <summary>
    ///     Interface representing an air conditioner. Provides all the supported actions to control
    ///     the air conditioning system.
    /// </summary>
    public interface IAirConditioner
    {
        /// <summary>
        ///     Gets the current working mode of the air conditioning system. For example, cooling
        ///     the room.
        /// </summary>
        AirConditionerMode CurrentMode { get; }

        /// <summary>
        ///     Gets a value indicating whether the air conditioner is currently powered on.
        /// </summary>
        bool IsOn { get; }

        /// <summary>
        ///     Gets the temperature detected by the air conditioner. Measured in centigrades.
        /// </summary>
        double RoomTemperature { get; }

        /// <summary>
        ///     Gets the min temperature that the air conditioner can cool the room to.
        /// </summary>
        double MinTemperature { get; }

        /// <summary>
        ///     Gets the max temperature that the air conditioner can heat the room to.
        /// </summary>
        double MaxTemperature { get; }

        /// <summary> Powers on the air conditioner. </summary>
        /// <returns> A boolean indicating if the air conditioner was powered off successfully. </returns>
        bool PowerOff();

        /// <summary> Powers off the air conditioner. </summary>
        /// <returns> A boolean indicating if the air conditioner was powered off successfully. </returns>
        bool PowerOn();

        /// <summary> Sets the air conditioner to cooling mode, for a target temperature. </summary>
        /// <param name="targetTemperature"> The temperature to achieve. </param>
        /// <returns>
        ///     A task that enables this method to be awaited, and a boolean indicating whether the
        ///     cooling mode was enabled.
        /// </returns>
        Task<bool> StartCoolingMode(double targetTemperature);

        /// <summary>
        ///     Sets the air conditioner to heating mode, to a target temperature. If the
        ///     <see cref="RoomTemperature"/> is higher than the target temperature, it does nothing.
        /// </summary>
        /// <param name="targetTemperature"> The temperature to achieve. </param>
        /// <returns>
        ///     A task that enables this method to be awaited, and a boolean indicating whether the
        ///     heating mode was enabled.
        /// </returns>
        Task<bool> StartHeatingMode(double targetTemperature);
    }
}