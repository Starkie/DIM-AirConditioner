﻿namespace Dim.AirConditioner.Control.Cli
{
    using System.Linq;
    using System.Speech.Recognition;
    using System.Speech.Synthesis;
    using Dim.AirConditioner.Logic;
    using Microsoft.Extensions.Logging;

    /// <summary>
    ///     Class that allows the control of an <see cref="IAirConditioner"/> by voice commands.
    /// </summary>
    public class VoiceControlledAirConditioner
    {
        // The instance of the applications logger.
        private readonly ILogger logger;

        // The air conditioner to control by the voice commands.
        private readonly IAirConditioner airConditioner;

        // The instance of the speech recognizar, that interprets the voice commands.
        private readonly SpeechRecognitionEngine speechRecognizer;

        // The instance of the speech synthetizer, that 'speaks' the system responses.
        private readonly SpeechSynthesizer speechSynthesizer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoiceControlledAirConditioner"/> class.
        /// </summary>
        /// <param name="logger"> The logger. </param>
        /// <param name="airConditioner">
        ///     The instance of the air conditioner to control by the voice commands.
        /// </param>
        public VoiceControlledAirConditioner(ILogger logger, IAirConditioner airConditioner)
        {
            this.logger = logger;
            this.airConditioner = airConditioner;

            this.speechSynthesizer = new SpeechSynthesizer();

            this.speechRecognizer = new SpeechRecognitionEngine();
            this.SetUpSpeechRecognizerCommands();

            this.PowerOnCommand();
        }

        private void SetUpSpeechRecognizerCommands()
        {
            // Only commands recognized with a 60% confidence will be accepted and executed.
            this.speechRecognizer.UpdateRecognizerSetting("CFGConfidenceRejectionThreshold", 60);

            // Load the grammar.
            Grammar voiceCommandsGrammar = this.BuildAirConditionerControlGrammar();
            voiceCommandsGrammar.Enabled = true;
            this.speechRecognizer.LoadGrammar(voiceCommandsGrammar);

            // Set up input device. Picks the default microphone configured in the system.
            this.speechRecognizer.SetInputToDefaultAudioDevice();

            // Set up the event handler, which will redirect the commands to the aproppiate code.
            this.speechRecognizer.SpeechRecognized += this.Recognizer_SpeechRecognized;

            this.speechRecognizer.RecognizeAsync(RecognizeMode.Multiple);
        }

        private Grammar BuildAirConditionerControlGrammar()
        {
            // Power On.
            GrammarBuilder powerOnAirConditionerCommand = new GrammarBuilder(AirConditionerControlVoiceCommands.PowerOnCommand);
            SemanticResultKey powerOnCommandMapping = new SemanticResultKey("PowerOn", powerOnAirConditionerCommand);

            // Power Off.
            GrammarBuilder powerOffAirConditionerCommand = new GrammarBuilder(AirConditionerControlVoiceCommands.PowerOffCommand);
            SemanticResultKey powerOffCommandMapping = new SemanticResultKey("PowerOff", powerOffAirConditionerCommand);

            // Current temperature.
            GrammarBuilder currentTemperatureCommand = new GrammarBuilder(AirConditionerControlVoiceCommands.CurrentTemperatureCommand);
            SemanticResultKey currentTemperatureMapping = new SemanticResultKey("CurrentTemperature", currentTemperatureCommand);

            // Target Temperature recognition.
            Choices numberChoice = new Choices(Enumerable.Range(0, 100)
                .Select(i => i.ToString())
                .ToArray());

            SemanticResultKey targetTempMapping = new SemanticResultKey("TargetTemp", numberChoice);

            // Heat Room.
            Choices heatRoomChoices = new Choices(
                AirConditionerControlVoiceCommands.HeatRoom,
                AirConditionerControlVoiceCommands.HeatRoom2,
                AirConditionerControlVoiceCommands.HeatRoom3);

            GrammarBuilder heatRoomCommand = new GrammarBuilder(heatRoomChoices);
            heatRoomCommand.Append(targetTempMapping);
            heatRoomCommand.Append(AirConditionerControlVoiceCommands.DegreesKeyWord);

            SemanticResultKey heatRoomCommandMapping = new SemanticResultKey("HeatRoom", heatRoomCommand);

            // Cool room.
            Choices coolRoomChoices = new Choices(
                AirConditionerControlVoiceCommands.CoolRoom,
                AirConditionerControlVoiceCommands.CoolRoom2,
                AirConditionerControlVoiceCommands.CoolRoom3);

            GrammarBuilder coolRoomCommand = new GrammarBuilder(coolRoomChoices);
            coolRoomCommand.Append(targetTempMapping);
            coolRoomCommand.Append(AirConditionerControlVoiceCommands.DegreesKeyWord);

            SemanticResultKey coolRoomCommandMapping = new SemanticResultKey("CoolRoom", coolRoomCommand);

            Choices commandCatalog = new Choices(
                powerOnCommandMapping,
                powerOffCommandMapping,
                currentTemperatureMapping,
                heatRoomCommandMapping,
                coolRoomCommandMapping);

            return new Grammar(commandCatalog);
        }

        /// <summary> Event handler tasked with routing the recognized voice commands. </summary>
        /// <param name="sender"> The object that caused the event. </param>
        /// <param name="e"> The speech recognized commands. </param>
        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            SemanticValue semantics = e.Result.Semantics;

            string rawText = e.Result.Text;
            RecognitionResult result = e.Result;

            if (semantics.ContainsKey("PowerOn"))
            {
                this.PowerOnCommand();
            }

            if (semantics.ContainsKey("PowerOff"))
            {
                this.PowerOffCommand();
            }

            if (semantics.ContainsKey("CurrentTemperature"))
            {
                this.CurrentTemperatureCommand();
            }

            if (semantics.ContainsKey("HeatRoom"))
            {
                SemanticValue heatRoomSemanticValue = semantics["HeatRoom"];
                double targetTemperature = double.Parse(heatRoomSemanticValue["TargetTemp"].Value.ToString());

                this.HeatRoomCoomand(targetTemperature);
            }

            if (semantics.ContainsKey("CoolRoom"))
            {
                SemanticValue coolRoomSemanticValue = semantics["CoolRoom"];
                double targetTemperature = double.Parse(coolRoomSemanticValue["TargetTemp"].Value.ToString());

                this.CoolRoomCommand(targetTemperature);
            }
        }

        private void PowerOnCommand()
        {
            if (this.airConditioner.IsOn)
            {
                this.speechSynthesizer.SpeakAsync("El aire acondicionado ya estaba encendido.");

                return;
            }

            this.airConditioner.PowerOn();
            this.speechSynthesizer.SpeakAsync("Aire acondicionado encendido.");
        }

        private void PowerOffCommand()
        {
            if (!this.airConditioner.IsOn)
            {
                this.speechSynthesizer.SpeakAsync("El aire acondicionado ya estaba apagado.");

                return;
            }

            this.speechSynthesizer.SpeakAsync("Apagando aire acondicionado.");
            this.airConditioner.PowerOff();
            this.speechSynthesizer.SpeakAsync("Aire acondicionado apagado.");
        }

        private void CurrentTemperatureCommand()
        {
            this.speechSynthesizer.SpeakAsync(string.Format(AirConditionerControlVoiceCommands.CurrentTemperatureResponse, this.airConditioner.RoomTemperature));
        }

        private void HeatRoomCoomand(double targetTemperature)
        {
            if (!this.airConditioner.IsOn)
            {
                this.PowerOnCommand();
            }

            bool wasHeatingModeEnabled = this.airConditioner.StartHeatingMode(targetTemperature).Result;

            if (wasHeatingModeEnabled)
            {
                this.speechSynthesizer.SpeakAsync("Calentando la habitación a " + targetTemperature + " grados centígrados.");
            }
            else if (this.airConditioner.RoomTemperature >= targetTemperature)
            {
                this.speechSynthesizer.Speak("No se ha encendido la calefacción.");

                this.CurrentTemperatureCommand();
            }
        }

        private void CoolRoomCommand(double targetTemperature)
        {
            if (!this.airConditioner.IsOn)
            {
                this.PowerOnCommand();
            }

            bool wasCoolingModeEnabled = this.airConditioner.StartCoolingMode(targetTemperature).Result;

            if (wasCoolingModeEnabled)
            {
                this.speechSynthesizer.SpeakAsync("Enfriando la habitación a " + targetTemperature + " grados centígrados.");
            }
            else if (this.airConditioner.RoomTemperature <= targetTemperature)
            {
                this.speechSynthesizer.Speak("No se ha encendido la refrigeración.");

                this.CurrentTemperatureCommand();

                this.PowerOffCommand();
            }
        }
    }
}