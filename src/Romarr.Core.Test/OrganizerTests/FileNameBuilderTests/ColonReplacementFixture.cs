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
    public class ColonReplacementFixture : CoreTest<FileNameBuilder>
    {
        private Game _series;
        private Rom _gameFile1;
        private RomFile _romFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>
                    .CreateNew()
                    .With(s => s.Title = "CSI: Vegas")
                    .Build();

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameGameFiles = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            _gameFile1 = Builder<Rom>.CreateNew()
                            .With(e => e.Title = "What Happens in Vegas")
                            .With(e => e.PlatformNumber = 1)
                            .With(e => e.FileNumber = 6)
                            .With(e => e.AbsoluteFileNumber = 100)
                            .Build();

            _romFile = new RomFile { Quality = new QualityModel(Quality.HDTV720p), ReleaseGroup = "RomarrTest" };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));

            Mocker.GetMock<ICustomFormatService>()
                  .Setup(v => v.All())
                  .Returns(new List<CustomFormat>());
        }

        [Test]
        public void should_replace_colon_followed_by_space_with_space_dash_space_by_default()
        {
            _namingConfig.StandardGameFileFormat = "{Game Title}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("CSI - Vegas");
        }

        [TestCase("CSI: Vegas", ColonReplacementFormat.Smart, "CSI - Vegas")]
        [TestCase("CSI: Vegas", ColonReplacementFormat.Dash, "CSI- Vegas")]
        [TestCase("CSI: Vegas", ColonReplacementFormat.Delete, "CSI Vegas")]
        [TestCase("CSI: Vegas", ColonReplacementFormat.SpaceDash, "CSI - Vegas")]
        [TestCase("CSI: Vegas", ColonReplacementFormat.SpaceDashSpace, "CSI - Vegas")]
        public void should_replace_colon_followed_by_space_with_expected_result(string seriesName, ColonReplacementFormat replacementFormat, string expected)
        {
            _series.Title = seriesName;
            _namingConfig.StandardGameFileFormat = "{Game Title}";
            _namingConfig.ColonReplacementFormat = replacementFormat;

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                .Should().Be(expected);
        }

        [TestCase("Game:Title", ColonReplacementFormat.Smart, "Game-Title")]
        [TestCase("Game:Title", ColonReplacementFormat.Dash, "Game-Title")]
        [TestCase("Game:Title", ColonReplacementFormat.Delete, "GameTitle")]
        [TestCase("Game:Title", ColonReplacementFormat.SpaceDash, "Game -Title")]
        [TestCase("Game:Title", ColonReplacementFormat.SpaceDashSpace, "Game - Title")]
        public void should_replace_colon_with_expected_result(string seriesName, ColonReplacementFormat replacementFormat, string expected)
        {
            _series.Title = seriesName;
            _namingConfig.StandardGameFileFormat = "{Game Title}";
            _namingConfig.ColonReplacementFormat = replacementFormat;

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                .Should().Be(expected);
        }

        [TestCase("Game: Title", ColonReplacementFormat.Custom, "\ua789", "Game\ua789 Title")]
        [TestCase("Game: Title", ColonReplacementFormat.Custom, "∶", "Game∶ Title")]
        public void should_replace_colon_with_custom_format(string seriesName, ColonReplacementFormat replacementFormat, string customFormat, string expected)
        {
            _series.Title = seriesName;
            _namingConfig.StandardGameFileFormat = "{Game Title}";
            _namingConfig.ColonReplacementFormat = replacementFormat;
            _namingConfig.CustomColonReplacementFormat = customFormat;

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                .Should().Be(expected);
        }
    }
}
