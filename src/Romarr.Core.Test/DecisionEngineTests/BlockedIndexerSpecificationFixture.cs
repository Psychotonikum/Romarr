using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.DecisionEngine;
using Romarr.Core.DecisionEngine.Specifications;
using Romarr.Core.Indexers;
using Romarr.Core.Parser.Model;
using Romarr.Core.Test.Framework;

namespace Romarr.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class BlockedIndexerSpecificationFixture : CoreTest<BlockedIndexerSpecification>
    {
        private RemoteRom _remoteRom;

        [SetUp]
        public void Setup()
        {
            _remoteRom = new RemoteRom
            {
                Release = new ReleaseInfo { IndexerId = 1 }
            };

            Mocker.GetMock<IIndexerStatusService>()
                  .Setup(v => v.GetBlockedProviders())
                  .Returns(new List<IndexerStatus>());
        }

        private void WithBlockedIndexer()
        {
            Mocker.GetMock<IIndexerStatusService>()
                  .Setup(v => v.GetBlockedProviders())
                  .Returns(new List<IndexerStatus> { new IndexerStatus { ProviderId = 1, DisabledTill = DateTime.UtcNow } });
        }

        [Test]
        public void should_return_true_if_no_blocked_indexer()
        {
            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_blocked_indexer()
        {
            WithBlockedIndexer();

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
            Subject.Type.Should().Be(RejectionType.Temporary);
        }
    }
}
