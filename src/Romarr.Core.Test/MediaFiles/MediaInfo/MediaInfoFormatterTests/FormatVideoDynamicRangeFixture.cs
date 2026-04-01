using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.MediaFiles.MediaInfo;
using Romarr.Test.Common;

namespace Romarr.Core.Test.MediaFiles.MediaInfo.MediaInfoFormatterTests
{
    [TestFixture]
    public class FormatVideoDynamicRangeFixture : TestBase
    {
        [TestCase(HdrFormat.None, "")]
        [TestCase(HdrFormat.Hlg10, "HDR")]
        [TestCase(HdrFormat.Pq10, "HDR")]
        [TestCase(HdrFormat.Hdr10, "HDR")]
        [TestCase(HdrFormat.Hdr10Plus, "HDR")]
        [TestCase(HdrFormat.DolbyVision, "HDR")]
        public void should_format_video_dynamic_range(HdrFormat format, string expectedVideoDynamicRange)
        {
            var mediaInfo = new MediaInfoModel
            {
                VideoHdrFormat = format,
                SchemaRevision = 8
            };

            MediaInfoFormatter.FormatVideoDynamicRange(mediaInfo).Should().Be(expectedVideoDynamicRange);
        }
    }
}
