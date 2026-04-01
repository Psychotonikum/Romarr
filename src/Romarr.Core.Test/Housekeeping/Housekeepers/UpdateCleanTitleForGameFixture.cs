using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using Romarr.Core.Housekeeping.Housekeepers;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;

namespace Romarr.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class UpdateCleanTitleForSeriesFixture : CoreTest<UpdateCleanTitleForSeries>
    {
        [Test]
        public void should_update_clean_title()
        {
            var game = Builder<Game>.CreateNew()
                                        .With(s => s.Title = "Full Title")
                                        .With(s => s.CleanTitle = "unclean")
                                        .Build();

            Mocker.GetMock<IGameRepository>()
                 .Setup(s => s.All())
                 .Returns(new[] { game });

            Subject.Clean();

            Mocker.GetMock<IGameRepository>()
                .Verify(v => v.Update(It.Is<Game>(s => s.CleanTitle == "fulltitle")), Times.Once());
        }

        [Test]
        public void should_not_update_unchanged_title()
        {
            var game = Builder<Game>.CreateNew()
                                        .With(s => s.Title = "Full Title")
                                        .With(s => s.CleanTitle = "fulltitle")
                                        .Build();

            Mocker.GetMock<IGameRepository>()
                 .Setup(s => s.All())
                 .Returns(new[] { game });

            Subject.Clean();

            Mocker.GetMock<IGameRepository>()
                .Verify(v => v.Update(It.Is<Game>(s => s.CleanTitle == "fulltitle")), Times.Never());
        }
    }
}
