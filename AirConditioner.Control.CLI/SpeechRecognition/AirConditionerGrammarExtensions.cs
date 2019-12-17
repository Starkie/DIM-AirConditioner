namespace Dim.AirConditioner.Control.Cli.SpeechRecognition
{
    using System.Linq;
    using System.Speech.Recognition;

    /// <summary> Class containing extension methods for building the Air Conditioner grammar. </summary>
    internal static class AirConditionerGrammarExtensions
    {
        private static readonly Choices NumberChoice = new Choices(Enumerable.Range(0, 100)
            .Select(i => i.ToString())
            .ToArray());

        // Common target temperature mapping.
        private static readonly SemanticResultKey TargetTempMapping = new SemanticResultKey("TargetTemp", NumberChoice);

        /// <summary> <see cref="Choices"/> extension method that adds the power on command. </summary>
        /// <param name="commandCatalogue"> The command catalogue choices. </param>
        /// <returns> The catalogue with the added command. </returns>
        public static Choices AddPowerOnCommand(this Choices commandCatalogue)
        {
            GrammarBuilder powerOnAirConditionerCommand = new GrammarBuilder(VoiceCommands.PowerOnCommand);
            SemanticResultKey powerOnCommandMapping = new SemanticResultKey("PowerOn", powerOnAirConditionerCommand);

            commandCatalogue.Add(powerOnCommandMapping);

            return commandCatalogue;
        }

        /// <summary> <see cref="Choices"/> extension method that adds the Power Off command. </summary>
        /// <param name="commandCatalogue"> The command catalogue choices. </param>
        /// <returns> The catalogue with the added command. </returns>
        public static Choices AddPowerOffCommand(this Choices commandCatalogue)
        {
            GrammarBuilder powerOffAirConditionerCommand = new GrammarBuilder(VoiceCommands.PowerOffCommand);
            SemanticResultKey powerOffCommandMapping = new SemanticResultKey("PowerOff", powerOffAirConditionerCommand);

            commandCatalogue.Add(powerOffCommandMapping);

            return commandCatalogue;
        }

        /// <summary>
        ///     <see cref="Choices"/> extension method that adds the Current Temperature command.
        /// </summary>
        /// <param name="commandCatalogue"> The command catalogue choices. </param>
        /// <returns> The catalogue with the added command. </returns>
        public static Choices AddCurrentTemperatureCommand(this Choices commandCatalogue)
        {
            GrammarBuilder currentTemperatureCommand = new GrammarBuilder(VoiceCommands.CurrentTemperatureCommand);
            SemanticResultKey currentTemperatureMapping = new SemanticResultKey("CurrentTemperature", currentTemperatureCommand);

            commandCatalogue.Add(currentTemperatureMapping);

            return commandCatalogue;
        }

        /// <summary> <see cref="Choices"/> extension method that adds the Heat Room command. </summary>
        /// <param name="commandCatalogue"> The command catalogue choices. </param>
        /// <returns> The catalogue with the added command. </returns>
        public static Choices AddHeatRoomCommand(this Choices commandCatalogue)
        {
            Choices heatRoomChoices = new Choices(
                VoiceCommands.HeatRoom,
                VoiceCommands.HeatRoom2,
                VoiceCommands.HeatRoom3);

            GrammarBuilder heatRoomCommand = new GrammarBuilder(heatRoomChoices);
            heatRoomCommand.Append(TargetTempMapping);
            heatRoomCommand.Append(VoiceCommands.DegreesKeyWord);

            SemanticResultKey heatRoomCommandMapping = new SemanticResultKey("HeatRoom", heatRoomCommand);

            commandCatalogue.Add(heatRoomCommandMapping);

            return commandCatalogue;
        }

        /// <summary> <see cref="Choices"/> extension method that adds the Cool Room command. </summary>
        /// <param name="commandCatalogue"> The command catalogue choices. </param>
        /// <returns> The catalogue with the added command. </returns>
        public static Choices AddCoolRoomCommand(this Choices commandCatalogue)
        {
            Choices coolRoomChoices = new Choices(
                VoiceCommands.CoolRoom,
                VoiceCommands.CoolRoom2,
                VoiceCommands.CoolRoom3);

            GrammarBuilder coolRoomCommand = new GrammarBuilder(coolRoomChoices);
            coolRoomCommand.Append(TargetTempMapping);
            coolRoomCommand.Append(VoiceCommands.DegreesKeyWord);

            SemanticResultKey coolRoomCommandMapping = new SemanticResultKey("CoolRoom", coolRoomCommand);

            commandCatalogue.Add(coolRoomCommandMapping);

            return commandCatalogue;
        }

        /// <summary>
        ///     <see cref="Choices"/> extension method that adds the Change Room Temperature command.
        /// </summary>
        /// <param name="commandCatalogue"> The command catalogue. </param>
        /// <returns> The catalogue with the added command. </returns>
        public static Choices AddChangeTemperatureCommand(this Choices commandCatalogue)
        {
            GrammarBuilder changeRoomTemperatureCommand = new GrammarBuilder(VoiceCommands.ChangeTemperatureCommand);
            changeRoomTemperatureCommand.Append(TargetTempMapping);
            changeRoomTemperatureCommand.Append(VoiceCommands.DegreesKeyWord);

            SemanticResultKey changeRoomTemperatureCommandMapping = new SemanticResultKey("ChangeRoomTemp", changeRoomTemperatureCommand);

            commandCatalogue.Add(changeRoomTemperatureCommandMapping);

            return commandCatalogue;
        }

        /// <summary>
        ///     Builds a <see cref="Grammar"/> from the given <see cref="Choices"/> command catalogue.
        /// </summary>
        /// <param name="commandCatalogue"> The command catalogue. </param>
        /// <returns> A grammar composed from the given command catalogue. </returns>
        public static Grammar BuildGrammar(this Choices commandCatalogue)
        {
            return new Grammar(commandCatalogue);
        }
    }
}