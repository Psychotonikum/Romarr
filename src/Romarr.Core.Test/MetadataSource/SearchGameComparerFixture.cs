using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.MetadataSource;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;

namespace Romarr.Core.Test.MetadataSource
{
    [TestFixture]
    public class SearchGameComparerFixture : CoreTest
    {
        private List<Game> _series;

        [SetUp]
        public void Setup()
        {
            _series = new List<Game>();
        }

        private void WithSeries(string title)
        {
            _series.Add(new Game { Title = title });
        }

        [Test]
        public void should_prefer_the_walking_dead_over_talking_dead_when_searching_for_the_walking_dead()
        {
            WithSeries("Talking Dead");
            WithSeries("The Walking Dead");

            _series.Sort(new SearchGameComparer("the walking dead"));

            _series.First().Title.Should().Be("The Walking Dead");
        }

        [Test]
        public void should_prefer_the_walking_dead_over_talking_dead_when_searching_for_walking_dead()
        {
            WithSeries("Talking Dead");
            WithSeries("The Walking Dead");

            _series.Sort(new SearchGameComparer("walking dead"));

            _series.First().Title.Should().Be("The Walking Dead");
        }

        [Test]
        public void should_prefer_blacklist_over_the_blacklist_when_searching_for_blacklist()
        {
            WithSeries("The Blacklist");
            WithSeries("Blacklist");

            _series.Sort(new SearchGameComparer("blacklist"));

            _series.First().Title.Should().Be("Blacklist");
        }

        [Test]
        public void should_prefer_the_blacklist_over_blacklist_when_searching_for_the_blacklist()
        {
            WithSeries("Blacklist");
            WithSeries("The Blacklist");

            _series.Sort(new SearchGameComparer("the blacklist"));

            _series.First().Title.Should().Be("The Blacklist");
        }
    }
}
