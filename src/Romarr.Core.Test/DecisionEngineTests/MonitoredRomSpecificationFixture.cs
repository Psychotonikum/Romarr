using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.DecisionEngine;
using Romarr.Core.DecisionEngine.Specifications.RssSync;
using Romarr.Core.IndexerSearch.Definitions;
using Romarr.Core.Parser.Model;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;

namespace Romarr.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class MonitoredFileSpecificationFixture : CoreTest<MonitoredFileSpecification>
    {
        private MonitoredFileSpecification _monitoredGameFileSpecification;

        private RemoteRom _parseResultMulti;
        private RemoteRom _parseResultSingle;
        private Game _fakeSeries;
        private Rom _firstGameFile;
        private Rom _secondGameFile;

        [SetUp]
        public void Setup()
        {
            _monitoredGameFileSpecification = Mocker.Resolve<MonitoredFileSpecification>();

            _fakeSeries = Builder<Game>.CreateNew()
                .With(c => c.Monitored = true)
                .Build();

            _firstGameFile = new Rom { Monitored = true };
            _secondGameFile = new Rom { Monitored = true };

            var singleGameFileList = new List<Rom> { _firstGameFile };
            var doubleGameFileList = new List<Rom> { _firstGameFile, _secondGameFile };

            _parseResultMulti = new RemoteRom
            {
                Game = _fakeSeries,
                Roms = doubleGameFileList
            };

            _parseResultSingle = new RemoteRom
            {
                Game = _fakeSeries,
                Roms = singleGameFileList
            };
        }

        private void WithFirstGameFileUnmonitored()
        {
            _firstGameFile.Monitored = false;
        }

        private void WithSecondGameFileUnmonitored()
        {
            _secondGameFile.Monitored = false;
        }

        [Test]
        public void setup_should_return_monitored_gameFile_should_return_true()
        {
            _monitoredGameFileSpecification.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeTrue();
            _monitoredGameFileSpecification.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void not_monitored_series_should_be_skipped()
        {
            _fakeSeries.Monitored = false;
            _monitoredGameFileSpecification.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void only_gameFile_not_monitored_should_return_false()
        {
            WithFirstGameFileUnmonitored();
            _monitoredGameFileSpecification.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void both_gameFiles_not_monitored_should_return_false()
        {
            WithFirstGameFileUnmonitored();
            WithSecondGameFileUnmonitored();
            _monitoredGameFileSpecification.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void only_first_gameFile_not_monitored_should_return_false()
        {
            WithFirstGameFileUnmonitored();
            _monitoredGameFileSpecification.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void only_second_gameFile_not_monitored_should_return_false()
        {
            WithSecondGameFileUnmonitored();
            _monitoredGameFileSpecification.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_for_single_gameFile_search()
        {
            _fakeSeries.Monitored = false;
            _monitoredGameFileSpecification.IsSatisfiedBy(_parseResultSingle, new ReleaseDecisionInformation(false, new SingleGameFileSearchCriteria())).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_gameFile_is_monitored_for_platform_search()
        {
            _monitoredGameFileSpecification.IsSatisfiedBy(_parseResultSingle, new ReleaseDecisionInformation(false, new PlatformSearchCriteria())).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_gameFile_is_not_monitored_for_platform_search()
        {
            WithFirstGameFileUnmonitored();
            _monitoredGameFileSpecification.IsSatisfiedBy(_parseResultSingle, new ReleaseDecisionInformation(false, new PlatformSearchCriteria { MonitoredGameFilesOnly = true })).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_gameFile_is_not_monitored_and_monitoredGameFilesOnly_flag_is_false()
        {
            WithFirstGameFileUnmonitored();
            _monitoredGameFileSpecification.IsSatisfiedBy(_parseResultSingle, new ReleaseDecisionInformation(false, new SingleGameFileSearchCriteria { MonitoredGameFilesOnly = false })).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_gameFile_is_not_monitored_and_monitoredGameFilesOnly_flag_is_true()
        {
            WithFirstGameFileUnmonitored();
            _monitoredGameFileSpecification.IsSatisfiedBy(_parseResultSingle, new ReleaseDecisionInformation(false, new SingleGameFileSearchCriteria { MonitoredGameFilesOnly = true })).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_all_gameFiles_are_not_monitored_for_platform_pack_release()
        {
            WithSecondGameFileUnmonitored();
            _parseResultMulti.ParsedRomInfo = new ParsedRomInfo
                                                  {
                                                    FullPlatform = true
                                                  };

            _monitoredGameFileSpecification.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }
    }
}
