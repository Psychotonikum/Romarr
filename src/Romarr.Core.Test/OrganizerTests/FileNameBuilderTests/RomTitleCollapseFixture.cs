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
    public class RomTitleCollapseFixture : CoreTest<FileNameBuilder>
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

        [TestCase("Hey, Baby, What's Wrong (1)", "Hey, Baby, What's Wrong (2)", "Hey, Baby, What's Wrong")]
        [TestCase("Meet the Guys and Girls of Cycle 20 Part 1", "Meet the Guys and Girls of Cycle 20 Part 2", "Meet the Guys and Girls of Cycle 20")]
        [TestCase("Meet the Guys and Girls of Cycle 20 Part1", "Meet the Guys and Girls of Cycle 20 Part2", "Meet the Guys and Girls of Cycle 20")]
        [TestCase("Meet the Guys and Girls of Cycle 20 Part01", "Meet the Guys and Girls of Cycle 20 Part02", "Meet the Guys and Girls of Cycle 20")]
        [TestCase("Meet the Guys and Girls of Cycle 20 Part 01", "Meet the Guys and Girls of Cycle 20 Part 02", "Meet the Guys and Girls of Cycle 20")]
        [TestCase("Meet the Guys and Girls of Cycle 20 part 1", "Meet the Guys and Girls of Cycle 20 part 2", "Meet the Guys and Girls of Cycle 20")]
        [TestCase("Meet the Guys and Girls of Cycle 20 pt 1", "Meet the Guys and Girls of Cycle 20 pt 2", "Meet the Guys and Girls of Cycle 20")]
        [TestCase("Meet the Guys and Girls of Cycle 20 pt. 1", "Meet the Guys and Girls of Cycle 20 pt. 2", "Meet the Guys and Girls of Cycle 20")]
        public void should_collapse_gameFile_titles_when_gameFile_titles_are_the_same(string title1, string title2, string expected)
        {
            _namingConfig.StandardGameFileFormat = "{Rom Title}";

            _gameFile1.Title = title1;
            _gameFile2.Title = title2;

            Subject.BuildFileName(new List<Rom> { _gameFile1, _gameFile2 }, _series, _romFile)
                   .Should().Be(expected);
        }

        [Test]
        public void should_not_collapse_gameFile_titles_when_gameFile_titles_are_not_the_same()
        {
            _namingConfig.StandardGameFileFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title}";
            _namingConfig.MultiGameFileStyle = MultiGameFileStyle.Scene;

            _gameFile1.Title = "Hello";
            _gameFile2.Title = "World";

            Subject.BuildFileName(new List<Rom> { _gameFile1, _gameFile2 }, _series, _romFile)
                   .Should().Be("South Park - S15E06-E07 - Hello + World");
        }

        [Test]
        public void should_not_collaspe_when_result_is_empty()
        {
            _namingConfig.StandardGameFileFormat = "{Rom Title}";

            _gameFile1.Title = "Part 1";
            _gameFile2.Title = "Part 2";

            Subject.BuildFileName(new List<Rom> { _gameFile1, _gameFile2 }, _series, _romFile)
                   .Should().Be("Part 1 + Part 2");
        }
    }
}
