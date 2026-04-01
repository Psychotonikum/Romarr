using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Romarr.Core.DecisionEngine.Specifications;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;

namespace Romarr.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class SameFilesSpecificationFixture : CoreTest<SameFilesSpecification>
    {
        private List<Rom> _gameFiles;

        [SetUp]
        public void Setup()
        {
            _gameFiles = Builder<Rom>.CreateListOfSize(2)
                                        .All()
                                        .With(e => e.RomFileId = 1)
                                        .BuildList();
        }

        private void GivenGameFilesInFile(List<Rom> roms)
        {
            Mocker.GetMock<IRomService>()
                  .Setup(s => s.GetRomsByFileId(It.IsAny<int>()))
                  .Returns(roms);
        }

        [Test]
        public void should_not_upgrade_when_new_release_contains_less_gameFiles()
        {
            GivenGameFilesInFile(_gameFiles);

            Subject.IsSatisfiedBy(new List<Rom> { _gameFiles.First() }).Should().BeFalse();
        }

        [Test]
        public void should_upgrade_when_new_release_contains_more_gameFiles()
        {
            GivenGameFilesInFile(new List<Rom> { _gameFiles.First() });

            Subject.IsSatisfiedBy(_gameFiles).Should().BeTrue();
        }

        [Test]
        public void should_upgrade_when_new_release_contains_the_same_gameFiles()
        {
            GivenGameFilesInFile(_gameFiles);

            Subject.IsSatisfiedBy(_gameFiles).Should().BeTrue();
        }

        [Test]
        public void should_upgrade_when_release_contains_the_same_gameFiles_as_multiple_files()
        {
            var roms = Builder<Rom>.CreateListOfSize(2)
                                           .BuildList();

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.GetRomsByFileId(roms.First().RomFileId))
                  .Returns(new List<Rom> { roms.First() });

            Mocker.GetMock<IRomService>()
                  .Setup(s => s.GetRomsByFileId(roms.Last().RomFileId))
                  .Returns(new List<Rom> { roms.Last() });

            Subject.IsSatisfiedBy(roms).Should().BeTrue();
        }
    }
}
