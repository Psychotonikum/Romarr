using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using Romarr.Common.Extensions;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;

namespace Romarr.Core.Test.TvTests.FileMonitoredServiceTests
{
    [TestFixture]
    public class SetGameFileMontitoredFixture : CoreTest<FileMonitoredService>
    {
        private Game _series;
        private List<Rom> _gameFiles;

        [SetUp]
        public void Setup()
        {
            var platforms = 4;

            _series = Builder<Game>.CreateNew()
                                     .With(s => s.Platforms = Builder<Platform>.CreateListOfSize(platforms)
                                                                           .All()
                                                                           .With(n => n.Monitored = true)
                                                                           .Build()
                                                                           .ToList())
                                     .Build();

            _gameFiles = Builder<Rom>.CreateListOfSize(platforms)
                                        .All()
                                        .With(e => e.Monitored = true)
                                        .With(e => e.AirDateUtc = DateTime.UtcNow.AddDays(-7))

                                        // Missing
                                        .TheFirst(1)
                                        .With(e => e.RomFileId = 0)

                                        // Has File
                                        .TheNext(1)
                                        .With(e => e.RomFileId = 1)

                                         // Future
                                        .TheNext(1)
                                        .With(e => e.RomFileId = 0)
                                        .With(e => e.AirDateUtc = DateTime.UtcNow.AddDays(7))

                                        // Future/TBA
                                        .TheNext(1)
                                        .With(e => e.RomFileId = 0)
                                        .With(e => e.AirDateUtc = null)
                                        .Build()
                                        .ToList();

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.GetGameFileBySeries(It.IsAny<int>()))
                  .Returns(_gameFiles);
        }

        private void GivenSpecials()
        {
            foreach (var rom in _gameFiles)
            {
                rom.PlatformNumber = 0;
            }

            _series.Platforms = new List<Platform> { new Platform { Monitored = false, PlatformNumber = 0 } };
        }

        [Test]
        public void should_be_able_to_monitor_series_without_changing_gameFiles()
        {
            Subject.SetGameFileMonitoredStatus(_series, null);

            Mocker.GetMock<IGameService>()
                  .Verify(v => v.UpdateSeries(It.IsAny<Game>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once());

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.UpdateGameFiles(It.IsAny<List<Rom>>()), Times.Never());
        }

