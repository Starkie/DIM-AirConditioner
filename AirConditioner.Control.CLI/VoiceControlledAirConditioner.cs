namespace Dim.AirConditioner.Control.Cli
{
    using System.Speech.Recognition;
    using System.Speech.Synthesis;
    using Dim.AirConditioner.Control.Cli.Resources;
    using Dim.AirConditioner.Control.Cli.SpeechRecognition;
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

        // The instance of the speech recognizer, that interprets the voice commands.
        private readonly SpeechRecognitionEngine speechRecognizer;

        // The instance of the speech synthesizer, that 'speaks' the system responses.
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

        /// <summary>
        ///     Sets up the <see cref="SpeechRecognitionEngine"/> with the required commands that
        ///     allow the voice control of the <see cref="IAirConditioner"/>.
        /// </summary>
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

            // Set up the event handler, which will redirect the commands to the appropriate code.
            this.speechRecognizer.SpeechRecognized += this.Recognizer_SpeechRecognized;

            this.speechRecognizer.RecognizeAsync(RecognizeMode.Multiple);
        }

        /// <summary>
        ///     Builds the grammar required to recognize the voice commands that control the <see cref="IAirConditioner"/>.
        /// </summary>
        /// <returns> The grammar with the supported commands. </returns>
        private Grammar BuildAirConditionerControlGrammar()
        {
            Choices commandCatalog = new Choices();

            return commandCatalog.AddPowerOnCommand()
                .AddPowerOffCommand()
                .AddCurrentTemperatureCommand()
                .AddHeatRoomCommand()
                .AddCoolRoomCommand()
                .AddChangeTemperatureCommand()
                .BuildGrammar();
        }

        /// <summary> Event handler tasked with routing the recognized voice commands. </summary>
        /// <param name="sender"> The object that caused the event. </param>
        /// <param name="e"> The speech recognized commands. </param>
        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            SemanticValue semantics = e.Result.Semantics;

            this.logger.LogInformation("Recognized Text: " + e.Result.Text);

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

            if (semantics.ContainsKey("ChangeRoomTemp"))
            {
                SemanticValue coolRoomSemanticValue = semantics["ChangeRoomTemp"];
                double targetTemperature = double.Parse(coolRoomSemanticValue["TargetTemp"].Value.ToString());

                this.ChangeRoomTemperatureCommand(targetTemperature);
            }
        }

        /// <summary> Command to power on the <see cref="IAirConditioner"/>. </summary>
        private void PowerOnCommand()
        {
            if (this.airConditioner.IsOn)
            {
                this.speechSynthesizer.SpeakAsync(SpeechResponses.AlreadyOn);

                return;
            }

            this.airConditioner.PowerOn();

            this.speechSynthesizer.SpeakAsync(SpeechResponses.PoweredOn);
        }

        /// <summary> Command to power off the <see cref="IAirConditioner"/>. </summary>
        private void PowerOffCommand()
        {
            if (!this.airConditioner.IsOn)
            {
                this.speechSynthesizer.SpeakAsync(SpeechResponses.AlreadyOff);

                return;
            }

            this.speechSynthesizer.SpeakAsync(SpeechResponses.TurningOff);
            this.airConditioner.PowerOff();
            this.speechSynthesizer.SpeakAsync(SpeechResponses.TurnedOff);
        }

        /// <summary>
        ///     Command that speaks aloud the current <see cref="IAirConditioner.RoomTemperature"/> .
        /// </summary>
        private void CurrentTemperatureCommand()
        {
            this.speechSynthesizer.SpeakAsync(string.Format(SpeechResponses.CurrentTemperatureResponse, this.airConditioner.RoomTemperature));
        }

        /// <summary>
        ///     Command to start the <see cref="IAirConditioner"/> and heat the room to the target temperature.
        /// </summary>
        /// <param name="targetTemperature"> The temperature to heat the room to. </param>
        /// <remarks>
        ///     If the target temperature is lower than the current
        ///     <see cref="IAirConditioner.RoomTemperature"/>, the air conditioner will not start.
        /// </remarks>
        private void HeatRoomCoomand(double targetTemperature)
        {
            if (!this.airConditioner.IsOn)
            {
                this.PowerOnCommand();
            }

            bool wasHeatingModeEnabled = this.airConditioner.StartHeatingMode(targetTemperature).Result;

            if (wasHeatingModeEnabled)
            {
                if (targetTemperature > this.airConditioner.MaxTemperature)
                {
                    this.speechSynthesizer.Speak(string.Format(SpeechResponses.HigherThanMaxTemperature, targetTemperature, this.airConditioner.MaxTemperature));
                    targetTemperature = this.airConditioner.MaxTemperature;
                }

                this.speechSynthesizer.SpeakAsync(string.Format(SpeechResponses.HeatingRoomToTemperature, targetTemperature));
            }
            else if (this.airConditioner.RoomTemperature >= targetTemperature)
            {
                this.speechSynthesizer.Speak(SpeechResponses.FailStartingUp);

                this.CurrentTemperatureCommand();

                this.PowerOffCommand();
            }
        }

        /// <summary>
        ///     Command to start the <see cref="IAirConditioner"/> and cool down the room to the
        ///     target temperature.
        /// </summary>
        /// <param name="targetTemperature"> The temperature to cool down the room to. </param>
        /// <remarks>
        ///     If the target temperature is higher than the current
        ///     <see cref="IAirConditioner.RoomTemperature"/>, the air conditioner will not start.
        /// </remarks>
        private void CoolRoomCommand(double targetTemperature)
        {
            if (!this.airConditioner.IsOn)
            {
                this.PowerOnCommand();
            }

            bool wasCoolingModeEnabled = this.airConditioner.StartCoolingMode(targetTemperature).Result;

            if (wasCoolingModeEnabled)
            {
                if (targetTemperature < this.airConditioner.MinTemperature)
                {
                    this.speechSynthesizer.Speak(string.Format(SpeechResponses.LowerThanMinTemperature, targetTemperature, this.airConditioner.MinTemperature));
                    targetTemperature = this.airConditioner.MinTemperature;
                }

                this.speechSynthesizer.SpeakAsync(string.Format(SpeechResponses.CoolingRoomToTemperature, targetTemperature));
            }
            else if (this.airConditioner.RoomTemperature <= targetTemperature)
            {
                this.speechSynthesizer.Speak(SpeechResponses.FailStartingUp);

                this.CurrentTemperatureCommand();

                this.PowerOffCommand();
            }
        }

        /// <summary>
        ///     Command to start the <see cref="IAirConditioner"/> and heat up or cool down the room
        ///     to the target temperature.
        /// </summary>
        /// <param name="targetTemperature">
        ///     The temperature to heat up/cool down the room to.
        /// </param>
        private void ChangeRoomTemperatureCommand(double targetTemperature)
        {
            if (targetTemperature > this.airConditioner.RoomTemperature)
            {
                this.HeatRoomCoomand(targetTemperature);
            }
            else
            {
                this.CoolRoomCommand(targetTemperature);
            }
        }
    }
}