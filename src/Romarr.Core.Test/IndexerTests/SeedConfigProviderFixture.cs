using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Romarr.Core.Indexers;
using Romarr.Core.Indexers.Torznab;
using Romarr.Core.Parser.Model;
using Romarr.Core.Test.Framework;

namespace Romarr.Core.Test.IndexerTests
{
    [TestFixture]
    public class SeedConfigProviderFixture : CoreTest<SeedConfigProvider>
    {
        [Test]
        public void should_not_return_config_for_non_existent_indexer()
        {
            Mocker.GetMock<ICachedIndexerSettingsProvider>()
                  .Setup(v => v.GetSettings(It.IsAny<int>()))
                  .Returns<CachedIndexerSettings>(null);

            var result = Subject.GetSeedConfiguration(new RemoteRom
            {
                Release = new ReleaseInfo
                {
                    DownloadProtocol = DownloadProtocol.Torrent,
                    IndexerId = 0
                }
            });

            result.Should().BeNull();
        }

        [Test]
        public void should_not_return_config_for_invalid_indexer()
        {
            Mocker.GetMock<ICachedIndexerSettingsProvider>()
                  .Setup(v => v.GetSettings(It.IsAny<int>()))
                  .Returns<CachedIndexerSettings>(null);

            var result = Subject.GetSeedConfiguration(new RemoteRom
            {
                Release = new ReleaseInfo
                {
                    DownloadProtocol = DownloadProtocol.Torrent,
                    IndexerId = 1
                },
                ParsedRomInfo = new ParsedRomInfo
                {
                    FullPlatform = true
                }
            });

            result.Should().BeNull();
        }

        [Test]
        public void should_return_platform_time_for_platform_packs()
        {
            var settings = new TorznabSettings();
            settings.SeedCriteria.PlatformPackSeedGoal = (int)PlatformPackSeedGoal.UsePlatformPackSeedGoal;
            settings.SeedCriteria.PlatformPackSeedTime = 10;

            Mocker.GetMock<ICachedIndexerSettingsProvider>()
                     .Setup(v => v.GetSettings(It.IsAny<int>()))
                     .Returns(new CachedIndexerSettings
                     {
                         FailDownloads = new HashSet<FailDownloads> { FailDownloads.Executables },
                         SeedCriteriaSettings = settings.SeedCriteria
                     });

            var result = Subject.GetSeedConfiguration(new RemoteRom
            {
                Release = new ReleaseInfo
                {
                    DownloadProtocol = DownloadProtocol.Torrent,
                    IndexerId = 1
                },
                ParsedRomInfo = new ParsedRomInfo
                {
                    FullPlatform = true
                }
            });

            result.Should().NotBeNull();
            result.SeedTime.Should().Be(TimeSpan.FromMinutes(10));
        }

        [Test]
        public void should_return_platform_ratio_for_platform_packs_when_set()
        {
            var settings = new TorznabSettings();
            settings.SeedCriteria.PlatformPackSeedGoal = (int)PlatformPackSeedGoal.UsePlatformPackSeedGoal;
            settings.SeedCriteria.SeedRatio = 1.0;
            settings.SeedCriteria.PlatformPackSeedRatio = 10.0;

            Mocker.GetMock<ICachedIndexerSettingsProvider>()
                .Setup(v => v.GetSettings(It.IsAny<int>()))
                .Returns(new CachedIndexerSettings
                {
                    FailDownloads = new HashSet<FailDownloads> { FailDownloads.Executables },
                    SeedCriteriaSettings = settings.SeedCriteria
                });

            var result = Subject.GetSeedConfiguration(new RemoteRom
            {
                Release = new ReleaseInfo
                {
                    DownloadProtocol = DownloadProtocol.Torrent,
                    IndexerId = 1
                },
                ParsedRomInfo = new ParsedRomInfo
                {
                    FullPlatform = true
                }
            });

            result.Should().NotBeNull();
            result.Ratio.Should().Be(10.0);
        }

        [Test]
        public void should_return_standard_ratio_for_platform_packs_when_not_set()
        {
            var settings = new TorznabSettings();
            settings.SeedCriteria.PlatformPackSeedGoal = (int)PlatformPackSeedGoal.UseStandardSeedGoal;
            settings.SeedCriteria.SeedRatio = 1.0;
            settings.SeedCriteria.PlatformPackSeedRatio = 10.0;

            Mocker.GetMock<ICachedIndexerSettingsProvider>()
                .Setup(v => v.GetSettings(It.IsAny<int>()))
                .Returns(new CachedIndexerSettings
                {
                    FailDownloads = new HashSet<FailDownloads> { FailDownloads.Executables },
                    SeedCriteriaSettings = settings.SeedCriteria
                });

            var result = Subject.GetSeedConfiguration(new RemoteRom
            {
                Release = new ReleaseInfo
                {
                    DownloadProtocol = DownloadProtocol.Torrent,
                    IndexerId = 1
                },
                ParsedRomInfo = new ParsedRomInfo
                {
                    FullPlatform = true
                }
            });

            result.Should().NotBeNull();
            result.Ratio.Should().Be(1.0);
        }
    }
}
