using System;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;

namespace Romarr.Core.Test.TvTests.RomRepositoryTests
{
    [TestFixture]
    public class GameFilesBetweenDatesFixture : DbTest<RomRepository, Rom>
    {
        [SetUp]
        public void Setup()
        {
            var game = Builder<Game>.CreateNew()
                                        .With(s => s.Id = 0)
                                        .With(s => s.Runtime = 30)
                                        .With(s => s.Monitored = true)
                                        .Build();

            game.Id = Db.Insert(game).Id;

            var rom = Builder<Rom>.CreateNew()
                                          .With(e => e.Id = 0)
                                          .With(e => e.GameId = game.Id)
                                          .With(e => e.Monitored = true)
                                          .Build();

            Db.Insert(rom);
        }

        [Test]
        public void should_get_gameFiles()
        {
            var roms = Subject.GameFilesBetweenDates(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(3), false, true);
            roms.Should().HaveCount(1);
        }
    }
}
