using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Romarr.Core.MediaFiles.GameFileImport.Specifications;
using Romarr.Core.Parser;
using Romarr.Core.Parser.Model;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;
using Romarr.Test.Common;

namespace Romarr.Core.Test.MediaFiles.GameFileImport.Specifications
{
    [TestFixture]
    public class MatchesFolderSpecificationFixture : CoreTest<MatchesFolderSpecification>
    {
        private LocalGameFile _localRom;

        [SetUp]
        public void Setup()
        {
            _localRom = Builder<LocalGameFile>.CreateNew()
                                                 .With(l => l.Path = @"C:\Test\Unsorted\Game.Title.S01E01.720p.HDTV-Romarr\S01E05.mkv".AsOsAgnostic())
                                                 .With(l => l.FileRomInfo =
                                                     Builder<ParsedRomInfo>.CreateNew()
                                                                               .With(p => p.RomNumbers = new[] { 5 })
                                                                               .With(p => p.PlatformNumber == 1)
                                                                               .With(p => p.FullPlatform = false)
                                                                               .Build())
                                                 .With(l => l.FolderRomInfo =
                                                     Builder<ParsedRomInfo>.CreateNew()
                                                                               .With(p => p.RomNumbers = new[] { 1 })
                                                                               .With(p => p.PlatformNumber == 1)
                                                                               .With(p => p.FullPlatform = false)
                                                                               .Build())
                                                 .Build();
        }

        private void GivenGameFiles(ParsedRomInfo parsedRomInfo, int[] romNumbers)
        {
            var platformNumber = parsedRomInfo.PlatformNumber;

            var roms = romNumbers.Select(n =>
                Builder<Rom>.CreateNew()
                                .With(e => e.Id = (platformNumber * 10) + n)
                                .With(e => e.PlatformNumber = platformNumber)
                                .With(e => e.FileNumber = n)
                                .Build())
            .ToList();

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetRoms(parsedRomInfo, It.IsAny<Game>(), true, null))
                  .Returns(roms);
        }

