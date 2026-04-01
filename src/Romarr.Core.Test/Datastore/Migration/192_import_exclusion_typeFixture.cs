using System.Linq;
using Dapper;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.Datastore.Migration;
using Romarr.Core.Test.Framework;

namespace Romarr.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class import_exclusion_typeFixture : MigrationTest<import_exclusion_type>
    {
        [Test]
        public void should_alter_tvdbid_column()
        {
            var db = WithDapperMigrationTestDb(c =>
            {
                c.Insert.IntoTable("ImportListExclusions").Row(new
                {
                    TvdbId = "1",
                    Title = "Some Game"
                });
            });

            // Should be able to insert as int after migration
            db.Execute("INSERT INTO \"ImportListExclusions\" (\"TvdbId\", \"Title\") VALUES (2, 'Some Other Game')");

            var exclusions = db.Query<ImportListExclusions192>("SELECT * FROM \"ImportListExclusions\"");

            exclusions.Should().HaveCount(2);
            exclusions.First().TvdbId.Should().Be(1);
        }
    }

    public class ImportListExclusions192
    {
        public int Id { get; set; }
        public int TvdbId { get; set; }
        public string Title { get; set; }
    }
}
