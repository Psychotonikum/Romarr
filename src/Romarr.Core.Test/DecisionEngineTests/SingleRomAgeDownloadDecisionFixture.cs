using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.DecisionEngine;
using Romarr.Core.DecisionEngine.Specifications;
using Romarr.Core.IndexerSearch.Definitions;
using Romarr.Core.Parser.Model;
using Romarr.Core.Qualities;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;

namespace Romarr.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class SingleGameFileAgeDownloadDecisionFixture : CoreTest<PlatformPackOnlySpecification>
    {
        private RemoteRom _parseResultMulti;
        private RemoteRom _parseResultSingle;
        private Game _series;
        private List<Rom> _gameFiles;
        private PlatformSearchCriteria _multiSearch;
        private ReleaseDecisionInformation _multiInfo;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>.CreateNew()
                                    .With(s => s.Platforms = Builder<Platform>.CreateListOfSize(1).Build().ToList())
                                    .With(s => s.GameType = GameTypes.Standard)
                                    .Build();

            _gameFiles = new List<Rom>();
            _gameFiles.Add(CreateGameFileStub(1, 400));
            _gameFiles.Add(CreateGameFileStub(2, 370));
            _gameFiles.Add(CreateGameFileStub(3, 340));
            _gameFiles.Add(CreateGameFileStub(4, 310));

            _multiSearch = new PlatformSearchCriteria();
            _multiSearch.Roms = _gameFiles.ToList();
            _multiSearch.PlatformNumber = 1;
            _multiInfo = new ReleaseDecisionInformation(false, _multiSearch);

            _parseResultMulti = new RemoteRom
            {
                Game = _series,
                Release = new ReleaseInfo(),
                ParsedRomInfo = new ParsedRomInfo { Quality = new QualityModel(Quality.SDTV, new Revision(version: 2)), FullPlatform = true },
                Roms = _gameFiles.ToList()
            };

            _parseResultSingle = new RemoteRom
            {
                Game = _series,
                Release = new ReleaseInfo(),
                ParsedRomInfo = new ParsedRomInfo { Quality = new QualityModel(Quality.SDTV, new Revision(version: 2)) },
                Roms = new List<Rom>()
            };
        }

        private Rom CreateGameFileStub(int number, int age)
        {
            return new Rom()
                   {
                        PlatformNumber = 1,
                        FileNumber = number,
                        AirDateUtc = DateTime.UtcNow.AddDays(-age)
                   };
        }

        [TestCase(1, 200, false)]
        [TestCase(4, 200, false)]
        [TestCase(1, 600, true)]
        [TestCase(1, 365, true)]
        [TestCase(4, 365, true)]
        [TestCase(1, 0, true)]
        public void single_gameFile_release(int rom, int platformSearchMaximumSingleGameFileAge, bool expectedResult)
        {
            _parseResultSingle.Release.PlatformSearchMaximumSingleFileAge = platformSearchMaximumSingleGameFileAge;
            _parseResultSingle.Roms.Clear();
            _parseResultSingle.Roms.Add(_gameFiles.Find(e => e.FileNumber == rom));

            Subject.IsSatisfiedBy(_parseResultSingle, _multiInfo).Accepted.Should().Be(expectedResult);
        }

        // should always accept all platform packs
        [TestCase(200, true)]
        [TestCase(600, true)]
        [TestCase(365, true)]
        [TestCase(0, true)]
        public void multi_gameFile_release(int platformSearchMaximumSingleGameFileAge, bool expectedResult)
        {
            _parseResultMulti.Release.PlatformSearchMaximumSingleFileAge = platformSearchMaximumSingleGameFileAge;

            Subject.IsSatisfiedBy(_parseResultMulti, _multiInfo).Accepted.Should().BeTrue();
        }
    }
}
