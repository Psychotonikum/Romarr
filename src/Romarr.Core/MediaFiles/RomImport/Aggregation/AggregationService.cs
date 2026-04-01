using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Romarr.Common.Disk;
using Romarr.Core.Configuration;
using Romarr.Core.Download;
using Romarr.Core.MediaFiles.GameFileImport.Aggregation.Aggregators;
using Romarr.Core.MediaFiles.MediaInfo;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.MediaFiles.GameFileImport.Aggregation
{
    public interface IAggregationService
    {
        LocalGameFile Augment(LocalGameFile localRom, DownloadClientItem downloadClientItem);
    }

    public class AggregationService : IAggregationService
    {
        private readonly IEnumerable<IAggregateLocalGameFile> _augmenters;
        private readonly IDiskProvider _diskProvider;
        private readonly IGameFileInfoReader _videoFileInfoReader;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public AggregationService(IEnumerable<IAggregateLocalGameFile> augmenters,
                                 IDiskProvider diskProvider,
                                 IGameFileInfoReader videoFileInfoReader,
                                 IConfigService configService,
                                 Logger logger)
        {
            _augmenters = augmenters.OrderBy(a => a.Order).ToList();
            _diskProvider = diskProvider;
            _videoFileInfoReader = videoFileInfoReader;
            _configService = configService;
            _logger = logger;
        }

        public LocalGameFile Augment(LocalGameFile localRom, DownloadClientItem downloadClientItem)
        {
            var isMediaFile = MediaFileExtensions.Extensions.Contains(Path.GetExtension(localRom.Path));

            if (localRom.DownloadClientRomInfo == null &&
                localRom.FolderRomInfo == null &&
                localRom.FileRomInfo == null)
            {
                if (isMediaFile)
                {
                    throw new AugmentingFailedException("Unable to parse rom info from path: {0}", localRom.Path);
                }
            }

            localRom.Size = _diskProvider.GetFileSize(localRom.Path);
            localRom.SceneName = localRom.SceneSource ? SceneNameCalculator.GetSceneName(localRom) : null;

            if (isMediaFile && (!localRom.ExistingFile || _configService.EnableMediaInfo))
            {
                localRom.MediaInfo = _videoFileInfoReader.GetMediaInfo(localRom.Path);
            }

            foreach (var augmenter in _augmenters)
            {
                try
                {
                    augmenter.Aggregate(localRom, downloadClientItem);
                }
                catch (Exception ex)
                {
                    var message = $"Unable to augment information for file: '{localRom.Path}'. Game: {localRom.Game} Error: {ex.Message}";

                    _logger.Warn(ex, message);
                }
            }

            return localRom;
        }
    }
}
