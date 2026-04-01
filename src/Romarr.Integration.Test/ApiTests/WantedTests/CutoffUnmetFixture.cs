using NUnit.Framework;

namespace Romarr.Integration.Test.ApiTests.WantedTests
{
    [TestFixture]
    [Ignore("Cutoff tests depend on ROM gameFiles which are not auto-created for games")]
    public class CutoffUnmetFixture : IntegrationTest
    {
        [Test]
        public void placeholder()
        {
            Assert.Pass("Cutoff tests require disk-based ROM files");
        }
    }
}
