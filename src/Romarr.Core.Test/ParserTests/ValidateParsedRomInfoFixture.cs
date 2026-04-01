using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.Parser;
using Romarr.Core.Parser.Model;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;
using Romarr.Test.Common;

namespace Romarr.Core.Test.ParserTests
{
    [TestFixture]
    public class ValidateParsedRomInfoFixture : CoreTest
    {
        private ParsedRomInfo _parsedRomInfo;
        private Game _series;

        [SetUp]
        public void Setup()
        {
            _parsedRomInfo = Builder<ParsedRomInfo>.CreateNew()
                                                           .With(p => p.AirDate = null)
                                                           .Build();

            _series = Builder<Game>.CreateNew()
                                     .With(s => s.GameType = GameTypes.Standard)
                                     .Build();
        }

        private void GivenDailyParsedRomInfo()
        {
            _parsedRomInfo.AirDate = "2018-05-21";
        }

        private void GivenDailySeries()
        {
            _series.GameType = GameTypes.Standard;
        }

        [Test]
        public void should_return_true_if_gameFile_info_is_not_daily()
        {
            ValidateParsedRomInfo.ValidateForGameType(_parsedRomInfo, _series).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_gameFile_info_is_daily_for_daily_series()
        {
            GivenDailyParsedRomInfo();
            GivenDailySeries();

            ValidateParsedRomInfo.ValidateForGameType(_parsedRomInfo, _series).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_gameFile_info_is_daily_for_standard_series()
        {
            GivenDailyParsedRomInfo();

            ValidateParsedRomInfo.ValidateForGameType(_parsedRomInfo, _series).Should().BeTrue();
        }

        [Test]
        public void should_not_log_warning_if_warnIfInvalid_is_false()
        {
            GivenDailyParsedRomInfo();

            ValidateParsedRomInfo.ValidateForGameType(_parsedRomInfo, _series, false);
            ExceptionVerification.ExpectedWarns(0);
        }
    }
}
