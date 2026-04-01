using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Common.Extensions;
using Romarr.Core.Indexers;
using Romarr.Core.IndexerSearch.Definitions;
using Romarr.Core.Parser.Model;
using Romarr.Core.Test.Framework;
using Romarr.Test.Common.Categories;

namespace Romarr.Core.Test.IndexerTests.IntegrationTests
{
    [IntegrationTest]
    public class IndexerIntegrationTests : CoreTest
    {
        private SingleGameFileSearchCriteria _singleSearchCriteria;

        [SetUp]
        public void SetUp()
        {
            UseRealHttp();

            _singleSearchCriteria = new SingleGameFileSearchCriteria()
                {
                    SceneTitles = new List<string> { "Person of Interest" },
                    PlatformNumber = 1,
                    FileNumber = 1
                };
        }

        private void ValidateTorrentResult(IList<ReleaseInfo> reports, bool hasSize = false, bool hasInfoUrl = false, bool hasMagnet = false)
        {
            reports.Should().OnlyContain(c => c.GetType() == typeof(TorrentInfo));

            ValidateResult(reports, hasSize, hasInfoUrl);

            reports.Should().OnlyContain(c => c.DownloadProtocol == DownloadProtocol.Torrent);

            if (hasMagnet)
            {
                reports.Cast<TorrentInfo>().Should().OnlyContain(c => c.MagnetUrl.StartsWith("magnet:"));
            }
        }

        private void ValidateResult(IList<ReleaseInfo> reports, bool hasSize = false, bool hasInfoUrl = false)
        {
            reports.Should().NotBeEmpty();
            reports.Should().OnlyContain(c => c.Title.IsNotNullOrWhiteSpace());
            reports.Should().OnlyContain(c => c.PublishDate.Year > 2000);
            reports.Should().OnlyContain(c => c.DownloadUrl.IsNotNullOrWhiteSpace());
            reports.Should().OnlyContain(c => c.DownloadUrl.StartsWith("http"));

            if (hasInfoUrl)
            {
                reports.Should().OnlyContain(c => c.InfoUrl.IsNotNullOrWhiteSpace());
            }

            if (hasSize)
            {
                reports.Should().OnlyContain(c => c.Size > 0);
            }
        }
    }
}
