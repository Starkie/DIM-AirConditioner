using Dim.AirConditioner.Logic;
using Dim.AirConditioner.Logic.Fakes;
using System;
using System.Threading.Tasks;

namespace Dim.AirConditioner.Control.Cli
{
    internal static class Program
    {
        public static async Task Main()
        {
            IAirConditioner airConditioner = new FakeAirConditioner(10);
            airConditioner.PowerOn();

            while (true)
            {
                await airConditioner.StartCoolingMode(5);

                await Task.Delay(TimeSpan.FromSeconds(15));

                await airConditioner.StartHeatingMode(10);

                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }
    }
}