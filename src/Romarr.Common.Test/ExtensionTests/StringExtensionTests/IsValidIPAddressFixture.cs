using FluentAssertions;
using NUnit.Framework;
using Romarr.Common.Extensions;

namespace Romarr.Common.Test.ExtensionTests.StringExtensionTests
{
    [TestFixture]
    public class IsValidIPAddressFixture
    {
        [TestCase("192.168.0.1")]
        [TestCase("::1")]
        [TestCase("2001:db8:4006:812::200e")]
        public void should_validate_ip_address(string input)
        {
            input.IsValidIpAddress().Should().BeTrue();
        }

        [TestCase("romarr.tv")]
        public void should_not_parse_non_ip_address(string input)
        {
            input.IsValidIpAddress().Should().BeFalse();
        }
    }
}
