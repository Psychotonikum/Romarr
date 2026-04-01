using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using Romarr.Common.Extensions;
using Romarr.Core.MediaFiles.GameFileImport.Aggregation.Aggregators;
using Romarr.Core.Parser;
using Romarr.Core.Parser.Model;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;
using Romarr.Test.Common;

namespace Romarr.Core.Test.MediaFiles.GameFileImport.Aggregation.Aggregators
{
    [TestFixture]
    public class AugmentGameFilesFixture : CoreTest<AggregateFiles>
    {
        private Game _series;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>.CreateNew().Build();

            var augmenters = new List<Mock<IAggregateLocalGameFile>>
                             {
                                 new Mock<IAggregateLocalGameFile>()
                             };

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetRoms(It.IsAny<ParsedRomInfo>(), _series, It.IsAny<bool>(), null))
                  .Returns(Builder<Rom>.CreateListOfSize(1).BuildList());

            Mocker.SetConstant(augmenters.Select(c => c.Object));
        }

        [Test]
        public void should_not_use_folder_for_full_platform()
        {
            var fileRomInfo = Parser.Parser.ParseTitle("Game.Title.S01E01");
            var folderRomInfo = Parser.Parser.ParseTitle("Game.Title.S01");
            var localRom = new LocalGameFile
                               {
                                   FileRomInfo = fileRomInfo,
                                   FolderRomInfo = folderRomInfo,
                                   Path = @"C:\Test\Unsorted TV\Game.Title.S01\Game.Title.S01E01.nsp".AsOsAgnostic(),
                                   Game = _series
                               };

            Subject.Aggregate(localRom, null);

            Mocker.GetMock<IParsingService>()
                  .Verify(v => v.GetRoms(fileRomInfo, _series, localRom.SceneSource, null), Times.Once());
        }

        [Test]
        public void should_not_use_folder_when_it_contains_more_than_one_valid_video_file()
        {
            var fileRomInfo = Parser.Parser.ParseTitle("Game.Title.S01E01");
            var folderRomInfo = Parser.Parser.ParseTitle("Game.Title.S01");
            var localRom = new LocalGameFile
            {
                FileRomInfo = fileRomInfo,
                FolderRomInfo = folderRomInfo,
                Path = @"C:\Test\Unsorted TV\Game.Title.S01\Game.Title.S01E01.nsp".AsOsAgnostic(),
                Game = _series,
                OtherVideoFiles = true
            };

            Subject.Aggregate(localRom, null);

            Mocker.GetMock<IParsingService>()
                  .Verify(v => v.GetRoms(fileRomInfo, _series, localRom.SceneSource, null), Times.Once());
        }

        [Test]
        public void should_not_use_folder_name_if_file_name_is_scene_name()
        {
            var fileRomInfo = Parser.Parser.ParseTitle("Game.Title.S01E01");
            var folderRomInfo = Parser.Parser.ParseTitle("Game.Title.S01E01");
            var localRom = new LocalGameFile
            {
                FileRomInfo = fileRomInfo,
                FolderRomInfo = folderRomInfo,
                Path = @"C:\Test\Unsorted TV\Game.Title.S01E01\Game.Title.S01E01.720p.HDTV-Romarr.nsp".AsOsAgnostic(),
                Game = _series
            };

            Subject.Aggregate(localRom, null);

            Mocker.GetMock<IParsingService>()
                  .Verify(v => v.GetRoms(fileRomInfo, _series, localRom.SceneSource, null), Times.Once());
        }

        [Test]
        public void should_use_folder_when_only_one_video_file()
        {
            var fileRomInfo = Parser.Parser.ParseTitle("Game.Title.S01E01");
            var folderRomInfo = Parser.Parser.ParseTitle("Game.Title.S01E01");
            var localRom = new LocalGameFile
            {
                FileRomInfo = fileRomInfo,
                FolderRomInfo = folderRomInfo,
                Path = @"C:\Test\Unsorted TV\Game.Title.S01E01\Game.Title.S01E01.nsp".AsOsAgnostic(),
                Game = _series
            };

            Subject.Aggregate(localRom, null);

            Mocker.GetMock<IParsingService>()
                  .Verify(v => v.GetRoms(folderRomInfo, _series, localRom.SceneSource, null), Times.Once());
        }

        [Test]
        public void should_use_file_when_folder_is_absolute_and_file_is_not()
        {
            var fileRomInfo = Parser.Parser.ParseTitle("Game.Title.S01E01");
            var folderRomInfo = Parser.Parser.ParseTitle("Game.Title.01");
            var localRom = new LocalGameFile
                               {
                                   FileRomInfo = fileRomInfo,
                                   FolderRomInfo = folderRomInfo,
                                   Path = @"C:\Test\Unsorted TV\Game.Title.101\Game.Title.S01E01.nsp".AsOsAgnostic(),
                                   Game = _series
                               };

            Subject.Aggregate(localRom, null);

            Mocker.GetMock<IParsingService>()
                  .Verify(v => v.GetRoms(fileRomInfo, _series, localRom.SceneSource, null), Times.Once());
        }

        [Test]
        public void should_use_special_info_when_not_null()
        {
            var fileRomInfo = Parser.Parser.ParseTitle("S00E01");
            var specialRomInfo = fileRomInfo.JsonClone();

            var localRom = new LocalGameFile
                               {
                                   FileRomInfo = fileRomInfo,
                                   Path = @"C:\Test\TV\Game\Specials\S00E01.mkv".AsOsAgnostic(),
                                   Game = _series
                               };

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetRoms(fileRomInfo, _series, It.IsAny<bool>(), null))
                  .Returns(new List<Rom>());

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.ParseSpecialRomTitle(fileRomInfo, It.IsAny<string>(), _series))
                  .Returns(specialRomInfo);

            Subject.Aggregate(localRom, null);

            Mocker.GetMock<IParsingService>()
                  .Verify(v => v.GetRoms(specialRomInfo, _series, localRom.SceneSource, null), Times.Once());
        }
    }
}
