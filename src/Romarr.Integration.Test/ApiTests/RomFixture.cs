using NUnit.Framework;

namespace Romarr.Integration.Test.ApiTests
{
    [TestFixture]
    [Ignore("Games do not auto-create ROMs from metadata like TV gameFiles - ROM files are created by disk scan")]
    public class GameFileFixture : IntegrationTest
    {
        [Test]
        public void placeholder()
        {
            Assert.Pass("ROM tests require disk-based ROM files, not metadata-derived gameFiles");
        }
    }
}
