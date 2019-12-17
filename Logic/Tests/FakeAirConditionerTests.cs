namespace Dim.AirConditioner.Logic.Fakes.Tests
{
    using System;
    using System.Threading.Tasks;
    using Dim.AirConditioner.Logic;
    using Dim.AirConditioner.Logic.Fakes;
    using Dim.AirConditioner.Logic.Tests;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FakeAirConditionerTests
    {
        private const string LogName = "FakeAirConditioner Tests";

        private static ILoggerFactory LoggerFactory;

        public FakeAirConditionerTests()
        {
            LoggerFactory = TestUtils.BuildLoggerFactory();
        }

        [TestMethod]
        public async Task SetToCoolingMode_CurrentRoomTemperatureLowerThanTarget_AirConditionerSwitchesToStandBy()
        {
            // Arrange.
            IAirConditioner airConditioner = new FakeAirConditioner(LoggerFactory.CreateLogger(LogName), 5, secondsToTemperatureChange: 0);
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
            IAirConditioner airConditioner = new FakeAirConditioner(LoggerFactory.CreateLogger(LogName), 10, secondsToTemperatureChange: 0);
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
            IAirConditioner airConditioner = new FakeAirConditioner(LoggerFactory.CreateLogger(LogName), 10, secondsToTemperatureChange: 0);
            airConditioner.PowerOn();

            // Act.
            await airConditioner.StartCoolingMode(9);

            // Assert
            await Task.Delay(TimeSpan.FromSeconds(1));

            airConditioner.RoomTemperature.Should().Be(9);
            airConditioner.CurrentMode.Should().Be(AirConditionerMode.StandBy);
        }

        [TestMethod]
        public async Task StartCoolingMode_TargetTemperatureLowerThanMix_AirConditionerCoolsRoomToMin()
        {
            // Arrange.
            IAirConditioner airConditioner = new FakeAirConditioner(LoggerFactory.CreateLogger(LogName), 5 + 1, secondsToTemperatureChange: 0);
            airConditioner.PowerOn();

            // Act.
            await airConditioner.StartCoolingMode(airConditioner.MinTemperature - 1);

            // Assert
            await Task.Delay(TimeSpan.FromSeconds(1));

            airConditioner.RoomTemperature.Should().Be(airConditioner.MinTemperature);
            airConditioner.CurrentMode.Should().Be(AirConditionerMode.StandBy);
        }

        [TestMethod]
        public async Task StartHeatingMode_CurrentRoomTemperatureHigherThanTarget_AirConditionerSwitchesToStandBy()
        {
            // Arrange.
            IAirConditioner airConditioner = new FakeAirConditioner(LoggerFactory.CreateLogger(LogName), 30, secondsToTemperatureChange: 0);
            airConditioner.PowerOn();

            // Act.
            await airConditioner.StartHeatingMode(20);

            // Assert
            airConditioner.RoomTemperature.Should().Be(30);
            airConditioner.CurrentMode.Should().Be(AirConditionerMode.StandBy);
        }

        [TestMethod]
        public async Task StartHeatingMode_AirConditionerIsOff_AirConditionerSwitchesToStandBy()
        {
            // Arrange.
            IAirConditioner airConditioner = new FakeAirConditioner(LoggerFactory.CreateLogger(LogName), 10, secondsToTemperatureChange: 0);
            airConditioner.PowerOff();

            // Act.
            await airConditioner.StartHeatingMode(20);

            // Assert
            airConditioner.RoomTemperature.Should().Be(10);
            airConditioner.CurrentMode.Should().Be(AirConditionerMode.StandBy);
        }

        [TestMethod]
        public async Task StartHeatingMode_RoomTemperatureIsLowerThanTarget_AirConditionerHeatsRoom()
        {
            // Arrange.
            IAirConditioner airConditioner = new FakeAirConditioner(LoggerFactory.CreateLogger(LogName), 10, secondsToTemperatureChange: 0);
            airConditioner.PowerOn();

            // Act.
            await airConditioner.StartHeatingMode(11);

            // Assert
            await Task.Delay(TimeSpan.FromSeconds(1));

            airConditioner.RoomTemperature.Should().Be(11);
            airConditioner.CurrentMode.Should().Be(AirConditionerMode.StandBy);
        }

        [TestMethod]
        public async Task StartHeatingMode_TargetTemperatureHigherThanMax_AirConditionerHeatsRoomToMax()
        {
            // Arrange.
            IAirConditioner airConditioner = new FakeAirConditioner(LoggerFactory.CreateLogger(LogName), 40 - 1, secondsToTemperatureChange: 0);
            airConditioner.PowerOn();

            // Act.
            await airConditioner.StartHeatingMode(airConditioner.MaxTemperature + 1);

            // Assert
            await Task.Delay(TimeSpan.FromSeconds(1));

            airConditioner.RoomTemperature.Should().Be(airConditioner.MaxTemperature);
            airConditioner.CurrentMode.Should().Be(AirConditionerMode.StandBy);
        }

        [TestMethod]
        public async Task PowerOff_PowerOffDuringRunningTask_TaskShouldBeStopped()
        {
            // Arrange.
            int initialTemperature = 20;
            IAirConditioner airConditioner = new FakeAirConditioner(LoggerFactory.CreateLogger(LogName), initialTemperature, secondsToTemperatureChange: 1);
            airConditioner.PowerOn();

            // Act.
            await airConditioner.StartHeatingMode(airConditioner.MaxTemperature);

            await Task.Delay(TimeSpan.FromSeconds(1));
            airConditioner.RoomTemperature.Should().BeGreaterThan(initialTemperature);
            airConditioner.CurrentMode.Should().Be(AirConditionerMode.Heating);

            airConditioner.PowerOff();

            // Assert
            airConditioner.RoomTemperature.Should().BeLessThan(airConditioner.MaxTemperature);
            airConditioner.CurrentMode.Should().Be(AirConditionerMode.StandBy);
        }
    }
}