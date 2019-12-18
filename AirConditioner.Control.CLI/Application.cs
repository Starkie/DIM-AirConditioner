namespace Dim.AirConditioner.Control.Cli
{
    using System;
    using System.Speech.Recognition;
    using System.Speech.Synthesis;
    using Dim.AirConditioner.Control.Cli.SpeechRecognition;
    using Dim.AirConditioner.Logic;
    using Dim.AirConditioner.Logic.Fakes;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary> Boot straps the application. </summary>
    internal class Application
    {
        /// <summary> Starts an instance of a <see cref="VoiceControlledAirConditioner&gt;"/>. </summary>
        /// <returns> The created voice controlled air conditioner. </returns>
        public static VoiceControlledAirConditioner StartVoiceControlledAirConditioner()
        {
            // Build the dependency container.
            IServiceCollection serviceDescriptors = new ServiceCollection();

            // Register services.
            serviceDescriptors
                .AddLogging(builder =>
                {
                    builder.AddFilter("Microsoft", LogLevel.Warning)
                           .AddFilter("System", LogLevel.Warning)
                           .AddFilter("Dim.AirConditioner", LogLevel.Debug)
                           .AddConsole();
                })
                .AddScoped<SpeechRecognitionEngine>()
                .AddScoped<AirConditionerSpeechRecognition>()
                .AddScoped<SpeechSynthesizer>()
                .AddScoped<IAirConditioner, FakeAirConditioner>()
                .AddScoped<VoiceControlledAirConditioner>();

            IServiceProvider serviceProvider = serviceDescriptors.BuildServiceProvider();

            return serviceProvider.GetService<VoiceControlledAirConditioner>();
        }
    }
}