namespace Logic.Tests
{
    using Dim.AirConditioner.Logic;
    using Dim.AirConditioner.Logic.Fakes;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FakeAirConditionerTests
    {
        [TestMethod]
        public void SetToCoolingMode_CurrentRoomTemepratureLowerThanTarget_AirConditionerSwitchesToStandBy()
        {
            // Arrange.
            IAirConditioner airConditioner = new FakeAirConditioner(10);
            airConditioner.PowerOn();

            // Act.
            airConditioner.SetToCoolingMode(5);

            // Assert
            airConditioner.RoomTemperature.Should().Be(10);
            airConditioner.CurrentMode.Should().Be(AirConditionerMode.StandBy);
        }

        [TestMethod]
        public void SetToCoolingMode_AirConditionerIsOff_AirConditionerSwitchesToStandBy()
        {
            // Arrange.
            IAirConditioner airConditioner = new FakeAirConditioner(10);
            airConditioner.PowerOff();

            // Act.
            airConditioner.SetToCoolingMode(5);

            // Assert
            airConditioner.RoomTemperature.Should().Be(10);
            airConditioner.CurrentMode.Should().Be(AirConditionerMode.StandBy);
        }
    }
}