        [Test]
        public void should_be_able_to_monitor_all_gameFiles()
        {
            Subject.SetGameFileMonitoredStatus(_series, new MonitoringOptions());

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.UpdateGameFiles(It.Is<List<Rom>>(l => l.All(e => e.Monitored))));
        }

        [Test]
        public void should_be_able_to_monitor_missing_gameFiles_only()
        {
            var monitoringOptions = new MonitoringOptions
                                    {
                                        Monitor = MonitorTypes.Missing
                                    };

            Subject.SetGameFileMonitoredStatus(_series, monitoringOptions);

            VerifyMonitored(e => !e.HasFile);
            VerifyNotMonitored(e => e.HasFile);
        }

        [Test]
        public void should_be_able_to_monitor_new_gameFiles_only()
        {
            var monitoringOptions = new MonitoringOptions
            {
                Monitor = MonitorTypes.Future
            };

            Subject.SetGameFileMonitoredStatus(_series, monitoringOptions);

            VerifyMonitored(e => e.AirDateUtc.HasValue && e.AirDateUtc.Value.After(DateTime.UtcNow));
            VerifyMonitored(e => !e.AirDateUtc.HasValue);
            VerifyNotMonitored(e => e.AirDateUtc.HasValue && e.AirDateUtc.Value.Before(DateTime.UtcNow));
        }

        [Test]
        public void should_not_monitor_missing_specials()
        {
            GivenSpecials();

            var monitoringOptions = new MonitoringOptions
            {
                Monitor = MonitorTypes.Missing
            };

            Subject.SetGameFileMonitoredStatus(_series, monitoringOptions);

            VerifyNotMonitored(e => e.PlatformNumber == 0);
        }

        [Test]
        public void should_not_monitor_new_specials()
        {
            GivenSpecials();

            var monitoringOptions = new MonitoringOptions
            {
                Monitor = MonitorTypes.Future
            };

            Subject.SetGameFileMonitoredStatus(_series, monitoringOptions);

            VerifyNotMonitored(e => e.PlatformNumber == 0);
        }

        [Test]
        public void should_not_monitor_platform_when_all_gameFiles_are_monitored_except_last_platform()
        {
            _series.Platforms = Builder<Platform>.CreateListOfSize(2)
                                             .All()
                                             .With(n => n.Monitored = true)
                                             .Build()
                                             .ToList();

            _gameFiles = Builder<Rom>.CreateListOfSize(5)
                                        .All()
                                        .With(e => e.PlatformNumber = 1)
                                        .With(e => e.RomFileId = 0)
                                        .With(e => e.AirDateUtc = DateTime.UtcNow.AddDays(-5))
                                        .TheLast(1)
                                        .With(e => e.PlatformNumber = 2)
                                        .Build()
                                        .ToList();

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.GetGameFileBySeries(It.IsAny<int>()))
                  .Returns(_gameFiles);

            var monitoringOptions = new MonitoringOptions
            {
                Monitor = MonitorTypes.LastPlatform
            };

            Subject.SetGameFileMonitoredStatus(_series, monitoringOptions);

            VerifyPlatformMonitored(n => n.PlatformNumber == 2);
            VerifyPlatformNotMonitored(n => n.PlatformNumber == 1);
        }

        [Test]
        public void should_be_able_to_monitor_no_gameFiles()
        {
            var monitoringOptions = new MonitoringOptions
                                    {
                                        Monitor = MonitorTypes.None
                                    };

            Subject.SetGameFileMonitoredStatus(_series, monitoringOptions);

            Mocker.GetMock<IRomService>()
                  .Verify(v => v.UpdateGameFiles(It.Is<List<Rom>>(l => l.All(e => !e.Monitored))));
        }

        [Test]
        public void should_monitor_missing_gameFiles()
        {
            var monitoringOptions = new MonitoringOptions
                                    {
                                        Monitor = MonitorTypes.Missing
                                    };

            Subject.SetGameFileMonitoredStatus(_series, monitoringOptions);

            VerifyMonitored(e => !e.HasFile);
            VerifyNotMonitored(e => e.HasFile);
        }

        [Test]
        public void should_monitor_last_platform_if_all_gameFiles_aired_more_than_90_days_ago()
        {
            _series.Platforms = Builder<Platform>.CreateListOfSize(2)
                .All()
                .With(n => n.Monitored = true)
                .Build()
                .ToList();

            _gameFiles = Builder<Rom>.CreateListOfSize(5)
                .All()
                .With(e => e.PlatformNumber = 1)
                .With(e => e.RomFileId = 0)
                .With(e => e.AirDateUtc = DateTime.UtcNow.AddDays(-200))
                .TheLast(2)
                .With(e => e.PlatformNumber = 2)
                .With(e => e.AirDateUtc = DateTime.UtcNow.AddDays(-100))
                .Build()
                .ToList();

            var monitoringOptions = new MonitoringOptions
            {
                Monitor = MonitorTypes.LastPlatform
            };

            Subject.SetGameFileMonitoredStatus(_series, monitoringOptions);

            VerifyPlatformMonitored(n => n.PlatformNumber == 2);
            VerifyMonitored(n => n.PlatformNumber == 2);

            VerifyPlatformNotMonitored(n => n.PlatformNumber == 1);
            VerifyNotMonitored(n => n.PlatformNumber == 1);
        }

        [Test]
        public void should_monitor_latest_platform_if_some_gameFiles_have_aired()
        {
            _gameFiles.ForEach(e => e.AirDateUtc = DateTime.UtcNow.AddDays(-7));

            var monitoringOptions = new MonitoringOptions
            {
                Monitor = MonitorTypes.Future
            };

            Subject.SetGameFileMonitoredStatus(_series, monitoringOptions);

            VerifyPlatformNotMonitored(n => n.PlatformNumber > 0);
            VerifyNotMonitored(n => n.PlatformNumber > 0);
        }

        [Test]
        public void should_monitor_latest_platform_if_some_gameFiles_have_aired_2()
        {
            _series.Platforms = Builder<Platform>.CreateListOfSize(2)
                                             .All()
                                             .With(n => n.Monitored = true)
                                             .Build()
                                             .ToList();

            _gameFiles = Builder<Rom>.CreateListOfSize(5)
                                        .All()
                                        .With(e => e.PlatformNumber = 1)
                                        .With(e => e.RomFileId = 0)
                                        .With(e => e.AirDateUtc = DateTime.UtcNow.AddDays(-100))
                                        .TheLast(2)
                                        .With(e => e.PlatformNumber = 2)
                                        .TheLast(1)
                                        .With(e => e.AirDateUtc = DateTime.UtcNow.AddDays(100))
                                        .Build()
                                        .ToList();

            var monitoringOptions = new MonitoringOptions
                                    {
                                        Monitor = MonitorTypes.LastPlatform
                                    };

            Subject.SetGameFileMonitoredStatus(_series, monitoringOptions);

            VerifyPlatformMonitored(n => n.PlatformNumber == 2);
            VerifyMonitored(n => n.PlatformNumber == 2);

            VerifyPlatformNotMonitored(n => n.PlatformNumber == 1);
            VerifyNotMonitored(n => n.PlatformNumber == 1);
        }

        private void VerifyMonitored(Func<Rom, bool> predicate)
        {
            Mocker.GetMock<IRomService>()
                .Verify(v => v.UpdateGameFiles(It.Is<List<Rom>>(l => l.Where(predicate).All(e => e.Monitored))));
        }

        private void VerifyNotMonitored(Func<Rom, bool> predicate)
        {
            Mocker.GetMock<IRomService>()
                .Verify(v => v.UpdateGameFiles(It.Is<List<Rom>>(l => l.Where(predicate).All(e => !e.Monitored))));
        }

        private void VerifyPlatformMonitored(Func<Platform, bool> predicate)
        {
            Mocker.GetMock<IGameService>()
                  .Verify(v => v.UpdateSeries(It.Is<Game>(s => s.Platforms.Where(predicate).All(n => n.Monitored)), It.IsAny<bool>(), It.IsAny<bool>()));
        }

        private void VerifyPlatformNotMonitored(Func<Platform, bool> predicate)
        {
            Mocker.GetMock<IGameService>()
                  .Verify(v => v.UpdateSeries(It.Is<Game>(s => s.Platforms.Where(predicate).All(n => !n.Monitored)), It.IsAny<bool>(), It.IsAny<bool>()));
        }
    }
}
