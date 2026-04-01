using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.Test.Framework;
using Romarr.Core.ThingiProvider;

namespace Romarr.Core.Test.ThingiProviderTests
{
    [TestFixture]
    public class NullConfigFixture : CoreTest<NullConfig>
    {
        [Test]
        public void should_be_valid()
        {
            Subject.Validate().IsValid.Should().BeTrue();
        }
    }
}
