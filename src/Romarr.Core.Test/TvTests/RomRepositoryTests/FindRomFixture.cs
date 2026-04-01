using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;

namespace Romarr.Core.Test.TvTests.RomRepositoryTests
{
    [TestFixture]
    public class FindGameFileFixture : DbTest<RomRepository, Rom>
    {
        private Rom _gameFile1;
        private Rom _gameFile2;

        [SetUp]
        public void Setup()
        {
            _gameFile1 = Builder<Rom>.CreateNew()
                                       .With(e => e.GameId = 1)
                                       .With(e => e.PlatformNumber = 1)
                                       .With(e => e.ScenePlatformNumber = 2)
                                       .With(e => e.FileNumber = 3)
                                       .With(e => e.AbsoluteFileNumber = 3)
                                       .With(e => e.SceneFileNumber = 4)
                                       .BuildNew();

            _gameFile2 = Builder<Rom>.CreateNew()
                                        .With(e => e.GameId = 1)
                                        .With(e => e.PlatformNumber = 1)
                                        .With(e => e.ScenePlatformNumber = 2)
                                        .With(e => e.FileNumber = 4)
                                        .With(e => e.SceneFileNumber = 4)
                                        .BuildNew();

            _gameFile1 = Db.Insert(_gameFile1);
        }

        [Test]
        public void should_find_gameFile_by_scene_numbering()
        {
            Subject.FindGameFilesBySceneNumbering(_gameFile1.GameId, _gameFile1.ScenePlatformNumber.Value, _gameFile1.SceneFileNumber.Value)
                   .First()
                   .Id
                   .Should()
                   .Be(_gameFile1.Id);
        }

        [Test]
        public void should_find_gameFile_by_standard_numbering()
        {
            Subject.Find(_gameFile1.GameId, _gameFile1.PlatformNumber, _gameFile1.FileNumber)
                   .Id
                   .Should()
                   .Be(_gameFile1.Id);
        }

        [Test]
        public void should_not_find_gameFile_that_does_not_exist()
        {
            Subject.Find(_gameFile1.GameId, _gameFile1.PlatformNumber + 1, _gameFile1.FileNumber)
                   .Should()
                   .BeNull();
        }

        [Test]
        public void should_find_gameFile_by_absolute_numbering()
        {
            Subject.Find(_gameFile1.GameId, _gameFile1.AbsoluteFileNumber.Value)
                .Id
                .Should()
                .Be(_gameFile1.Id);
        }

        [Test]
        public void should_return_multiple_gameFile_if_multiple_match_by_scene_numbering()
        {
            _gameFile2 = Db.Insert(_gameFile2);

            Subject.FindGameFilesBySceneNumbering(_gameFile1.GameId, _gameFile1.ScenePlatformNumber.Value, _gameFile1.SceneFileNumber.Value)
                   .Should()
                   .HaveCount(2);
        }
    }
}
