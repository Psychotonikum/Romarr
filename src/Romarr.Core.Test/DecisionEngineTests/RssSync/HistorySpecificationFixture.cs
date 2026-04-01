using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Romarr.Core.Configuration;
using Romarr.Core.CustomFormats;
using Romarr.Core.DecisionEngine;
using Romarr.Core.DecisionEngine.Specifications;
using Romarr.Core.DecisionEngine.Specifications.RssSync;
using Romarr.Core.History;
using Romarr.Core.IndexerSearch.Definitions;
using Romarr.Core.Languages;
using Romarr.Core.Parser.Model;
using Romarr.Core.Profiles.Qualities;
using Romarr.Core.Qualities;
using Romarr.Core.Test.CustomFormats;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;

namespace Romarr.Core.Test.DecisionEngineTests.RssSync
{
    [TestFixture]
    public class HistorySpecificationFixture : CoreTest<HistorySpecification>
    {
        private const int FIRST_EPISODE_ID = 1;
        private const int SECOND_EPISODE_ID = 2;

        private HistorySpecification _upgradeHistory;

        private RemoteRom _parseResultMulti;
        private RemoteRom _parseResultSingle;
        private Tuple<QualityModel, Language> _upgradableQuality;
        private Tuple<QualityModel, Language> _notupgradableQuality;
        private Game _fakeSeries;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<UpgradableSpecification>();
            _upgradeHistory = Mocker.Resolve<HistorySpecification>();

            CustomFormatsTestHelpers.GivenCustomFormats();

            var singleGameFileList = new List<Rom> { new Rom { Id = FIRST_EPISODE_ID, PlatformNumber = 12, FileNumber = 3 } };
            var doubleGameFileList = new List<Rom>
            {
                                                            new Rom { Id = FIRST_EPISODE_ID, PlatformNumber = 12, FileNumber = 3 },
                                                            new Rom { Id = SECOND_EPISODE_ID, PlatformNumber = 12, FileNumber = 4 },
                                                            new Rom { Id = 3, PlatformNumber = 12, FileNumber = 5 }
            };

            _fakeSeries = Builder<Game>.CreateNew()
                .With(c => c.QualityProfile = new QualityProfile
                {
                    UpgradeAllowed = true,
                    Cutoff = Quality.Bluray1080p.Id,
                    FormatItems = CustomFormatsTestHelpers.GetSampleFormatItems("None"),
                    MinFormatScore = 0,
                    Items = Qualities.QualityFixture.GetDefaultQualities()
                })
                .Build();

