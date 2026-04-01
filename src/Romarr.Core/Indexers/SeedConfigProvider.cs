using System;
using Romarr.Core.Download.Clients;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.Indexers
{
    public interface ISeedConfigProvider
    {
        TorrentSeedConfiguration GetSeedConfiguration(RemoteRom release);
        TorrentSeedConfiguration GetSeedConfiguration(int indexerId, bool fullPlatform);
    }

    public class SeedConfigProvider : ISeedConfigProvider
    {
        private readonly ICachedIndexerSettingsProvider _cachedIndexerSettingsProvider;

        public SeedConfigProvider(ICachedIndexerSettingsProvider cachedIndexerSettingsProvider)
        {
            _cachedIndexerSettingsProvider = cachedIndexerSettingsProvider;
        }

        public TorrentSeedConfiguration GetSeedConfiguration(RemoteRom remoteRom)
        {
            if (remoteRom.Release.DownloadProtocol != DownloadProtocol.Torrent)
            {
                return null;
            }

            if (remoteRom.Release.IndexerId == 0)
            {
                return null;
            }

            return GetSeedConfiguration(remoteRom.Release.IndexerId, remoteRom.ParsedRomInfo.FullPlatform);
        }

        public TorrentSeedConfiguration GetSeedConfiguration(int indexerId, bool fullPlatform)
        {
            if (indexerId == 0)
            {
                return null;
            }

            var settings = _cachedIndexerSettingsProvider.GetSettings(indexerId);
            var seedCriteria = settings?.SeedCriteriaSettings;

            if (seedCriteria == null)
            {
                return null;
            }

            var usePlatformPackSeedGoal = (PlatformPackSeedGoal)seedCriteria.PlatformPackSeedGoal == PlatformPackSeedGoal.UsePlatformPackSeedGoal;

            var seedConfig = new TorrentSeedConfiguration
            {
                Ratio = (fullPlatform && usePlatformPackSeedGoal)
                    ? seedCriteria.PlatformPackSeedRatio
                    : seedCriteria.SeedRatio
            };

            var seedTime = (fullPlatform && usePlatformPackSeedGoal) ? seedCriteria.PlatformPackSeedTime : seedCriteria.SeedTime;
            if (seedTime.HasValue)
            {
                seedConfig.SeedTime = TimeSpan.FromMinutes(seedTime.Value);
            }

            return seedConfig;
        }
    }
}
