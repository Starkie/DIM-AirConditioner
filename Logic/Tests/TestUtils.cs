using Microsoft.Extensions.Logging;

namespace Logic.Tests
{
    public static class TestUtils
    {
        public static ILoggerFactory BuildLoggerFactory()
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