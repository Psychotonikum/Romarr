using FluentAssertions;
using NUnit.Framework;
using Romarr.Common.Disk;
using Romarr.Mono.Disk;
using Romarr.Mono.EnvironmentInfo.VersionAdapters;
using Romarr.Test.Common;

namespace Romarr.Mono.Test.EnvironmentInfo
{
    [TestFixture]
    [Platform("Linux")]
    public class ReleaseFileVersionAdapterFixture : TestBase<ReleaseFileVersionAdapter>
    {
        [SetUp]
        public void Setup()
        {
            NotBsd();

            Mocker.SetConstant<IDiskProvider>(Mocker.Resolve<DiskProvider>());
        }

        [Test]
        public void should_get_version_info()
        {
            var info = Subject.Read();
            info.FullName.Should().NotBeNullOrWhiteSpace();
            info.Name.Should().NotBeNullOrWhiteSpace();
            info.Version.Should().NotBeNullOrWhiteSpace();
        }
    }
}
