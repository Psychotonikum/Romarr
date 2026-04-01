using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Romarr.Core.MediaFiles;
using Romarr.Core.MediaFiles.GameFileImport;
using Romarr.Core.Parser.Model;
using Romarr.Core.Profiles.Qualities;
using Romarr.Core.Qualities;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;
using Romarr.Test.Common;

namespace Romarr.Core.Test.MediaFiles.GameFileImport
{
    [TestFixture]
    public class GetSceneNameFixture : CoreTest
    {
        private LocalGameFile _localRom;
        private string _platformName = "game.title.s02.dvdrip.x264-ingot";
        private string _gameFileName = "game.title.s02e23.dvdrip.x264-ingot";

        [SetUp]
        public void Setup()
        {
            var game = Builder<Game>.CreateNew()
                                        .With(e => e.QualityProfile = new QualityProfile { Items = Qualities.QualityFixture.GetDefaultQualities() })
                                        .With(s => s.Path = @"C:\Test\TV\Game Title".AsOsAgnostic())
                                        .Build();

            var rom = Builder<Rom>.CreateNew()
                                          .Build();

            _localRom = new LocalGameFile
                            {
                                Game = game,
                                Roms = new List<Rom> { rom },
                                Path = Path.Combine(game.Path, "Game Title - S02E23 - Rom Title.mkv"),
                                Quality = new QualityModel(Quality.Bluray720p),
                                ReleaseGroup = "DRONE"
                            };
        }

        private void GivenExistingFileOnDisk()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.GetFilesWithRelativePath(It.IsAny<int>(), It.IsAny<string>()))
                  .Returns(new List<RomFile>());
        }

        [Test]
        public void should_use_download_client_item_title_as_scene_name()
        {
            _localRom.DownloadClientRomInfo = new ParsedRomInfo
                                                      {
                                                          ReleaseTitle = _gameFileName
                                                      };

            SceneNameCalculator.GetSceneName(_localRom).Should()
                               .Be(_gameFileName);
        }

        [Test]
        public void should_not_use_download_client_item_title_as_scene_name_if_full_platform()
        {
            _localRom.DownloadClientRomInfo = new ParsedRomInfo
                                                      {
                                                          ReleaseTitle = _platformName,
                                                          FullPlatform = true
                                                      };

            _localRom.Path = Path.Combine(@"C:\Test\Unsorted TV", _platformName, _gameFileName)
                                     .AsOsAgnostic();

            SceneNameCalculator.GetSceneName(_localRom).Should()
                               .BeNull();
        }

        [Test]
        public void should_not_use_download_client_item_title_as_scene_name_if_there_are_other_video_files()
        {
            _localRom.OtherVideoFiles = true;
            _localRom.DownloadClientRomInfo = new ParsedRomInfo
                                                      {
                                                          ReleaseTitle = _platformName,
                                                          FullPlatform = false
                                                      };

            _localRom.Path = Path.Combine(@"C:\Test\Unsorted TV", _platformName, _gameFileName)
                                     .AsOsAgnostic();

            SceneNameCalculator.GetSceneName(_localRom).Should()
                               .BeNull();
        }

        [Test]
        public void should_use_file_name_as_scenename_only_if_it_looks_like_scenename()
        {
            _localRom.Path = Path.Combine(@"C:\Test\Unsorted TV", _platformName, _gameFileName + ".mkv")
                                     .AsOsAgnostic();

            SceneNameCalculator.GetSceneName(_localRom).Should()
                               .Be(_gameFileName);
        }

        [Test]
        public void should_not_use_file_name_as_scenename_if_it_doesnt_look_like_scenename()
        {
            _localRom.Path = Path.Combine(@"C:\Test\Unsorted TV", _gameFileName, "aaaaa.mkv")
                                     .AsOsAgnostic();

            SceneNameCalculator.GetSceneName(_localRom).Should()
                               .BeNull();
        }

        [Test]
        public void should_use_folder_name_as_scenename_only_if_it_looks_like_scenename()
        {
            _localRom.FolderRomInfo = new ParsedRomInfo
                                              {
                                                  ReleaseTitle = _gameFileName
                                              };

            SceneNameCalculator.GetSceneName(_localRom).Should()
                               .Be(_gameFileName);
        }

        [Test]
        public void should_not_use_folder_name_as_scenename_if_it_doesnt_look_like_scenename()
        {
            _localRom.Path = Path.Combine(@"C:\Test\Unsorted TV", _gameFileName, "aaaaa.mkv")
                                     .AsOsAgnostic();

            _localRom.FolderRomInfo = new ParsedRomInfo
                                              {
                                                  ReleaseTitle = "aaaaa"
                                              };

            SceneNameCalculator.GetSceneName(_localRom).Should()
                               .BeNull();
        }

        [Test]
        public void should_not_use_folder_name_as_scenename_if_it_is_for_a_full_platform()
        {
            _localRom.Path = Path.Combine(@"C:\Test\Unsorted TV", _gameFileName, "aaaaa.mkv")
                                     .AsOsAgnostic();

            _localRom.FolderRomInfo = new ParsedRomInfo
                                              {
                                                  ReleaseTitle = _platformName,
                                                  FullPlatform = true
                                              };

            SceneNameCalculator.GetSceneName(_localRom).Should()
                               .BeNull();
        }

        [Test]
        public void should_not_use_folder_name_as_scenename_if_it_is_for_batch()
        {
            var batchName = "[HorribleSubs] Game Title (01-62) [1080p] (Batch)";

            _localRom.DownloadClientRomInfo = new ParsedRomInfo
                                                      {
                                                          FullPlatform = false,
                                                          ReleaseTitle = batchName
                                                      };

            _localRom.Path = Path.Combine(@"C:\Test\Unsorted TV", batchName, "[HorribleSubs] Game Title - 14 [1080p].mkv")
                                     .AsOsAgnostic();

            _localRom.OtherVideoFiles = true;

            _localRom.FolderRomInfo = new ParsedRomInfo
                                              {
                                                  ReleaseTitle = _platformName,
                                                  FullPlatform = false
                                              };

            SceneNameCalculator.GetSceneName(_localRom).Should()
                               .BeNull();
        }

        [Test]
        public void should_not_use_folder_name_as_scenename_if_there_are_other_video_files()
        {
            _localRom.OtherVideoFiles = true;
            _localRom.Path = Path.Combine(@"C:\Test\Unsorted TV", _gameFileName, "aaaaa.mkv")
                                     .AsOsAgnostic();

            _localRom.FolderRomInfo = new ParsedRomInfo
                                              {
                                                  ReleaseTitle = _platformName,
                                                  FullPlatform = false
                                              };

            SceneNameCalculator.GetSceneName(_localRom).Should()
                               .BeNull();
        }

        [TestCase(".nsp")]
        [TestCase(".par2")]
        [TestCase(".nzb")]
        public void should_remove_extension_from_nzb_title_for_scene_name(string extension)
        {
            _localRom.DownloadClientRomInfo = new ParsedRomInfo
                                                      {
                                                          ReleaseTitle = _gameFileName + extension
                                                      };

            SceneNameCalculator.GetSceneName(_localRom).Should()
                               .Be(_gameFileName);
        }
    }
}
