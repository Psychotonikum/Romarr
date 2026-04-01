using System.IO;
using NUnit.Framework;
using Romarr.Core.Datastore.Migration.Framework;

namespace Romarr.Test.Common.Datastore
{
    public static class SqliteDatabase
    {
        public static string GetCachedDb(MigrationType type)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, $"cached_{type}.db");
        }
    }
}
