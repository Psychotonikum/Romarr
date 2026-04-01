using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.Organizer;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;

namespace Romarr.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]
    public class RequiresRomTitleFixture : CoreTest<FileNameBuilder>
    {
        private Game _series;
        private Rom _gameFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>
                    .CreateNew()
                    .With(s => s.Title = "South Park")
                    .Build();

            _gameFile = Builder<Rom>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.PlatformNumber = 15)
                            .With(e => e.FileNumber = 6)
                            .With(e => e.AbsoluteFileNumber = 100)
                            .Build();

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameGameFiles = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);
        }

        [Test]
        public void should_return_false_when_gameFile_title_is_not_part_of_the_pattern()
        {
            _namingConfig.StandardGameFileFormat = "{Game Title} S{platform:00}E{rom:00}";
            Subject.RequiresRomTitle(_series, new List<Rom> { _gameFile }).Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_renaming_gameFiles_is_off()
        {
            _namingConfig.RenameGameFiles = false;
            Subject.RequiresRomTitle(_series, new List<Rom> { _gameFile }).Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_gameFile_title_is_part_of_the_pattern()
        {
            Subject.RequiresRomTitle(_series, new List<Rom> { _gameFile }).Should().BeTrue();
        }
    }
}
