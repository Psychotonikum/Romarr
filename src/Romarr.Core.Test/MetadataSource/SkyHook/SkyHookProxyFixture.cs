using System;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.Exceptions;
using Romarr.Core.MetadataSource.SkyHook;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;
using Romarr.Test.Common.Categories;

namespace Romarr.Core.Test.MetadataSource.SkyHook
{
    [TestFixture]
    [IntegrationTest]
    public class SkyHookProxyFixture : CoreTest<SkyHookProxy>
    {
        [SetUp]
        public void Setup()
        {
            UseRealHttp();
            UseRealIgdb();
        }

        [TestCase(1942, "The Witcher 3: Wild Hunt")]
        [TestCase(72, "Portal 2")]
        [TestCase(1020, "Grand Theft Auto V")]
        public void should_be_able_to_get_series_detail(int igdbId, string title)
        {
            var details = Subject.GetGameInfo(igdbId);

            ValidateSeries(details.Item1);

            details.Item1.Title.Should().Be(title);
        }

        [Test]
        public void getting_details_of_invalid_series()
        {
            Assert.Throws<GameNotFoundException>(() => Subject.GetGameInfo(int.MaxValue));
        }

        private void ValidateSeries(Game game)
        {
            game.Should().NotBeNull();
            game.Title.Should().NotBeNullOrWhiteSpace();
            game.CleanTitle.Should().Be(Parser.Parser.CleanGameTitle(game.Title));
            game.SortTitle.Should().Be(GameTitleNormalizer.Normalize(game.Title, game.IgdbId));
            game.Overview.Should().NotBeNullOrWhiteSpace();
            game.FirstAired.Should().HaveValue();
            game.FirstAired.Value.Kind.Should().Be(DateTimeKind.Utc);
            game.Images.Should().NotBeEmpty();
            game.TitleSlug.Should().NotBeNullOrWhiteSpace();

            game.IgdbId.Should().BeGreaterThan(0);
        }
    }
}
