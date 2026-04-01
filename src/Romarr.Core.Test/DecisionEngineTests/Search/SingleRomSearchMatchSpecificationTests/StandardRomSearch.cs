using System;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.DecisionEngine;
using Romarr.Core.DecisionEngine.Specifications.Search;
using Romarr.Core.IndexerSearch.Definitions;
using Romarr.Core.Parser.Model;
using Romarr.Test.Common;

namespace Romarr.Core.Test.DecisionEngineTests.Search.SingleFileSearchMatchSpecificationTests
{
    [TestFixture]
    public class StandardGameFileSearch : TestBase<SingleFileSearchMatchSpecification>
    {
        private RemoteRom _remoteRom = new();
        private SingleGameFileSearchCriteria _searchCriteria = new();
        private ReleaseDecisionInformation _information;

        [SetUp]
        public void Setup()
        {
            _remoteRom.ParsedRomInfo = new ParsedRomInfo();
            _remoteRom.ParsedRomInfo.PlatformNumber = 5;
            _remoteRom.ParsedRomInfo.RomNumbers = new[] { 1 };
            _remoteRom.MappedPlatformNumber = 5;

            _searchCriteria.PlatformNumber = 5;
            _searchCriteria.FileNumber = 1;
            _information = new ReleaseDecisionInformation(false, _searchCriteria);
        }

        [Test]
        public void should_return_false_if_platform_does_not_match()
        {
            _remoteRom.ParsedRomInfo.PlatformNumber = 10;
            _remoteRom.MappedPlatformNumber = 10;

            Subject.IsSatisfiedBy(_remoteRom, _information).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_platform_matches_after_scenemapping()
        {
            _remoteRom.ParsedRomInfo.PlatformNumber = 10;
            _remoteRom.MappedPlatformNumber = 5; // 10 -> 5 mapping
            _searchCriteria.PlatformNumber = 10; // searching by igdb 5 = 10 scene

            Subject.IsSatisfiedBy(_remoteRom, _information).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_platform_does_not_match_after_scenemapping()
        {
            _remoteRom.ParsedRomInfo.PlatformNumber = 10;
            _remoteRom.MappedPlatformNumber = 6; // 9 -> 5 mapping
            _searchCriteria.PlatformNumber = 9; // searching by igdb 5 = 9 scene

            Subject.IsSatisfiedBy(_remoteRom, _information).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_full_platform_result_for_single_gameFile_search()
        {
            _remoteRom.ParsedRomInfo.RomNumbers = Array.Empty<int>();

            Subject.IsSatisfiedBy(_remoteRom, _information).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_gameFile_number_does_not_match_search_criteria()
        {
            _remoteRom.ParsedRomInfo.RomNumbers = new[] { 2 };

            Subject.IsSatisfiedBy(_remoteRom, _information).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_full_platform_result_for_full_platform_search()
        {
            Subject.IsSatisfiedBy(_remoteRom, _information).Accepted.Should().BeTrue();
        }
    }
}
