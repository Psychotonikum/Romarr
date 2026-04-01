using System.IO;
using NUnit.Framework;
using Romarr.Core.Datastore.Migration.Framework;
using Romarr.Test.Common.Datastore;

namespace Romarr.Core.Test
{
    [SetUpFixture]
    public class RemoveCachedDatabase
    {
        [OneTimeSetUp]
        [OneTimeTearDown]
        public void ClearCachedDatabase()
        {
            var mainCache = SqliteDatabase.GetCachedDb(MigrationType.Main);
            if (File.Exists(mainCache))
            {
                File.Delete(mainCache);
            }

            var logCache = SqliteDatabase.GetCachedDb(MigrationType.Log);
            if (File.Exists(logCache))
            {
                File.Delete(logCache);
            }
        }
    }
}
