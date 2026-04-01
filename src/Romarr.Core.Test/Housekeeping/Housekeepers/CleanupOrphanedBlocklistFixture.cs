using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.Blocklisting;
using Romarr.Core.Housekeeping.Housekeepers;
using Romarr.Core.Languages;
using Romarr.Core.Qualities;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;

namespace Romarr.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedBlocklistFixture : DbTest<CleanupOrphanedBlocklist, Blocklist>
    {
        [Test]
        public void should_delete_orphaned_blocklist_items()
        {
            var blocklist = Builder<Blocklist>.CreateNew()
                .With(h => h.Languages = new List<Language> { Language.English })
                .With(h => h.RomIds = new List<int>())
                .With(h => h.Quality = new QualityModel())
                .BuildNew();

            Db.Insert(blocklist);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_blocklist_items()
        {
            var game = Builder<Game>.CreateNew().BuildNew();

            Db.Insert(game);

            var blocklist = Builder<Blocklist>.CreateNew()
                .With(h => h.Languages = new List<Language> { Language.English })
                .With(h => h.RomIds = new List<int>())
                .With(h => h.Quality = new QualityModel())
                .With(b => b.GameId = game.Id)
                .BuildNew();

            Db.Insert(blocklist);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }
    }
}
