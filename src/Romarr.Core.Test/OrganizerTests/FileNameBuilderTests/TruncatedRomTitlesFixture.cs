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

    public class TruncatedRomTitlesFixture : CoreTest<FileNameBuilder>
    {
        private Game _series;
        private List<Rom> _gameFiles;
        private RomFile _romFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>
                    .CreateNew()
                    .With(s => s.Title = "Game Title")
                    .Build();

            _namingConfig = NamingConfig.Default;
            _namingConfig.MultiGameFileStyle = 0;
            _namingConfig.RenameGameFiles = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            _gameFiles = new List<Rom>
                        {
                            Builder<Rom>.CreateNew()
                                            .With(e => e.Title = "Rom Title 1")
                                            .With(e => e.PlatformNumber = 1)
                                            .With(e => e.FileNumber = 1)
                                            .Build(),

                            Builder<Rom>.CreateNew()
                                            .With(e => e.Title = "Another Rom Title")
                                            .With(e => e.PlatformNumber = 1)
                                            .With(e => e.FileNumber = 2)
                                            .Build(),

                            Builder<Rom>.CreateNew()
                                            .With(e => e.Title = "Yet Another Rom Title")
                                            .With(e => e.PlatformNumber = 1)
                                            .With(e => e.FileNumber = 3)
                                            .Build(),

                            Builder<Rom>.CreateNew()
                                            .With(e => e.Title = "Yet Another Rom Title Take 2")
                                            .With(e => e.PlatformNumber = 1)
                                            .With(e => e.FileNumber = 4)
                                            .Build(),

                            Builder<Rom>.CreateNew()
                                            .With(e => e.Title = "Yet Another Rom Title Take 3")
                                            .With(e => e.PlatformNumber = 1)
                                            .With(e => e.FileNumber = 5)
                                            .Build(),

                            Builder<Rom>.CreateNew()
                                            .With(e => e.Title = "Yet Another Rom Title Take 4")
                                            .With(e => e.PlatformNumber = 1)
                                            .With(e => e.FileNumber = 6)
                                            .Build(),

                            Builder<Rom>.CreateNew()
                                            .With(e => e.Title = "A Really Really Really Really Long Rom Title")
                                            .With(e => e.PlatformNumber = 1)
                                            .With(e => e.FileNumber = 7)
                                            .Build()
                        };

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
        public void should_truncate_with_extension()
        {
            _series.Title = "The Fantastic Life of Mr. Sisko";

            _gameFiles[0].PlatformNumber = 2;
            _gameFiles[0].FileNumber = 18;
            _gameFiles[0].Title = "This title has to be exactly the right number of characters in length, combined with the game title, quality and rom number it becomes close to 255 and the extension puts it right above the 255 limit and triggers the truncation logic";
            _romFile.Quality.Quality = Quality.Unknown;
            _gameFiles = _gameFiles.Take(1).ToList();
            _namingConfig.StandardGameFileFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title} {Quality Full}";

            var result = Subject.BuildFileName(_gameFiles, _series, _romFile, ".mkv");
            result.Length.Should().BeLessOrEqualTo(255);

            result.Should().StartWith("The Fantastic Life of Mr. Sisko - S02E18 - This title has to be");
            result.Should().EndWith("Unknown.mkv");
            result.Length.Should().BeLessOrEqualTo(255);
        }

        [Test]
        public void should_truncate_with_ellipsis_between_first_and_last_gameFile_titles()
        {
            _series.Title = "A Somewhat Long Game Title Name";
            _namingConfig.StandardGameFileFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title} {Quality Full}";

            var result = Subject.BuildFileName(_gameFiles, _series, _romFile);
            result.Length.Should().BeLessOrEqualTo(255);
            result.Should().Be("A Somewhat Long Game Title Name - S01E01-02-03-04-05-06-07 - Rom Title 1...A Really Really Really Really Long Rom Title Unknown");
        }

        [Test]
        public void should_truncate_with_ellipsis_if_only_first_gameFile_title_fits()
        {
            _series.Title = "Lorem ipsum dolor sit amet, consectetur adipiscing elit Maecenas et magna sem Morbi vitae volutpat quam, id porta arcu Orci varius natoque penatibus et magnis dis parturient montes";
            _namingConfig.StandardGameFileFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title} {Quality Full}";

            var result = Subject.BuildFileName(_gameFiles, _series, _romFile);
            result.Length.Should().BeLessOrEqualTo(255);
            result.Should().Be("Lorem ipsum dolor sit amet, consectetur adipiscing elit Maecenas et magna sem Morbi vitae volutpat quam, id porta arcu Orci varius natoque penatibus et magnis dis parturient montes - S01E01-02-03-04-05-06-07 - Rom Title 1... Unknown");
        }

        [Test]
        public void should_truncate_first_gameFile_title_with_ellipsis_if_only_partially_fits()
        {
            _series.Title = "Lorem ipsum dolor sit amet, consectetur adipiscing elit Maecenas et magna sem Morbi vitae volutpat quam, id porta arcu Orci varius natoque penatibus et magnis dis parturient montes nascetur ridiculus musu Cras vestibulum porttitor";
            _namingConfig.StandardGameFileFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title} {Quality Full}";

            var result = Subject.BuildFileName(new List<Rom> { _gameFiles.First() }, _series, _romFile);
            result.Length.Should().BeLessOrEqualTo(255);
            result.Should().StartWith("Lorem ipsum dolor sit amet");
            result.Should().EndWith("Unknown");
            result.Should().Contain("...");
        }

        [Test]
        public void should_truncate_titles_measuring_series_title_bytes()
        {
            _series.Title = "Lor\u00E9m ipsum dolor sit amet, consectetur adipiscing elit Maecenas et magna sem Morbi vitae volutpat quam, id porta arcu Orci varius natoque penatibus et magnis dis parturient montes nascetur ridiculus musu Cras vestibulum amet";
            _namingConfig.StandardGameFileFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title} {Quality Full}";

            var result = Subject.BuildFileName(new List<Rom> { _gameFiles.First() }, _series, _romFile);
            result.GetByteCount().Should().BeLessOrEqualTo(255);

            result.Should().StartWith("Lor\u00E9m ipsum dolor sit amet");
            result.Should().EndWith("Unknown");
            result.Should().Contain("...");
        }

        [Test]
        public void should_truncate_titles_measuring_gameFile_title_bytes()
        {
            _series.Title = "Lorem ipsum dolor sit amet, consectetur adipiscing elit Maecenas et magna sem Morbi vitae volutpat quam, id porta arcu Orci varius natoque penatibus et magnis dis parturient montes nascetur ridiculus musu Cras vestibulum";
            _namingConfig.StandardGameFileFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title} {Quality Full}";

            _gameFiles.First().Title = "Episod\u00E9 Title";

            var result = Subject.BuildFileName(new List<Rom> { _gameFiles.First() }, _series, _romFile);
            result.GetByteCount().Should().BeLessOrEqualTo(255);

            result.Should().Be("Lorem ipsum dolor sit amet, consectetur adipiscing elit Maecenas et magna sem Morbi vitae volutpat quam, id porta arcu Orci varius natoque penatibus et magnis dis parturient montes nascetur ridiculus musu Cras vestibulum - S01E01 - Episod\u00E9 Title Unknown");
        }

        [Test]
        public void should_truncate_titles_measuring_gameFile_title_bytes_middle()
        {
            _series.Title = "Lorem ipsum dolor sit amet, consectetur adipiscing elit Maecenas et magna sem Morbi vitae volutpat quam, id porta arcu Orci varius natoque penatibus et magnis dis parturient montes nascetur ridiculus musu Cras vestibulum porttitor";
            _namingConfig.StandardGameFileFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title} {Quality Full}";

            _gameFiles.First().Title = "Rom T\u00E9tle";

            var result = Subject.BuildFileName(new List<Rom> { _gameFiles.First() }, _series, _romFile);
            result.GetByteCount().Should().BeLessOrEqualTo(255);

            result.Should().StartWith("Lorem ipsum dolor sit amet");
            result.Should().EndWith("Unknown");
            result.Should().Contain("...");
        }
    }
}
