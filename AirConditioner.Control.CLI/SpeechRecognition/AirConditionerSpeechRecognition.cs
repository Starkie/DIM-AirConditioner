namespace Dim.AirConditioner.Control.Cli.SpeechRecognition
{
    using System.Speech.Recognition;

    /// <summary>
    ///     A speech recognition engine configured to recognize <see cref="IAirConditioner"/>
    ///     control commands.
    /// </summary>
    public class AirConditionerSpeechRecognition
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AirConditionerSpeechRecognition"/> class.
        /// </summary>
        /// <param name="speechRecognitionEngine">
        ///     An instance of the in-process speech recognition engine.
        /// </param>
        public AirConditionerSpeechRecognition(SpeechRecognitionEngine speechRecognitionEngine)
        {
            this.SpeechRecognitionEngine = this.SetUpSpeechRecognizerCommands(speechRecognitionEngine);
        }

        /// <summary> Gets the underlying speech recognition engine. </summary>
        public SpeechRecognitionEngine SpeechRecognitionEngine { get; }

        /// <summary> The instance of the speech recognition command. </summary>
        /// <summary>
        ///     Sets up the <see cref="System.Speech.Recognition.SpeechRecognitionEngine"/> with the
        ///     required commands that allow the voice control of the <see cref="IAirConditioner"/>.
        /// </summary>
        private SpeechRecognitionEngine SetUpSpeechRecognizerCommands(SpeechRecognitionEngine speechRecognitionEngine)
        {
            // Only commands recognized with a 60% confidence will be accepted and executed.
            speechRecognitionEngine.UpdateRecognizerSetting("CFGConfidenceRejectionThreshold", 60);

            // Load the grammar.
            Grammar voiceCommandsGrammar = this.BuildAirConditionerControlGrammar();
            voiceCommandsGrammar.Enabled = true;
            speechRecognitionEngine.LoadGrammar(voiceCommandsGrammar);

            // Set up input device. Picks the default microphone configured in the system.
            speechRecognitionEngine.SetInputToDefaultAudioDevice();

            speechRecognitionEngine.RecognizeAsync(RecognizeMode.Multiple);

            return speechRecognitionEngine;
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
    }
}