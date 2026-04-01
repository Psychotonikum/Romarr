using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Romarr.Common.Disk;
using Romarr.Common.Extensions;
using Romarr.Core.Configuration;
using Romarr.Core.DecisionEngine.Specifications;
using Romarr.Core.Parser.Model;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;
using Romarr.Test.Common;

namespace Romarr.Core.Test.DecisionEngineTests
{
    public class FreeSpaceSpecificationFixture : CoreTest<FreeSpaceSpecification>
    {
        private RemoteRom _remoteRom;

        [SetUp]
        public void Setup()
        {
            _remoteRom = new RemoteRom() { Release = new ReleaseInfo(), Game = new Game { Path = @"C:\Test\TV\Game".AsOsAgnostic() } };
        }

        private void WithMinimumFreeSpace(int size)
        {
            Mocker.GetMock<IConfigService>().SetupGet(c => c.MinimumFreeSpaceWhenImporting).Returns(size);
        }

        private void WithAvailableSpace(int size)
        {
            Mocker.GetMock<IDiskProvider>().Setup(s => s.GetAvailableSpace(It.IsAny<string>())).Returns(size.Megabytes());
        }

        private void WithSize(int size)
        {
            _remoteRom.Release.Size = size.Megabytes();
        }

        [Test]
        public void should_return_true_when_available_space_is_more_than_size()
        {
            WithMinimumFreeSpace(0);
            WithAvailableSpace(200);
            WithSize(100);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_available_space_minus_size_is_more_than_minimum_free_space()
        {
            WithMinimumFreeSpace(50);
            WithAvailableSpace(200);
            WithSize(100);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_available_space_is_less_than_size()
        {
            WithMinimumFreeSpace(0);
            WithAvailableSpace(200);
            WithSize(1000);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_when_available_space_minus_size_is_less_than_minimum_free_space()
        {
            WithMinimumFreeSpace(150);
            WithAvailableSpace(200);
            WithSize(100);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_skip_free_space_check_is_true()
        {
            Mocker.GetMock<IConfigService>()
                .Setup(s => s.SkipFreeSpaceCheckWhenImporting)
                .Returns(true);

            WithMinimumFreeSpace(150);
            WithAvailableSpace(200);
            WithSize(100);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_root_folder_is_not_available()
        {
            WithMinimumFreeSpace(150);
            WithSize(100);

            Mocker.GetMock<IDiskProvider>().Setup(s => s.GetAvailableSpace(It.IsAny<string>())).Throws<DirectoryNotFoundException>();

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }
    }
}
