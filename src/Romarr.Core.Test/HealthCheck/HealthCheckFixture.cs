using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.HealthCheck;
using Romarr.Core.Test.Framework;

namespace Romarr.Core.Test.HealthCheck
{
    [TestFixture]
    public class HealthCheckFixture : CoreTest
    {
        private const string WikiRoot = "https://wiki.servarr.com/";
        [TestCase("I blew up because of some weird user mistake", null, WikiRoot + "romarr/system#i-blew-up-because-of-some-weird-user-mistake")]
        [TestCase("I blew up because of some weird user mistake", "#my-health-check", WikiRoot + "romarr/system#my-health-check")]
        [TestCase("I blew up because of some weird user mistake", "custom_page#my-health-check", WikiRoot + "romarr/custom_page#my-health-check")]
        public void should_format_wiki_url(string message, string wikiFragment, string expectedUrl)
        {
            var subject = new Romarr.Core.HealthCheck.HealthCheck(typeof(HealthCheckBase), HealthCheckResult.Warning, HealthCheckReason.ServerNotification, message, wikiFragment);

            subject.WikiUrl.Should().Be(expectedUrl);
        }
    }
}
