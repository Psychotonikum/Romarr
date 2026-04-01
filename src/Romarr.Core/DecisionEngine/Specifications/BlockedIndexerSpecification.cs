using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using Romarr.Common.Cache;
using Romarr.Core.Indexers;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.DecisionEngine.Specifications
{
    public class BlockedIndexerSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly IIndexerStatusService _indexerStatusService;
        private readonly Logger _logger;

        private readonly ICachedDictionary<IndexerStatus> _blockedIndexerCache;

        public BlockedIndexerSpecification(IIndexerStatusService indexerStatusService, ICacheManager cacheManager, Logger logger)
        {
            _indexerStatusService = indexerStatusService;
            _logger = logger;

            _blockedIndexerCache = cacheManager.GetCacheDictionary(GetType(), "blocked", FetchBlockedIndexer, TimeSpan.FromSeconds(15));
        }

        public SpecificationPriority Priority => SpecificationPriority.Database;
        public RejectionType Type => RejectionType.Temporary;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteRom subject, ReleaseDecisionInformation information)
        {
            var status = _blockedIndexerCache.Find(subject.Release.IndexerId.ToString());
            if (status != null)
            {
                return DownloadSpecDecision.Reject(DownloadRejectionReason.IndexerDisabled, $"Indexer {subject.Release.Indexer} is blocked till {status.DisabledTill} due to failures, cannot grab release.");
            }

            return DownloadSpecDecision.Accept();
        }

        private IDictionary<string, IndexerStatus> FetchBlockedIndexer()
        {
            return _indexerStatusService.GetBlockedProviders().ToDictionary(v => v.ProviderId.ToString());
        }
    }
}
