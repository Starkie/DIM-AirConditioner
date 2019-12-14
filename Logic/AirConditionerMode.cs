namespace Dim.AirConditioner.Logic
{
    /// <summary> Represents the possible modes for the air conditioning system. </summary>
    public enum AirConditionerMode
    {
        /// <summary> The the air conditioner will be lowering the current room temperature. </summary>
        Cooling,

        /// <summary> The air conditioner will be increasing the current room temperature. </summary>
        Heating,

        /// <summary> The air coditioner is waiting for a command. </summary>
        StandBy,
    }
}