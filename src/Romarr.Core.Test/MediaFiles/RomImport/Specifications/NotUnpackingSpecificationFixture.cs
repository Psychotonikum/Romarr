using System;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Romarr.Common.Disk;
using Romarr.Core.Configuration;
using Romarr.Core.MediaFiles.GameFileImport.Specifications;
using Romarr.Core.Parser.Model;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;
using Romarr.Test.Common;

namespace Romarr.Core.Test.MediaFiles.GameFileImport.Specifications
{
    [TestFixture]
    public class NotUnpackingSpecificationFixture : CoreTest<NotUnpackingSpecification>
    {
        private LocalGameFile _localRom;

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IConfigService>()
                .SetupGet(s => s.DownloadClientWorkingFolders)
                .Returns("_UNPACK_|_FAILED_");

            _localRom = new LocalGameFile
            {
                Path = @"C:\Test\Unsorted TV\30.rock\30.rock.s01e01.avi".AsOsAgnostic(),
                Size = 100,
                Game = Builder<Game>.CreateNew().Build()
            };
        }

        private void GivenInWorkingFolder()
        {
            _localRom.Path = @"C:\Test\Unsorted TV\_UNPACK_30.rock\someSubFolder\30.rock.s01e01.avi".AsOsAgnostic();
        }

        private void GivenLastWriteTimeUtc(DateTime time)
        {
            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.FileGetLastWrite(It.IsAny<string>()))
                .Returns(time);
        }

        [Test]
        public void should_return_true_if_not_in_working_folder()
        {
            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_in_old_working_folder()
        {
            WindowsOnly();

            GivenInWorkingFolder();
            GivenLastWriteTimeUtc(DateTime.UtcNow.AddHours(-1));

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_in_working_folder_and_last_write_time_was_recent()
        {
            GivenInWorkingFolder();
            GivenLastWriteTimeUtc(DateTime.UtcNow);

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_unopacking_on_linux()
        {
            PosixOnly();

            GivenInWorkingFolder();
            GivenLastWriteTimeUtc(DateTime.UtcNow.AddDays(-5));

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeFalse();
        }
    }
}
