using FluentAssertions;
using Moq;
using NUnit.Framework;
using Romarr.Core.MetadataSource;
using Romarr.Core.MetadataSource.SkyHook;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;
using Romarr.Test.Common;
using Romarr.Test.Common.Categories;

namespace Romarr.Core.Test.MetadataSource.SkyHook
{
    [TestFixture]
    [IntegrationTest]
    public class SkyHookProxySearchFixture : CoreTest<SkyHookProxy>
    {
        [SetUp]
        public void Setup()
        {
            UseRealHttp();
            UseRealIgdb();
        }

        [TestCase("The Witcher 3", "The Witcher 3: Wild Hunt")]
        [TestCase("Portal 2", "Portal 2")]
        [TestCase("Grand Theft Auto V", "Grand Theft Auto V")]
        [TestCase("Final Fantasy VII", "Final Fantasy VII")]
        [TestCase("igdb:1942", "The Witcher 3: Wild Hunt")]
        [TestCase("igdbid:1942", "The Witcher 3: Wild Hunt")]
        [TestCase("igdbid: 1942 ", "The Witcher 3: Wild Hunt")]
        public void successful_search(string title, string expected)
        {
            var result = Subject.SearchForNewGame(title);

            result.Should().NotBeEmpty();

            result.Should().Contain(r => r.Title == expected);

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("tt0496424")]
        [Ignore("IMDB search not supported for games")]
        public void should_search_by_imdb(string title, string expected)
        {
            var result = Subject.SearchForNewGameByImdbId(title);

            result.Should().NotBeEmpty();

            result[0].Title.Should().Be(expected);

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("4565se")]
        public void should_not_search_by_imdb_if_invalid(string title)
        {
            var result = Subject.SearchForNewGameByImdbId(title);
            result.Should().BeEmpty();

            Mocker.GetMock<ISearchForNewGame>()
                  .Verify(v => v.SearchForNewGame(It.IsAny<string>()), Times.Never());

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("igdbid:")]
        [TestCase("igdbid: 99999999999999999999")]
        [TestCase("igdbid: 0")]
        [TestCase("igdbid: -12")]
        [TestCase("adjalkwdjkalwdjklawjdlKAJD")]
        public void no_search_result(string term)
        {
            var result = Subject.SearchForNewGame(term);
            result.Should().BeEmpty();

            ExceptionVerification.IgnoreWarns();
        }

        [TestCase("igdbid:1942")]
        [TestCase("The Witcher 3")]
        public void should_return_existing_series_if_found(string term)
        {
            const int igdbId = 1942;
            var existingGame = new Game
            {
                IgdbId = igdbId
            };

            Mocker.GetMock<IGameService>().Setup(c => c.FindByIgdbId(igdbId)).Returns(existingGame);

            var result = Subject.SearchForNewGame("igdbid: " + igdbId);

            result.Should().Contain(existingGame);
            result.Should().ContainSingle(c => c.IgdbId == igdbId);
        }
    }
}
