using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.CustomFormats;
using Romarr.Core.MediaFiles;
using Romarr.Core.Organizer;
using Romarr.Core.Qualities;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;

namespace Romarr.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]
    public class OriginalTitleFixture : CoreTest<FileNameBuilder>
    {
        private Game _series;
        private Rom _gameFile;
        private RomFile _romFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>
                    .CreateNew()
                    .With(s => s.Title = "My Game")
                    .Build();

            _gameFile = Builder<Rom>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.PlatformNumber = 15)
                            .With(e => e.FileNumber = 6)
                            .With(e => e.AbsoluteFileNumber = 100)
                            .Build();

            _romFile = new RomFile { Id = 5, Quality = new QualityModel(Quality.HDTV720p), ReleaseGroup = "RomarrTest" };

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameGameFiles = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));

            Mocker.GetMock<ICustomFormatService>()
                  .Setup(v => v.All())
                  .Returns(new List<CustomFormat>());
        }

        [Test]
        public void should_include_original_title_if_not_current_file_name()
        {
            _romFile.SceneName = "my.game.s15e06";
            _romFile.RelativePath = "My Game - S15E06 - City Sushi";
            _namingConfig.StandardGameFileFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title} {[Original Title]}";

            Subject.BuildFileName(new List<Rom> { _gameFile }, _series, _romFile)
                   .Should().Be("My Game - S15E06 - City Sushi [my.game.s15e06]");
        }

        [Test]
        public void should_include_current_filename_if_not_renaming_files()
        {
            _romFile.SceneName = "my.game.s15e06";
            _namingConfig.RenameGameFiles = false;

            Subject.BuildFileName(new List<Rom> { _gameFile }, _series, _romFile)
                   .Should().Be("my.game.s15e06");
        }

        [Test]
        public void should_include_current_filename_if_not_including_platform_and_gameFile_tokens_for_standard_series()
        {
            _romFile.RelativePath = "My Game - S15E06 - City Sushi";
            _namingConfig.StandardGameFileFormat = "{Original Title} {Quality Title}";

            Subject.BuildFileName(new List<Rom> { _gameFile }, _series, _romFile)
                   .Should().Be("My Game - S15E06 - City Sushi Unknown");
        }

        [Test]
        public void should_include_current_filename_if_not_including_air_date_token_for_daily_series()
        {
            _series.GameType = GameTypes.Standard;
            _gameFile.AirDate = "2022-04-28";
            _romFile.RelativePath = "My Game - 2022-04-28 - City Sushi";
            _namingConfig.StandardGameFileFormat = "{Original Title} {Quality Title}";

            Subject.BuildFileName(new List<Rom> { _gameFile }, _series, _romFile)
                   .Should().Be("My Game - 2022-04-28 - City Sushi Unknown");
        }

        [Test]
        public void should_include_current_filename_if_not_including_absolute_gameFile_number_token_for_standard_series()
        {
            _series.GameType = GameTypes.Standard;
            _romFile.RelativePath = "My Game - S15E06 - City Sushi";
            _namingConfig.StandardGameFileFormat = "{Original Title} {Quality Title}";

            Subject.BuildFileName(new List<Rom> { _gameFile }, _series, _romFile)
                   .Should().Be("My Game - S15E06 - City Sushi Unknown");
        }

        [Test]
        public void should_not_include_current_filename_if_including_platform_and_gameFile_tokens_for_standard_series()
        {
            _romFile.RelativePath = "My Game - S15E06 - City Sushi";
            _namingConfig.StandardGameFileFormat = "{Game Title} - S{platform:00}E{rom:00} {[Original Title]}";

            Subject.BuildFileName(new List<Rom> { _gameFile }, _series, _romFile)
                   .Should().Be("My Game - S15E06");
        }

        [Test]
        public void should_not_include_current_filename_if_including_gameFile_tokens_for_standard_series()
        {
            _series.GameType = GameTypes.Standard;
            _romFile.RelativePath = "My Game - S15E06 - City Sushi";
            _namingConfig.StandardGameFileFormat = "{Game Title} - S{platform:00}E{rom:00} {[Original Title]}";

            Subject.BuildFileName(new List<Rom> { _gameFile }, _series, _romFile)
                   .Should().Be("My Game - S15E06");
        }

        [Test]
        public void should_include_current_filename_for_new_file_if_including_platform_and_gameFile_tokens_for_standard_series()
        {
            _romFile.Id = 0;
            _romFile.RelativePath = "My Game - S15E06 - City Sushi";
            _namingConfig.StandardGameFileFormat = "{Game Title} - S{platform:00}E{rom:00} {[Original Title]}";

            Subject.BuildFileName(new List<Rom> { _gameFile }, _series, _romFile)
                   .Should().Be("My Game - S15E06 [My Game - S15E06 - City Sushi]");
        }

        [Test]
        public void should_include_current_filename_for_new_file_if_including_gameFile_tokens_for_standard_series()
        {
            _series.GameType = GameTypes.Standard;
            _romFile.Id = 0;
            _romFile.RelativePath = "My Game - S15E06 - City Sushi";
            _namingConfig.StandardGameFileFormat = "{Game Title} - S{platform:00}E{rom:00} {[Original Title]}";

            Subject.BuildFileName(new List<Rom> { _gameFile }, _series, _romFile)
                   .Should().Be("My Game - S15E06 [My Game - S15E06 - City Sushi]");
        }
    }
}
