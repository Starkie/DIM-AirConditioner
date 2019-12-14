namespace Logic.Tests
{
    using Dim.AirConditioner.Logic;
    using Dim.AirConditioner.Logic.Fakes;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Threading.Tasks;

    [TestClass]
    public class FakeAirConditionerTests
    {
        [TestMethod]
        public async Task SetToCoolingMode_CurrentRoomTemperatureLowerThanTarget_AirConditionerSwitchesToStandBy()
        {
            // Arrange.
            IAirConditioner airConditioner = new FakeAirConditioner(5);
            airConditioner.PowerOn();

            // Act.
            await airConditioner.StartCoolingMode(10);

            // Assert
            airConditioner.RoomTemperature.Should().Be(5);
            airConditioner.CurrentMode.Should().Be(AirConditionerMode.StandBy);
        }

        [TestMethod]
        public async Task SetToCoolingMode_AirConditionerIsOff_AirConditionerSwitchesToStandBy()
        {
            // Arrange.
            IAirConditioner airConditioner = new FakeAirConditioner(10);
            airConditioner.PowerOff();

            // Act.
            await airConditioner.StartCoolingMode(5);

            // Assert
            airConditioner.RoomTemperature.Should().Be(10);
            airConditioner.CurrentMode.Should().Be(AirConditionerMode.StandBy);
        }

        [TestMethod]
        public async Task SetToCoolingMode_RoomTemperatureIsHigherThanTarget_AirConditionerCoolsRoom()
        {
            // Arrange.
            IAirConditioner airConditioner = new FakeAirConditioner(10);
            airConditioner.PowerOn();

            // Act.
            await airConditioner.StartCoolingMode(9);

            // Assert
            airConditioner.RoomTemperature.Should().Be(9);
            airConditioner.CurrentMode.Should().Be(AirConditionerMode.StandBy);
        }
    }
}