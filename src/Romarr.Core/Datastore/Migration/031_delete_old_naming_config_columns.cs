using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(31)]
    public class delete_old_naming_config_columns : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("Separator")
                  .Column("NumberStyle")
                  .Column("IncludeGameTitle")
                  .Column("IncludeRomTitle")
                  .Column("IncludeQuality")
                  .Column("ReplaceSpaces")
                  .FromTable("NamingConfig");
        }
    }
}
