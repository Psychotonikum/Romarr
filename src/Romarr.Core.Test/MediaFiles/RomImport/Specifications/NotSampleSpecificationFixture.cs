using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.MediaFiles.GameFileImport.Specifications;
using Romarr.Core.Parser.Model;
using Romarr.Core.Qualities;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;

namespace Romarr.Core.Test.MediaFiles.GameFileImport.Specifications
{
    [TestFixture]
    public class NotSampleSpecificationFixture : CoreTest<NotSampleSpecification>
    {
        private Game _series;
        private LocalGameFile _localRom;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>.CreateNew()
                                     .With(s => s.GameType = GameTypes.Standard)
                                     .Build();

            var roms = Builder<Rom>.CreateListOfSize(1)
                                           .All()
                                           .With(e => e.PlatformNumber = 1)
                                           .Build()
                                           .ToList();

            _localRom = new LocalGameFile
                                {
                                    Path = @"C:\Test\30 Rock\30.rock.s01e01.avi",
                                    Roms = roms,
                                    Game = _series,
                                    Quality = new QualityModel(Quality.HDTV720p)
                                };
        }

        [Test]
        public void should_return_true_for_existing_file()
        {
            _localRom.ExistingFile = true;
            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }
    }
}
