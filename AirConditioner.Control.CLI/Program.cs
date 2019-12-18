namespace Dim.AirConditioner.Control.Cli
{
    using System;
    using System.Threading.Tasks;

    internal static class Program
    {
        public static async Task Main()
        {
            VoiceControlledAirConditioner voiceControlledAirConditioner = Application.StartVoiceControlledAirConditioner();

            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }
}