            _parseResultMulti = new RemoteRom
            {
                Game = _fakeSeries,
                ParsedRomInfo = new ParsedRomInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)), Languages = new List<Language> { Language.English } },
                Roms = doubleGameFileList,
                CustomFormats = new List<CustomFormat>()
            };

            _parseResultSingle = new RemoteRom
            {
                Game = _fakeSeries,
                ParsedRomInfo = new ParsedRomInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)), Languages = new List<Language> { Language.English } },
                Roms = singleGameFileList,
                CustomFormats = new List<CustomFormat>()
            };

            _upgradableQuality = new Tuple<QualityModel, Language>(new QualityModel(Quality.SDTV, new Revision(version: 1)), Language.English);

            _notupgradableQuality = new Tuple<QualityModel, Language>(new QualityModel(Quality.HDTV1080p, new Revision(version: 2)), Language.English);

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.EnableCompletedDownloadHandling)
                  .Returns(true);

            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(x => x.ParseCustomFormat(It.IsAny<FileHistory>(), It.IsAny<Game>()))
                  .Returns(new List<CustomFormat>());
        }

        private void GivenMostRecentForRom(int romId, string downloadId, Tuple<QualityModel, Language> quality, DateTime date, FileHistoryEventType eventType)
        {
            Mocker.GetMock<IHistoryService>().Setup(s => s.MostRecentForRom(romId))
                  .Returns(new FileHistory { DownloadId = downloadId, Quality = quality.Item1, Date = date, EventType = eventType, Languages = new List<Language> { quality.Item2 } });
        }

        private void GivenCdhDisabled()
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.EnableCompletedDownloadHandling)
                  .Returns(false);
        }

        [Test]
        public void should_return_true_if_it_is_a_search()
        {
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new ReleaseDecisionInformation(false, new PlatformSearchCriteria())).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_latest_history_item_is_null()
        {
            Mocker.GetMock<IHistoryService>().Setup(s => s.MostRecentForRom(It.IsAny<int>())).Returns((FileHistory)null);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_latest_history_item_is_not_grabbed()
        {
            GivenMostRecentForRom(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, FileHistoryEventType.DownloadFailed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
        }

// [Test]
//        public void should_return_true_if_latest_history_has_a_download_id_and_cdh_is_enabled()
//        {
//            GivenMostRecentForRom(FIRST_EPISODE_ID, "test", _notupgradableQuality, DateTime.UtcNow, HistoryEventType.Grabbed);
//            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
//        }

        [Test]
        public void should_return_true_if_latest_history_item_is_older_than_twelve_hours()
        {
            GivenMostRecentForRom(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow.AddHours(-13), FileHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_upgradable_if_only_gameFile_is_upgradable()
        {
            GivenMostRecentForRom(FIRST_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, FileHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_upgradable_if_both_gameFiles_are_upgradable()
        {
            GivenMostRecentForRom(FIRST_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, FileHistoryEventType.Grabbed);
            GivenMostRecentForRom(SECOND_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, FileHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_not_be_upgradable_if_both_gameFiles_are_not_upgradable()
        {
            GivenMostRecentForRom(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, FileHistoryEventType.Grabbed);
            GivenMostRecentForRom(SECOND_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, FileHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_not_upgradable_if_only_first_gameFiles_is_upgradable()
        {
            GivenMostRecentForRom(FIRST_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, FileHistoryEventType.Grabbed);
            GivenMostRecentForRom(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, FileHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_not_upgradable_if_only_second_gameFiles_is_upgradable()
        {
            GivenMostRecentForRom(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow, FileHistoryEventType.Grabbed);
            GivenMostRecentForRom(SECOND_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, FileHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_not_be_upgradable_if_gameFile_is_of_same_quality_as_existing()
        {
            _fakeSeries.QualityProfile = new QualityProfile { Cutoff = Quality.Bluray1080p.Id, Items = Qualities.QualityFixture.GetDefaultQualities() };
            _parseResultSingle.ParsedRomInfo.Quality = new QualityModel(Quality.WEBDL1080p, new Revision(version: 1));
            _upgradableQuality = new Tuple<QualityModel, Language>(new QualityModel(Quality.WEBDL1080p, new Revision(version: 1)), Language.English);

            GivenMostRecentForRom(FIRST_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, FileHistoryEventType.Grabbed);

            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_not_be_upgradable_if_cutoff_already_met()
        {
            _fakeSeries.QualityProfile = new QualityProfile { Cutoff = Quality.WEBDL1080p.Id, Items = Qualities.QualityFixture.GetDefaultQualities() };
            _parseResultSingle.ParsedRomInfo.Quality = new QualityModel(Quality.WEBDL1080p, new Revision(version: 1));
            _upgradableQuality = new Tuple<QualityModel, Language>(new QualityModel(Quality.WEBDL1080p, new Revision(version: 1)), Language.Spanish);

            GivenMostRecentForRom(FIRST_EPISODE_ID, string.Empty, _upgradableQuality, DateTime.UtcNow, FileHistoryEventType.Grabbed);

            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_latest_history_item_is_only_one_hour_old()
        {
            GivenMostRecentForRom(FIRST_EPISODE_ID, string.Empty, _notupgradableQuality, DateTime.UtcNow.AddHours(-1), FileHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_latest_history_has_a_download_id_and_cdh_is_disabled()
        {
            GivenCdhDisabled();
            GivenMostRecentForRom(FIRST_EPISODE_ID, "test", _upgradableQuality, DateTime.UtcNow.AddDays(-100), FileHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultMulti, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_cutoff_already_met_and_cdh_is_disabled()
        {
            GivenCdhDisabled();
            _fakeSeries.QualityProfile = new QualityProfile { Cutoff = Quality.WEBDL1080p.Id, Items = Qualities.QualityFixture.GetDefaultQualities() };
            _parseResultSingle.ParsedRomInfo.Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 1));
            _upgradableQuality = new Tuple<QualityModel, Language>(new QualityModel(Quality.WEBDL1080p, new Revision(version: 1)), Language.Spanish);

            GivenMostRecentForRom(FIRST_EPISODE_ID, "test", _upgradableQuality, DateTime.UtcNow.AddDays(-100), FileHistoryEventType.Grabbed);

            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_only_gameFile_is_not_upgradable_and_cdh_is_disabled()
        {
            GivenCdhDisabled();
            GivenMostRecentForRom(FIRST_EPISODE_ID, "test", _notupgradableQuality, DateTime.UtcNow.AddDays(-100), FileHistoryEventType.Grabbed);
            _upgradeHistory.IsSatisfiedBy(_parseResultSingle, new()).Accepted.Should().BeFalse();
        }
    }
}
