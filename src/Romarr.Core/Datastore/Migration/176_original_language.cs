using FluentMigrator;
using Romarr.Core.Datastore.Migration.Framework;
using Romarr.Core.Languages;

namespace Romarr.Core.Datastore.Migration
{
    [Migration(176)]
    public class original_language : RomarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series")
                .AddColumn("OriginalLanguage").AsInt32().WithDefaultValue((int)Language.English);
        }
    }
}
