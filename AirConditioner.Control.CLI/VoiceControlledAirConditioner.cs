namespace Dim.AirConditioner.Control.Cli
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
            this.speechSynthesizer.SpeakAsync("Inicializando la Aplicación");

            this.speechRecognizer = new SpeechRecognitionEngine();
            this.SetUpSpeechRecognizerCommands();

            this.speechSynthesizer.SpeakAsync("Aplicación preparada para reconocer su voz");
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
            this.speechSynthesizer.SpeakAsync("Creando ahora la gramática");

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
            GrammarBuilder heatRoomCommand = new GrammarBuilder(AirConditionerControlVoiceCommands.HeatRoom);
            heatRoomCommand.Append(targetTempMapping);
            heatRoomCommand.Append(AirConditionerControlVoiceCommands.DegreesKeyWord);

            SemanticResultKey heatRoomCommandMapping = new SemanticResultKey("HeatRoom", heatRoomCommand);

            // Cool room.
            GrammarBuilder coolRoomCommand = new GrammarBuilder(AirConditionerControlVoiceCommands.CoolRoom);
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

                this.airConditioner.StartHeatingMode(targetTemperature);

                this.speechSynthesizer.SpeakAsync("Calentando la habitación a " + targetTemperature + " grados centígrados.");
            }

            if (semantics.ContainsKey("CoolRoom"))
            {
                SemanticValue coolRoomSemanticValue = semantics["CoolRoom"];

                double targetTemperature = double.Parse(coolRoomSemanticValue["TargetTemp"].Value.ToString());

                this.airConditioner.StartCoolingMode(targetTemperature);

                this.speechSynthesizer.SpeakAsync("Enfriando la habitación a " + targetTemperature + " grados centígrados.");
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
    }
}