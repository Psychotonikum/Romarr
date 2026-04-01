using NUnit.Framework;

namespace Romarr.Integration.Test.ApiTests.WantedTests
{
    [TestFixture]
    [Ignore("Wanted/Missing depends on ROM gameFiles which are not auto-created for games")]
    public class MissingFixture : IntegrationTest
    {
        [Test]
        public void placeholder()
        {
            Assert.Pass("Missing ROM tests require disk-based setup");
        }
    }
}
