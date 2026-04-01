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

    public class ReservedDeviceNameFixture : CoreTest<FileNameBuilder>
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

            _romFile = new RomFile { Quality = new QualityModel(Quality.HDTV720p), ReleaseGroup = "RomarrTest" };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));

            Mocker.GetMock<ICustomFormatService>()
                  .Setup(v => v.All())
                  .Returns(new List<CustomFormat>());
        }

        [TestCase("Con Game", "Con_Game")]
        [TestCase("Com1 Sat", "Com1_Sat")]
        public void should_replace_reserved_device_name_in_series_folder(string title, string expected)
        {
            _series.Title = title;
            _namingConfig.GameFolderFormat = "{Game.Title}";

            Subject.GetGameFolder(_series).Should().Be($"{expected}");
        }

        [TestCase("Con Game", "Con_Game")]
        [TestCase("Com1 Sat", "Com1_Sat")]
        public void should_replace_reserved_device_name_in_platform_folder(string title, string expected)
        {
            _series.Title = title;
            _namingConfig.PlatformFolderFormat = "{Game.Title} - Platform {Platform:00}";

            Subject.GetPlatformFolder(_series, 1).Should().Be($"{expected} - Platform 01");
        }

        [TestCase("Con Game", "Con_Game")]
        [TestCase("Com1 Sat", "Com1_Sat")]
        public void should_replace_reserved_device_name_in_file_name(string title, string expected)
        {
            _series.Title = title;
            _namingConfig.StandardGameFileFormat = "{Game.Title} - S{Platform:00}E{Rom:00}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile).Should().Be($"{expected} - S15E06");
        }
    }
}