        [Test]
        public void should_be_accepted_for_existing_file()
        {
            _localRom.ExistingFile = true;

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_folder_name_is_not_parseable()
        {
            _localRom.Path = @"C:\Test\Unsorted\Game.Title\S01E01.mkv".AsOsAgnostic();
            _localRom.FolderRomInfo = null;

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_file_name_is_not_parseable()
        {
            _localRom.Path = @"C:\Test\Unsorted\Game.Title.S01E01\AFDAFD.mkv".AsOsAgnostic();
            _localRom.FileRomInfo = null;

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_should_be_accepted_for_full_platform()
        {
            _localRom.Path = @"C:\Test\Unsorted\Game.Title.S01\S01E01.mkv".AsOsAgnostic();
            _localRom.FolderRomInfo.RomNumbers = Array.Empty<int>();
            _localRom.FolderRomInfo.FullPlatform = true;

            GivenGameFiles(_localRom.FileRomInfo, _localRom.FileRomInfo.RomNumbers);
            GivenGameFiles(_localRom.FolderRomInfo, new[] { 1, 2, 3, 4, 5 });

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_file_and_folder_have_the_same_gameFile()
        {
            _localRom.FileRomInfo.RomNumbers = new[] { 1 };
            _localRom.FolderRomInfo.RomNumbers = new[] { 1 };
            _localRom.Path = @"C:\Test\Unsorted\Game.Title.S01E01.720p.HDTV-Romarr\S01E01.mkv".AsOsAgnostic();

            GivenGameFiles(_localRom.FileRomInfo, _localRom.FileRomInfo.RomNumbers);
            GivenGameFiles(_localRom.FolderRomInfo, _localRom.FolderRomInfo.RomNumbers);

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_file_is_one_gameFile_in_folder()
        {
            _localRom.FileRomInfo.RomNumbers = new[] { 1 };
            _localRom.FolderRomInfo.RomNumbers = new[] { 1 };
            _localRom.Path = @"C:\Test\Unsorted\Game.Title.S01E01E02.720p.HDTV-Romarr\S01E01.mkv".AsOsAgnostic();

            GivenGameFiles(_localRom.FileRomInfo, _localRom.FileRomInfo.RomNumbers);
            GivenGameFiles(_localRom.FolderRomInfo, _localRom.FolderRomInfo.RomNumbers);

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_disregard_subfolder()
        {
            _localRom.FileRomInfo.RomNumbers = new[] { 5, 6 };
            _localRom.FolderRomInfo.RomNumbers = new[] { 1, 2 };
            _localRom.Path = @"C:\Test\Unsorted\Game.Title.S01E01E02.720p.HDTV-Romarr\S01E05E06.mkv".AsOsAgnostic();

            GivenGameFiles(_localRom.FileRomInfo, _localRom.FileRomInfo.RomNumbers);
            GivenGameFiles(_localRom.FolderRomInfo, _localRom.FolderRomInfo.RomNumbers);

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_rejected_if_file_and_folder_do_not_have_same_gameFile()
        {
            _localRom.Path = @"C:\Test\Unsorted\Game.Title.S01E01.720p.HDTV-Romarr\S01E05.mkv".AsOsAgnostic();

            GivenGameFiles(_localRom.FileRomInfo, _localRom.FileRomInfo.RomNumbers);
            GivenGameFiles(_localRom.FolderRomInfo, _localRom.FolderRomInfo.RomNumbers);

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_rejected_if_file_and_folder_do_not_have_the_same_gameFiles()
        {
            _localRom.FileRomInfo.RomNumbers = new[] { 5, 6 };
            _localRom.FolderRomInfo.RomNumbers = new[] { 1, 2 };
            _localRom.Path = @"C:\Test\Unsorted\Game.Title.S01E01E02.720p.HDTV-Romarr\S01E05E06.mkv".AsOsAgnostic();

            GivenGameFiles(_localRom.FileRomInfo, _localRom.FileRomInfo.RomNumbers);
            GivenGameFiles(_localRom.FolderRomInfo, _localRom.FolderRomInfo.RomNumbers);

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_rejected_if_file_and_folder_do_not_have_gameFiles_from_the_same_platform()
        {
            _localRom.FileRomInfo.PlatformNumber = 2;
            _localRom.FileRomInfo.RomNumbers = new[] { 1 };

            _localRom.FolderRomInfo.FullPlatform = true;
            _localRom.FolderRomInfo.PlatformNumber = 1;
            _localRom.FolderRomInfo.RomNumbers = new[] { 1, 2 };

            GivenGameFiles(_localRom.FileRomInfo, _localRom.FileRomInfo.RomNumbers);
            GivenGameFiles(_localRom.FolderRomInfo, _localRom.FolderRomInfo.RomNumbers);

            _localRom.Path = @"C:\Test\Unsorted\Game.Title.S01.720p.HDTV-Romarr\S02E01.mkv".AsOsAgnostic();

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_rejected_if_file_and_folder_do_not_have_gameFiles_from_the_same_partial_platform()
        {
            _localRom.FileRomInfo.PlatformNumber = 2;
            _localRom.FileRomInfo.RomNumbers = new[] { 1 };

            _localRom.FolderRomInfo.PlatformNumber = 1;
            _localRom.FolderRomInfo.RomNumbers = new[] { 1, 2 };

            GivenGameFiles(_localRom.FileRomInfo, _localRom.FileRomInfo.RomNumbers);
            GivenGameFiles(_localRom.FolderRomInfo, _localRom.FolderRomInfo.RomNumbers);

            _localRom.Path = @"C:\Test\Unsorted\Game.Title.S01.720p.HDTV-Romarr\S02E01.mkv".AsOsAgnostic();

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_accepted_if_file_and_folder_have_gameFiles_from_the_same_platform()
        {
            _localRom.FileRomInfo.PlatformNumber = 1;
            _localRom.FileRomInfo.RomNumbers = new[] { 1 };

            _localRom.FolderRomInfo.FullPlatform = true;
            _localRom.FolderRomInfo.PlatformNumber = 1;
            _localRom.FolderRomInfo.RomNumbers = new[] { 1, 2 };

            GivenGameFiles(_localRom.FileRomInfo, _localRom.FileRomInfo.RomNumbers);
            GivenGameFiles(_localRom.FolderRomInfo, _localRom.FolderRomInfo.RomNumbers);

            _localRom.Path = @"C:\Test\Unsorted\Game.Title.S01.720p.HDTV-Romarr\S01E01.mkv".AsOsAgnostic();

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_both_file_and_folder_info_map_to_same_special()
        {
            var title = "Some.Special.S12E00.WEB-DL.1080p-GoodNightTV";
            var actualInfo = Parser.Parser.ParseTitle("Some.Special.S0E100.WEB-DL.1080p-GoodNightTV.mkv");

            var folderInfo = Parser.Parser.ParseTitle(title);
            var fileInfo = Parser.Parser.ParseTitle(title + ".mkv");
            var localRom = new LocalGameFile
            {
                FileRomInfo = fileInfo,
                FolderRomInfo = folderInfo,
                Game = new Games.Game
                {
                    Id = 1,
                    Title = "Some Special"
                }
            };

            GivenGameFiles(actualInfo, actualInfo.RomNumbers);

            Mocker.GetMock<IParsingService>()
                .Setup(v => v.ParseSpecialRomTitle(fileInfo, It.IsAny<string>(), 0, 0, null, null))
                .Returns(actualInfo);

            Mocker.GetMock<IParsingService>()
                .Setup(v => v.ParseSpecialRomTitle(folderInfo, It.IsAny<string>(), 0, 0, null, null))
                .Returns(actualInfo);

            Subject.IsSatisfiedBy(localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_file_has_absolute_gameFile_number_and_folder_uses_standard()
        {
            _localRom.FileRomInfo.PlatformNumber = 1;
            _localRom.FileRomInfo.AbsoluteRomNumbers = new[] { 1 };

            _localRom.FolderRomInfo.PlatformNumber = 1;
            _localRom.FolderRomInfo.RomNumbers = new[] { 1, 2 };

            GivenGameFiles(_localRom.FileRomInfo, new[] { 1 });
            GivenGameFiles(_localRom.FolderRomInfo, _localRom.FolderRomInfo.RomNumbers);

            _localRom.Path = @"C:\Test\Unsorted\Game.Title.S01.720p.HDTV-Romarr\S02E01.mkv".AsOsAgnostic();

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_scene_platform_number_matches_but_platform_number_does_not()
        {
            _localRom.Path = @"C:\Test\Unsorted\Game.Title.S01\S01E01.mkv".AsOsAgnostic();
            _localRom.FolderRomInfo.RomNumbers = Array.Empty<int>();
            _localRom.FolderRomInfo.FullPlatform = true;

            var rom = Builder<Rom>.CreateNew()
                .With(e => e.Id = (1 * 10) + 5)
                .With(e => e.PlatformNumber = 5)
                .With(e => e.ScenePlatformNumber = 1)
                .With(e => e.FileNumber = 5)
                .Build();

            Mocker.GetMock<IParsingService>()
                .Setup(s => s.GetRoms(_localRom.FileRomInfo, It.IsAny<Game>(), true, null))
                .Returns(new List<Rom> { rom });

            GivenGameFiles(_localRom.FolderRomInfo, new[] { 1, 2, 3, 4, 5 });

            Subject.IsSatisfiedBy(_localRom, null).Accepted.Should().BeTrue();
        }
    }
}
