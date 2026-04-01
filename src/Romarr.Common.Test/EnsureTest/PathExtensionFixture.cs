using NUnit.Framework;
using Romarr.Common.Disk;
using Romarr.Common.EnsureThat;
using Romarr.Test.Common;

namespace Romarr.Common.Test.EnsureTest
{
    [TestFixture]
    public class PathExtensionFixture : TestBase
    {
        [TestCase(@"p:\TV Shows\file with, comma.mkv")]
        [TestCase(@"\\server\share\file with, comma.mkv")]
        public void EnsureWindowsPath(string path)
        {
            WindowsOnly();
            Ensure.That(path, () => path).IsValidPath(PathValidationType.CurrentOs);
        }

        [TestCase(@"/var/user/file with, comma.mkv")]
        public void EnsureLinuxPath(string path)
        {
            PosixOnly();
            Ensure.That(path, () => path).IsValidPath(PathValidationType.CurrentOs);
        }
    }
}
