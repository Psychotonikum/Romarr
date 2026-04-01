using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.MediaFiles.GameFileImport.Specifications;
using Romarr.Core.Parser.Model;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;
using Romarr.Test.Common;

namespace Romarr.Core.Test.MediaFiles.GameFileImport.Specifications
{
    [TestFixture]
    public class FullPlatformSpecificationFixture : CoreTest<FullPlatformSpecification>
    {
        private LocalGameFile _localRom;

        [SetUp]
        public void Setup()
        {
            _localRom = new LocalGameFile
            {
                Path = @"C:\Test\30 Rock\30.rock.s01e01.avi".AsOsAgnostic(),
                Size = 100,
                Game = Builder<Game>.CreateNew().Build(),
                FileRomInfo = new ParsedRomInfo
                                    {
                                        FullPlatform = false
                                    }
            };
        }

        [Test]
        public void should_return_true_if_no_fileinfo_available()
        {
            _localRom.FileRomInfo = null;
            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_file_contains_the_full_platform()
        {
            _localRom.FileRomInfo.FullPlatform = true;

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_file_does_not_contain_the_full_platform()
        {
            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }
    }
}
