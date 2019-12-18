namespace Dim.AirConditioner.Control.Cli
{
    using System.Speech.Recognition;
    using System.Speech.Synthesis;
    using System.Threading.Tasks;
    using Dim.AirConditioner.Control.Cli.Resources;
    using Dim.AirConditioner.Control.Cli.SpeechRecognition;
    using Dim.AirConditioner.Logic;
    using Microsoft.Extensions.Logging;

    /// <summary>
    ///     Class that allows the control of an <see cref="IAirConditioner"/> by voice commands.
    /// </summary>
    public class VoiceControlledAirConditioner : IAirConditioner
    {
        // The instance of the applications logger.
        private readonly ILogger logger;

        // The air conditioner to control by the voice commands.
        private readonly IAirConditioner airConditioner;

        // The instance of the speech recognizer, that interprets the voice commands.
        private readonly AirConditionerSpeechRecognition speechRecognizer;

        // The instance of the speech synthesizer, that 'speaks' the system responses.
        private readonly SpeechSynthesizer speechSynthesizer;

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoiceControlledAirConditioner"/> class.
        /// </summary>
        /// <param name="airConditioner">
        ///     The instance of the air conditioner to control by the voice commands.
        /// </param>
        /// <param name="speechRecognition">
        ///     An instance of the in-process speech recognition engine.
        /// </param>
        /// <param name="speechSynthesizer">
        ///     An instance of the speech synthesizer, to output vocal responses to the user.
        /// </param>
        /// <param name="logger"> The logger. </param>
        public VoiceControlledAirConditioner(IAirConditioner airConditioner, AirConditionerSpeechRecognition speechRecognition, SpeechSynthesizer speechSynthesizer, ILogger<IAirConditioner> logger)
        {
            this.logger = logger;
            this.airConditioner = airConditioner;
            this.speechSynthesizer = speechSynthesizer;
            this.speechRecognizer = speechRecognition;

            // Set up the event handler, which will redirect the commands to the appropriate code.
            speechRecognition.SpeechRecognitionEngine.SpeechRecognized += this.Recognizer_SpeechRecognized;

            this.PowerOn();
        }

        /// <inheritdoc/>
        public AirConditionerMode CurrentMode => this.airConditioner.CurrentMode;

        /// <inheritdoc/>
        public bool IsOn => this.airConditioner.IsOn;

        /// <inheritdoc/>
        public double RoomTemperature => this.airConditioner.RoomTemperature;

        /// <inheritdoc/>
        public double MinTemperature => this.airConditioner.MinTemperature;

        /// <inheritdoc/>
        public double MaxTemperature => this.airConditioner.MaxTemperature;

        /// <summary> Event handler tasked with routing the recognized voice commands. </summary>
        /// <param name="sender"> The object that caused the event. </param>
        /// <param name="e"> The speech recognized commands. </param>
        private void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            SemanticValue semantics = e.Result.Semantics;

            this.logger.LogInformation("Recognized Text: " + e.Result.Text);

            if (semantics.ContainsKey("PowerOn"))
            {
                this.PowerOn();
            }

            if (semantics.ContainsKey("PowerOff"))
            {
                this.PowerOff();
            }

            if (semantics.ContainsKey("CurrentTemperature"))
            {
                this.CurrentTemperatureCommand();
            }

            if (semantics.ContainsKey("HeatRoom"))
            {
                SemanticValue heatRoomSemanticValue = semantics["HeatRoom"];
                double targetTemperature = double.Parse(heatRoomSemanticValue["TargetTemp"].Value.ToString());

                Task.WaitAny(this.StartHeatingMode(targetTemperature));
            }

            if (semantics.ContainsKey("CoolRoom"))
            {
                SemanticValue coolRoomSemanticValue = semantics["CoolRoom"];
                double targetTemperature = double.Parse(coolRoomSemanticValue["TargetTemp"].Value.ToString());

                Task.WaitAny(this.StartCoolingMode(targetTemperature));
            }

            if (semantics.ContainsKey("ChangeRoomTemp"))
            {
                SemanticValue coolRoomSemanticValue = semantics["ChangeRoomTemp"];
                double targetTemperature = double.Parse(coolRoomSemanticValue["TargetTemp"].Value.ToString());

                Task.WaitAny(this.ChangeRoomTemperatureCommand(targetTemperature));
            }
        }

        /// <summary>
        ///     Command that speaks aloud the current <see cref="IAirConditioner.RoomTemperature"/> .
        /// </summary>
        private void CurrentTemperatureCommand()
        {
            this.speechSynthesizer.SpeakAsync(string.Format(SpeechResponses.CurrentTemperatureResponse, this.RoomTemperature));
        }

        /// <summary>
        ///     Command to start the <see cref="IAirConditioner"/> and heat up or cool down the room
        ///     to the target temperature.
        /// </summary>
        /// <param name="targetTemperature">
        ///     The temperature to heat up/cool down the room to.
        /// </param>
        private async Task ChangeRoomTemperatureCommand(double targetTemperature)
        {
            if (targetTemperature > this.RoomTemperature)
            {
                await this.StartHeatingMode(targetTemperature);
            }
            else
            {
                await this.StartCoolingMode(targetTemperature);
            }
        }

        /// <inheritdoc/>
        public bool PowerOn()
        {
            if (this.IsOn)
            {
                this.speechSynthesizer.SpeakAsync(SpeechResponses.AlreadyOn);

                return true;
            }

            this.speechSynthesizer.SpeakAsync(SpeechResponses.PoweredOn);
            return this.airConditioner.PowerOn();
        }

        /// <inheritdoc/>
        public bool PowerOff()
        {
            if (!this.IsOn)
            {
                this.speechSynthesizer.SpeakAsync(SpeechResponses.AlreadyOff);

                return true;
            }

            this.speechSynthesizer.SpeakAsync(SpeechResponses.TurningOff);
            return this.airConditioner.PowerOff();
        }

        /// <inheritdoc/>
        public async Task<bool> StartCoolingMode(double targetTemperature)
        {
            if (!this.IsOn)
            {
                this.PowerOn();
            }

            // Try to start air conditioner.
            bool wasCoolingModeEnabled = await this.airConditioner.StartCoolingMode(targetTemperature);

            if (wasCoolingModeEnabled)
            {
                // If the target temperature was lower than the minimum temperature, it will have
                // been truncated to that. Must inform the user.
                if (targetTemperature < this.MinTemperature)
                {
                    this.speechSynthesizer.Speak(string.Format(SpeechResponses.LowerThanMinTemperature, targetTemperature, this.MinTemperature));
                    targetTemperature = this.MinTemperature;
                }

                this.speechSynthesizer.SpeakAsync(string.Format(SpeechResponses.CoolingRoomToTemperature, targetTemperature));
            }

            // If the air conditioner wasn't started because the room temperature was already lower
            // than the target temperature.
            else if (this.RoomTemperature <= targetTemperature)
            {
                this.speechSynthesizer.Speak(SpeechResponses.FailStartingUp);

                this.CurrentTemperatureCommand();

                this.PowerOff();
            }

            return wasCoolingModeEnabled;
        }

        /// <inheritdoc/>
        public async Task<bool> StartHeatingMode(double targetTemperature)
        {
            if (!this.IsOn)
            {
                this.PowerOn();
            }

            // Try to start the air conditioner in heating mode.
            bool wasHeatingModeEnabled = await this.airConditioner.StartHeatingMode(targetTemperature);

            if (wasHeatingModeEnabled)
            {
                // If the target temperature was higher than the Max. temperature, it will have been
                // truncated to that. Must inform the user.
                if (targetTemperature > this.MaxTemperature)
                {
                    this.speechSynthesizer.Speak(string.Format(SpeechResponses.HigherThanMaxTemperature, targetTemperature, this.MaxTemperature));
                    targetTemperature = this.MaxTemperature;
                }

                this.speechSynthesizer.SpeakAsync(string.Format(SpeechResponses.HeatingRoomToTemperature, targetTemperature));
            }

            // If the air conditioner wasn't started because the room temperature was already higher
            // than the target temperature. Must also inform the user.
            else if (this.RoomTemperature >= targetTemperature)
            {
                this.speechSynthesizer.Speak(SpeechResponses.FailStartingUp);

                this.CurrentTemperatureCommand();

                this.PowerOff();
            }

            return wasHeatingModeEnabled;
        }
    }
}