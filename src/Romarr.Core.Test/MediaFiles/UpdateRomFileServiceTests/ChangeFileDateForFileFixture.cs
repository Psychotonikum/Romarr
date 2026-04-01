using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Romarr.Common.Disk;
using Romarr.Common.Extensions;
using Romarr.Core.Configuration;
using Romarr.Core.MediaFiles;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;
using Romarr.Test.Common;

namespace Romarr.Core.Test.MediaFiles.UpdateRomFileServiceTests
{
    [TestFixture]
    public class ChangeFileDateForFileFixture : CoreTest<UpdateRomFileService>
    {
        private readonly DateTime _veryOldAirDateUtc = new(1965, 01, 01, 0, 0, 0, 512, 512, DateTimeKind.Utc);
        private DateTime _lastWrite = new(2025, 07, 27, 12, 0, 0, 512, 512, DateTimeKind.Utc);
        private Game _series;
        private RomFile _romFile;
        private string _seriesFolder;
        private List<Rom> _gameFiles;

        [SetUp]
        public void Setup()
        {
            _seriesFolder = @"C:\Test\TV\Game Title".AsOsAgnostic();

            _series = Builder<Game>.CreateNew()
                                     .With(s => s.Path = _seriesFolder)
                                     .Build();

            _gameFiles = Builder<Rom>.CreateListOfSize(1)
                                        .All()
                                        .With(e => e.AirDateUtc = _lastWrite.AddDays(2))
                                        .Build()
                                        .ToList();

            _romFile = Builder<RomFile>.CreateNew()
                                               .With(f => f.Path = Path.Combine(_series.Path, "Platform 1", "Game Title - S01E01.mkv").AsOsAgnostic())
                                               .With(f => f.RelativePath = @"Platform 1\Game Title - S01E01.mkv".AsOsAgnostic())
                                               .Build();

            Mocker.GetMock<IDiskProvider>()
                .Setup(x => x.FileGetLastWrite(_romFile.Path))
                .Returns(() => _lastWrite);

            Mocker.GetMock<IDiskProvider>()
                .Setup(x => x.FileSetLastWriteTime(_romFile.Path, It.IsAny<DateTime>()))
                .Callback<string, DateTime>((path, dateTime) =>
                {
                    _lastWrite = dateTime.Kind == DateTimeKind.Utc
                        ? dateTime
                        : dateTime.ToUniversalTime();
                });

            Mocker.GetMock<IConfigService>()
                .Setup(x => x.FileDate)
                .Returns(FileDateType.LocalAirDate);
        }

        [Test]
        public void should_change_date_once_only()
        {
            var previousWrite = new DateTime(_lastWrite.Ticks, _lastWrite.Kind);

            Subject.ChangeFileDateForFile(_romFile, _series, _gameFiles);
            Subject.ChangeFileDateForFile(_romFile, _series, _gameFiles);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.FileSetLastWriteTime(_romFile.Path, It.IsAny<DateTime>()), Times.Once());

            var actualWriteTime = Mocker.GetMock<IDiskProvider>().Object.FileGetLastWrite(_romFile.Path).ToLocalTime();
            actualWriteTime.Should().Be(_gameFiles[0].AirDateUtc.Value.ToLocalTime().WithTicksFrom(previousWrite));
        }

        [Test]
        public void should_clamp_mtime_on_posix()
        {
            PosixOnly();

            var previousWrite = new DateTime(_lastWrite.Ticks, _lastWrite.Kind);
            _gameFiles[0].AirDateUtc = _veryOldAirDateUtc;

            Subject.ChangeFileDateForFile(_romFile, _series, _gameFiles);
            Subject.ChangeFileDateForFile(_romFile, _series, _gameFiles);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.FileSetLastWriteTime(_romFile.Path, It.IsAny<DateTime>()), Times.Once());

            var actualWriteTime = Mocker.GetMock<IDiskProvider>().Object.FileGetLastWrite(_romFile.Path).ToLocalTime();
            actualWriteTime.Should().Be(DateTimeExtensions.EpochTime.ToLocalTime().WithTicksFrom(previousWrite));
        }

        [Test]
        public void should_not_clamp_mtime_on_windows()
        {
            WindowsOnly();

            var previousWrite = new DateTime(_lastWrite.Ticks, _lastWrite.Kind);
            _gameFiles[0].AirDateUtc = _veryOldAirDateUtc;

            Subject.ChangeFileDateForFile(_romFile, _series, _gameFiles);
            Subject.ChangeFileDateForFile(_romFile, _series, _gameFiles);

            Mocker.GetMock<IDiskProvider>()
                .Verify(v => v.FileSetLastWriteTime(_romFile.Path, It.IsAny<DateTime>()), Times.Once());

            var actualWriteTime = Mocker.GetMock<IDiskProvider>().Object.FileGetLastWrite(_romFile.Path).ToLocalTime();
            actualWriteTime.Should().Be(_gameFiles[0].AirDateUtc.Value.ToLocalTime().WithTicksFrom(previousWrite));
        }
    }
}
