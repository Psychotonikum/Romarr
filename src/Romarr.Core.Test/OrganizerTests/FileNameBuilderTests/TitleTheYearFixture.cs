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
    public class TitleTheYearFixture : CoreTest<FileNameBuilder>
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
                    .Build();

            _gameFile = Builder<Rom>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.PlatformNumber = 15)
                            .With(e => e.FileNumber = 6)
                            .With(e => e.AbsoluteFileNumber = 100)
                            .Build();

            _romFile = new RomFile { Quality = new QualityModel(Quality.HDTV720p), ReleaseGroup = "RomarrTest" };

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

        [TestCase("The Mist", 2018, "Mist, The (2018)")]
        [TestCase("The Rat Pack (A&E)", 1999, "Rat Pack, The (A&E) (1999)")]
        [TestCase("The Climax: I (Almost) Got Away With It (2016)", 2016, "Climax - I (Almost) Got Away With It, The (2016)")]
        [TestCase("A", 2017, "A (2017)")]
        public void should_get_expected_title_back(string title, int year, string expected)
        {
            _series.Title = title;
            _series.Year = year;
            _namingConfig.StandardGameFileFormat = "{Game TitleTheYear}";

            Subject.BuildFileName(new List<Rom> { _gameFile }, _series, _romFile)
                   .Should().Be(expected);
        }

        [Test]
        public void should_not_include_0_for_year()
        {
            _series.Title = "The Alienist";
            _series.Year = 0;
            _namingConfig.StandardGameFileFormat = "{Game TitleTheYear}";

            Subject.BuildFileName(new List<Rom> { _gameFile }, _series, _romFile)
                   .Should().Be("Alienist, The");
        }
    }
}
