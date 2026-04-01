using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.Download.Pending;
using Romarr.Core.Housekeeping.Housekeepers;
using Romarr.Core.Parser.Model;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;

namespace Romarr.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedPendingReleasesFixture : DbTest<CleanupOrphanedPendingReleases, PendingRelease>
    {
        [Test]
        public void should_delete_orphaned_pending_items()
        {
            var pendingRelease = Builder<PendingRelease>.CreateNew()
                .With(h => h.ParsedRomInfo = new ParsedRomInfo())
                .With(h => h.Release = new ReleaseInfo())
                .BuildNew();

            Db.Insert(pendingRelease);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_pending_items()
        {
            var game = Builder<Game>.CreateNew().BuildNew();

            Db.Insert(game);

            var pendingRelease = Builder<PendingRelease>.CreateNew()
                .With(h => h.GameId = game.Id)
                .With(h => h.ParsedRomInfo = new ParsedRomInfo())
                .With(h => h.Release = new ReleaseInfo())
                .BuildNew();

            Db.Insert(pendingRelease);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }
    }
}
