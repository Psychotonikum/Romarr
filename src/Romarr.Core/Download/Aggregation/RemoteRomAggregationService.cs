using System;
using System.Collections.Generic;
using NLog;
using Romarr.Core.Download.Aggregation.Aggregators;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.Download.Aggregation
{
    public interface IRemoteFileAggregationService
    {
        RemoteRom Augment(RemoteRom remoteRom);
    }

    public class RemoteFileAggregationService : IRemoteFileAggregationService
    {
        private readonly IEnumerable<IAggregateRemoteGameFile> _augmenters;
        private readonly Logger _logger;

        public RemoteFileAggregationService(IEnumerable<IAggregateRemoteGameFile> augmenters,
                                  Logger logger)
        {
            _augmenters = augmenters;
            _logger = logger;
        }

        public RemoteRom Augment(RemoteRom remoteRom)
        {
            if (remoteRom == null)
            {
                return null;
            }

            foreach (var augmenter in _augmenters)
            {
                try
                {
                    augmenter.Aggregate(remoteRom);
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, ex.Message);
                }
            }

            return remoteRom;
        }
    }
}
