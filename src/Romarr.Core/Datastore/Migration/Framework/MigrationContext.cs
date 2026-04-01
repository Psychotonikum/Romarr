using System;

namespace Romarr.Core.Datastore.Migration.Framework
{
    public class MigrationContext
    {
        [ThreadStatic]
        private static MigrationContext _current;

        public static MigrationContext Current
        {
            get => _current;
            set => _current = value;
        }

        public MigrationType MigrationType { get; private set; }
        public long? DesiredVersion { get; set; }

        public Action<RomarrMigrationBase> BeforeMigration { get; set; }

        public MigrationContext(MigrationType migrationType, long? desiredVersion = null)
        {
            MigrationType = migrationType;
            DesiredVersion = desiredVersion;
        }
    }
}
