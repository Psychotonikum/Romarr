using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.Housekeeping.Housekeepers;
using Romarr.Core.Organizer;
using Romarr.Core.Test.Framework;

namespace Romarr.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupAdditionalNamingSpecsFixture : DbTest<CleanupAdditionalNamingSpecs, NamingConfig>
    {
        [Test]
        public void should_delete_additional_naming_configs()
        {
            var specs = Builder<NamingConfig>.CreateListOfSize(5)
                                             .BuildListOfNew();

            Db.InsertMany(specs);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }

        [Test]
        public void should_not_delete_if_only_one_spec()
        {
            var spec = Builder<NamingConfig>.CreateNew()
                                            .BuildNew();

            Db.Insert(spec);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }
    }
}
