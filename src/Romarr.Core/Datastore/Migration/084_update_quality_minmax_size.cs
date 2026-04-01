using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(84)]
    public class update_quality_minmax_size : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("QualityDefinitions").AlterColumn("MinSize").AsDouble().Nullable();
            Alter.Table("QualityDefinitions").AlterColumn("MaxSize").AsDouble().Nullable();

            Execute.Sql("UPDATE \"QualityDefinitions\" SET \"MaxSize\" = NULL WHERE \"Quality\" = 10 OR \"MaxSize\" = 0");
        }
    }

    public class QualityDefinition84
    {
        public int Id { get; set; }
        public int Quality { get; set; }
        public string Title { get; set; }
        public int? MinSize { get; set; }
        public int? MaxSize { get; set; }
    }
}
