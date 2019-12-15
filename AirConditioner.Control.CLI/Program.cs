namespace Dim.AirConditioner.Control.Cli
{
    using System;
    using System.Threading.Tasks;
    using Dim.AirConditioner.Logic;
    using Dim.AirConditioner.Logic.Fakes;
    using Microsoft.Extensions.Logging;

    internal static class Program
    {
        public static async Task Main()
        {
            using (ILoggerFactory loggerFactory = BuildLoggerFactory())
            {
                ILogger logger = loggerFactory.CreateLogger("Air Conditioner Voice Control");

                IAirConditioner airConditioner = new FakeAirConditioner(logger, initialTemperature: 23.5);

                VoiceControlledAirConditioner voiceControlledAirConditioner = new VoiceControlledAirConditioner(airConditioner);

                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
        }

        private static ILoggerFactory BuildLoggerFactory()
        {
            return LoggerFactory.Create(builder =>
            {
                builder.AddFilter("Microsoft", LogLevel.Warning)
                       .AddFilter("System", LogLevel.Warning)
                       .AddFilter("Dim.AirConditioner", LogLevel.Debug)
                       .AddConsole();
            });
        }
    }
}