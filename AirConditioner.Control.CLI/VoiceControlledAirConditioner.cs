using System.Speech.Recognition;
using System.Speech.Synthesis;
using Dim.AirConditioner.Logic;

namespace Dim.AirConditioner.Control.Cli
{
    public class VoiceControlledAirConditioner
    {
        // The air conditioner to control by the voice commands.
        private readonly IAirConditioner airConditioner;

        private SpeechRecognitionEngine recognizer;

        private SpeechSynthesizer synth;

        public VoiceControlledAirConditioner(IAirConditioner airConditioner)
        {
            this.airConditioner = airConditioner;

            this.recognizer = new SpeechRecognitionEngine();

            this.synth = new SpeechSynthesizer();

            this.synth.Speak("Inicializando la Aplicación");

            Grammar grammar = this.BuildAirConditionerControlGrammar();
            this.recognizer.SetInputToDefaultAudioDevice();
            this.recognizer.UnloadAllGrammars();
            this.recognizer.UpdateRecognizerSetting("CFGConfidenceRejectionThreshold", 60);
            grammar.Enabled = true;
            this.recognizer.LoadGrammar(grammar);
            this.recognizer.SpeechRecognized += this.recognizer_SpeechRecognized;

            //reconocimiento asíncrono y múltiples veces
            this.recognizer.RecognizeAsync(RecognizeMode.Multiple);
            this.synth.Speak("Aplicación preparada para reconocer su voz");
        }

        private Grammar BuildAirConditionerControlGrammar()
        {
            this.synth.Speak("Creando ahora la gramática");

            // Power On.
            GrammarBuilder powerOnAirConditionerCommand = new GrammarBuilder(AirConditionerControlVoiceCommands.PowerOnCommand);
            SemanticResultKey powerOnCommandMapping = new SemanticResultKey("PowerOn", powerOnAirConditionerCommand);

            // Power Off.
            GrammarBuilder powerOffAirConditionerCommand = new GrammarBuilder(AirConditionerControlVoiceCommands.PowerOffCommand);
            SemanticResultKey powerOffCommandMapping = new SemanticResultKey("PowerOff", powerOffAirConditionerCommand);

            // Current temperature.
            GrammarBuilder currentTemperatureCommand = new GrammarBuilder(AirConditionerControlVoiceCommands.CurrentTemperatureCommand);
            SemanticResultKey currentTemperatureMapping = new SemanticResultKey("CurrentTemperature", currentTemperatureCommand);

            Choices commandCatalog = new Choices(powerOnCommandMapping, powerOffCommandMapping, currentTemperatureMapping);

            return new Grammar(commandCatalog);
        }

        private void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
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
        }

        private void PowerOnCommand()
        {
            if (this.airConditioner.IsOn)
            {
                this.synth.Speak("El aire acondicionado ya estaba encendido.");

                return;
            }

            this.airConditioner.PowerOn();
            this.synth.Speak("Aire acondicionado encendido.");
        }

        private void PowerOffCommand()
        {
            if (!this.airConditioner.IsOn)
            {
                this.synth.Speak("El aire acondicionado ya estaba apagado.");

                return;
            }

            this.airConditioner.PowerOff();
            this.synth.Speak("Aire acondicionado apagado.");
        }

        private void CurrentTemperatureCommand()
        {
            this.synth.Speak(string.Format(AirConditionerControlVoiceCommands.CurrentTemperatureResponse, this.airConditioner.RoomTemperature));
        }
    }
}