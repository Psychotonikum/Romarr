using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.DecisionEngine;
using Romarr.Core.DecisionEngine.Specifications.Search;
using Romarr.Core.IndexerSearch.Definitions;
using Romarr.Core.Parser.Model;
using Romarr.Core.Games;
using Romarr.Test.Common;

namespace Romarr.Core.Test.DecisionEngineTests.Search
{
    [TestFixture]
    public class GameSpecificationFixture : TestBase<GameSpecification>
    {
        private Game _series1;
        private Game _series2;
        private RemoteRom _remoteRom = new();
        private SearchCriteriaBase _searchCriteria = new SingleGameFileSearchCriteria();
        private ReleaseDecisionInformation _information;

        [SetUp]
        public void Setup()
        {
            _series1 = Builder<Game>.CreateNew().With(s => s.Id = 1).Build();
            _series2 = Builder<Game>.CreateNew().With(s => s.Id = 2).Build();

            _remoteRom.Game = _series1;
            _information = new ReleaseDecisionInformation(false, _searchCriteria);
        }

        [Test]
        public void should_return_false_if_series_doesnt_match()
        {
            _searchCriteria.Game = _series2;

            Subject.IsSatisfiedBy(_remoteRom, _information).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_series_ids_match()
        {
            _searchCriteria.Game = _series1;

            Subject.IsSatisfiedBy(_remoteRom, _information).Accepted.Should().BeTrue();
        }
    }
}
