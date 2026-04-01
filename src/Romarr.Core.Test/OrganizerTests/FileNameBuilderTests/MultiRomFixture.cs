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

    public class MultiGameFileFixture : CoreTest<FileNameBuilder>
    {
        private Game _series;
        private Rom _gameFile1;
        private Rom _gameFile2;
        private Rom _gameFile3;
        private RomFile _romFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>
                    .CreateNew()
                    .With(s => s.Title = "South Park")
                    .Build();

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameGameFiles = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            _gameFile1 = Builder<Rom>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.PlatformNumber = 15)
                            .With(e => e.FileNumber = 6)
                            .With(e => e.AbsoluteFileNumber = 100)
                            .Build();

            _gameFile2 = Builder<Rom>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.PlatformNumber = 15)
                            .With(e => e.FileNumber = 7)
                            .With(e => e.AbsoluteFileNumber = 101)
                            .Build();

            _gameFile3 = Builder<Rom>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.PlatformNumber = 15)
                            .With(e => e.FileNumber = 8)
                            .With(e => e.AbsoluteFileNumber = 102)
                            .Build();

            _romFile = new RomFile { Quality = new QualityModel(Quality.HDTV720p), ReleaseGroup = "RomarrTest" };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));

            Mocker.GetMock<ICustomFormatService>()
                  .Setup(v => v.All())
                  .Returns(new List<CustomFormat>());
        }

        private void GivenProper()
        {
            _romFile.Quality.Revision.Version = 2;
        }

        [Test]
        public void should_replace_Series_space_Title()
        {
            _namingConfig.StandardGameFileFormat = "{Game Title}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("South Park");
        }

        [Test]
        public void should_format_extend_multi_gameFile_properly()
        {
            _namingConfig.StandardGameFileFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title}";
            _namingConfig.MultiGameFileStyle = 0;

            Subject.BuildFileName(new List<Rom> { _gameFile1, _gameFile2 }, _series, _romFile)
                .Should().Be("South Park - S15E06-07 - City Sushi");
        }

        [Test]
        public void should_format_duplicate_multi_gameFile_properly()
        {
            _namingConfig.StandardGameFileFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title}";
            _namingConfig.MultiGameFileStyle = MultiGameFileStyle.Duplicate;

            Subject.BuildFileName(new List<Rom> { _gameFile1, _gameFile2 }, _series, _romFile)
                .Should().Be("South Park - S15E06 - S15E07 - City Sushi");
        }

        [Test]
        public void should_format_repeat_multi_gameFile_properly()
        {
            _namingConfig.StandardGameFileFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title}";
            _namingConfig.MultiGameFileStyle = MultiGameFileStyle.Repeat;

            Subject.BuildFileName(new List<Rom> { _gameFile1, _gameFile2 }, _series, _romFile)
                .Should().Be("South Park - S15E06E07 - City Sushi");
        }

        [Test]
        public void should_format_scene_multi_gameFile_properly()
        {
            _namingConfig.StandardGameFileFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title}";
            _namingConfig.MultiGameFileStyle = MultiGameFileStyle.Scene;

            Subject.BuildFileName(new List<Rom> { _gameFile1, _gameFile2 }, _series, _romFile)
                .Should().Be("South Park - S15E06-E07 - City Sushi");
        }

        [Test]
        public void should_get_proper_filename_when_multi_gameFile_is_duplicated_and_bracket_follows_pattern()
        {
            _namingConfig.StandardGameFileFormat =
                "{Game Title} - S{platform:00}E{rom:00} - ({Quality Title}, {MediaInfo Full}, {Release Group}) - {Rom Title}";
            _namingConfig.MultiGameFileStyle = MultiGameFileStyle.Duplicate;

            Subject.BuildFileName(new List<Rom> { _gameFile1, _gameFile2 }, _series, _romFile)
                   .Should().Be("South Park - S15E06 - S15E07 - (Unknown, , RomarrTest) - City Sushi");
        }

        [Test]
        public void should_format_range_multi_gameFile_properly()
        {
            _namingConfig.StandardGameFileFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title}";
            _namingConfig.MultiGameFileStyle = MultiGameFileStyle.Range;

            Subject.BuildFileName(new List<Rom> { _gameFile1, _gameFile2, _gameFile3 }, _series, _romFile)
                .Should().Be("South Park - S15E06-08 - City Sushi");
        }

        [Test]
        public void should_format_single_gameFile_with_range_multi_gameFile_properly()
        {
            _namingConfig.StandardGameFileFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title}";
            _namingConfig.MultiGameFileStyle = MultiGameFileStyle.Range;

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                .Should().Be("South Park - S15E06 - City Sushi");
        }

        [Test]
        public void should_format_single_anime_gameFile_with_range_multi_gameFile_properly()
        {
            _series.GameType = GameTypes.Standard;
            _namingConfig.MultiGameFileStyle = MultiGameFileStyle.Range;
            _namingConfig.StandardGameFileFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("South Park - S15E06 - City Sushi");
        }

        [Test]
        public void should_default_to_dash_when_serparator_is_not_set()
        {
            _series.GameType = GameTypes.Standard;
            _namingConfig.MultiGameFileStyle = MultiGameFileStyle.Duplicate;
            _namingConfig.StandardGameFileFormat = "{Game Title} - {platform}x{rom:00} - {Rom Title} - {Quality Title}";

            Subject.BuildFileName(new List<Rom> { _gameFile1, _gameFile2 }, _series, _romFile)
                   .Should().Be("South Park - 15x06 - 15x07 - City Sushi - Unknown");
        }

        [Test]
        public void should_format_prefixed_range_multi_gameFile_properly()
        {
            _namingConfig.StandardGameFileFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title}";
            _namingConfig.MultiGameFileStyle = MultiGameFileStyle.PrefixedRange;

            Subject.BuildFileName(new List<Rom> { _gameFile1, _gameFile2, _gameFile3 }, _series, _romFile)
                .Should().Be("South Park - S15E06-E08 - City Sushi");
        }

        [Test]
        public void should_format_prefixed_range_multi_gameFile_anime_properly()
        {
            _series.GameType = GameTypes.Standard;
            _namingConfig.MultiGameFileStyle = MultiGameFileStyle.PrefixedRange;
            _namingConfig.StandardGameFileFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title}";

            Subject.BuildFileName(new List<Rom> { _gameFile1, _gameFile2, _gameFile3 }, _series, _romFile)
                   .Should().Be("South Park - S15E06-E08 - City Sushi");
        }

        [Test]
        public void should_format_single_gameFile_with_prefixed_range_multi_gameFile_properly()
        {
            _namingConfig.StandardGameFileFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title}";
            _namingConfig.MultiGameFileStyle = MultiGameFileStyle.PrefixedRange;

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                .Should().Be("South Park - S15E06 - City Sushi");
        }

        [Test]
        public void should_format_single_anime_gameFile_with_prefixed_range_multi_gameFile_properly()
        {
            _series.GameType = GameTypes.Standard;
            _namingConfig.MultiGameFileStyle = MultiGameFileStyle.PrefixedRange;
            _namingConfig.StandardGameFileFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("South Park - S15E06 - City Sushi");
        }

        [Test]
        public void should_format_prefixed_range_multi_gameFile_using_gameFile_separator()
        {
            _namingConfig.StandardGameFileFormat = "{Game Title} - {platform:0}x{rom:00} - {Rom Title}";
            _namingConfig.MultiGameFileStyle = MultiGameFileStyle.PrefixedRange;

            Subject.BuildFileName(new List<Rom> { _gameFile1, _gameFile2, _gameFile3 }, _series, _romFile)
                .Should().Be("South Park - 15x06-x08 - City Sushi");
        }

        [Test]
        public void should_format_range_multi_gameFile_wrapped_in_brackets()
        {
            _namingConfig.StandardGameFileFormat = "{Game Title} (S{platform:00}E{rom:00}) {Rom Title}";
            _namingConfig.MultiGameFileStyle = MultiGameFileStyle.Range;

            Subject.BuildFileName(new List<Rom> { _gameFile1, _gameFile2, _gameFile3 }, _series, _romFile)
                .Should().Be("South Park (S15E06-08) City Sushi");
        }
    }
}
