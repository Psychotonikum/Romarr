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

    public class MultiRomTitleFixture : CoreTest<FileNameBuilder>
    {
        private Game _series;
        private Rom _gameFile1;
        private Rom _gameFile2;
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
                            .With(e => e.Title = "Rom Title")
                            .With(e => e.PlatformNumber = 15)
                            .With(e => e.FileNumber = 6)
                            .With(e => e.AbsoluteFileNumber = 100)
                            .Build();

            _gameFile2 = Builder<Rom>.CreateNew()
                            .With(e => e.Title = "Rom Title")
                            .With(e => e.PlatformNumber = 15)
                            .With(e => e.FileNumber = 7)
                            .With(e => e.AbsoluteFileNumber = 101)
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

        [TestCase("Rom Title (1)", "Rom Title (2)")]
        [TestCase("Rom Title Part 1", "Rom Title Part 2")]
        [TestCase("Rom Title", "Rom Title: Part 2")]
        public void should_replace_Series_space_Title(string firstTitle, string secondTitle)
        {
            _gameFile1.Title = firstTitle;
            _gameFile2.Title = secondTitle;

            _namingConfig.StandardGameFileFormat = "{Rom Title} {Quality Full}";

            Subject.BuildFileName(new List<Rom> { _gameFile1, _gameFile2 }, _series, _romFile)
                   .Should().Be("Rom Title Unknown");
        }
    }
}
