using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.CustomFormats;
using Romarr.Core.MediaFiles;
using Romarr.Core.MediaFiles.MediaInfo;
using Romarr.Core.Organizer;
using Romarr.Core.Qualities;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;
using Romarr.Test.Common;

namespace Romarr.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]
    public class FileNameBuilderFixture : CoreTest<FileNameBuilder>
    {
        private Game _series;
        private Rom _gameFile1;
        private RomFile _romFile;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>
                    .CreateNew()
                    .With(s => s.Title = "South Park")
                    .Build();

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameGameFiles = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            _gameFile1 = Builder<Rom>.CreateNew()
                            .With(e => e.Title = "City Sushi")
                            .With(e => e.PlatformNumber = 15)
                            .With(e => e.FileNumber = 6)
                            .With(e => e.AbsoluteFileNumber = 100)
                            .Build();

            _romFile = new RomFile { Quality = new QualityModel(Quality.HDTV720p), ReleaseGroup = "RomarrTest" };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));

            Mocker.GetMock<ICustomFormatService>()
                  .Setup(v => v.All())
                  .Returns(new List<CustomFormat>());
        }

        private void GivenProper()
        {
            _romFile.Quality.Revision.Version = 2;
        }

        private void GivenReal()
        {
            _romFile.Quality.Revision.Real = 1;
        }

        [Test]
        public void should_replace_Series_space_Title()
        {
            _namingConfig.StandardGameFileFormat = "{Game Title}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("South Park");
        }

        [Test]
        public void should_replace_Series_underscore_Title()
        {
            _namingConfig.StandardGameFileFormat = "{Game_Title}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("South_Park");
        }

        [Test]
        public void should_replace_Series_dot_Title()
        {
            _namingConfig.StandardGameFileFormat = "{Game.Title}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("South.Park");
        }

        [Test]
        public void should_replace_Series_dash_Title()
        {
            _namingConfig.StandardGameFileFormat = "{Game-Title}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("South-Park");
        }

        [Test]
        public void should_replace_SERIES_TITLE_with_all_caps()
        {
            _namingConfig.StandardGameFileFormat = "{GAME TITLE}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("SOUTH PARK");
        }

        [Test]
        public void should_replace_SERIES_TITLE_with_random_casing_should_keep_original_casing()
        {
            _namingConfig.StandardGameFileFormat = "{gAmE-tItLE}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be(_series.Title.Replace(' ', '-'));
        }

        [Test]
        public void should_replace_series_title_with_all_lower_case()
        {
            _namingConfig.StandardGameFileFormat = "{game title}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("south park");
        }

        [Test]
        public void should_cleanup_Series_Title()
        {
            _namingConfig.StandardGameFileFormat = "{Game.CleanTitle}";
            _series.Title = "South Park (1997)";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("South.Park.1997");
        }

        [Test]
        public void should_replace_gameFile_title()
        {
            _namingConfig.StandardGameFileFormat = "{Rom Title}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("City Sushi");
        }

        [Test]
        public void should_replace_gameFile_title_if_pattern_has_random_casing()
        {
            _namingConfig.StandardGameFileFormat = "{rOm-TitLe}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("City-Sushi");
        }

        [Test]
        public void should_replace_platform_number_with_single_digit()
        {
            _gameFile1.PlatformNumber = 1;
            _namingConfig.StandardGameFileFormat = "{platform}x{rom}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("1x6");
        }

        [Test]
        public void should_replace_platform00_number_with_two_digits()
        {
            _gameFile1.PlatformNumber = 1;
            _namingConfig.StandardGameFileFormat = "{platform:00}x{rom}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("01x6");
        }

        [Test]
        public void should_replace_gameFile_number_with_single_digit()
        {
            _gameFile1.PlatformNumber = 1;
            _namingConfig.StandardGameFileFormat = "{platform}x{rom}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("1x6");
        }

        [Test]
        public void should_replace_gameFile00_number_with_two_digits()
        {
            _gameFile1.PlatformNumber = 1;
            _namingConfig.StandardGameFileFormat = "{platform}x{rom:00}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("1x06");
        }

        [Test]
        public void should_replace_quality_title()
        {
            _namingConfig.StandardGameFileFormat = "{Quality Title}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("Unknown");
        }

        [Test]
        public void should_replace_quality_proper_with_proper()
        {
            _namingConfig.StandardGameFileFormat = "{Quality Proper}";
            GivenProper();

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("Proper");
        }

        [Test]
        public void should_replace_quality_real_with_real()
        {
            _namingConfig.StandardGameFileFormat = "{Quality Real}";
            GivenReal();

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("REAL");
        }

        [Test]
        public void should_replace_all_contents_in_pattern()
        {
            _namingConfig.StandardGameFileFormat = "{Game Title} - P{platform:00}R{rom:00} - {Rom Title} [{Quality Title}]";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("South Park - P15R06 - City Sushi [Unknown]");
        }

        [TestCase("Some Escaped {{ String", "Some Escaped { String")]
        [TestCase("Some Escaped }} String", "Some Escaped } String")]
        [TestCase("Some Escaped {{Game Title}} String", "Some Escaped {Game Title} String")]
        [TestCase("Some Escaped {{{Game Title}}} String", "Some Escaped {South Park} String")]
        public void should_escape_token_in_format(string format, string expected)
        {
            _namingConfig.StandardGameFileFormat = format;

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be(expected);
        }

        [Test]
        public void should_escape_token_in_title()
        {
            _namingConfig.StandardGameFileFormat = "Some Unescaped {Game Title} String";
            _series.Title = "My {Quality Full} Title";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("Some Unescaped My {Quality Full} Title String");
        }

        [Test]
        public void use_file_name_when_sceneName_is_null()
        {
            _namingConfig.RenameGameFiles = false;
            _romFile.RelativePath = "30 Rock - P01R01 - Test";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be(Path.GetFileNameWithoutExtension(_romFile.RelativePath));
        }

        [Test]
        public void use_path_when_sceneName_and_relative_path_are_null()
        {
            _namingConfig.RenameGameFiles = false;
            _romFile.RelativePath = null;
            _romFile.Path = @"C:\Test\Unsorted\Game - P01R01 - Test".AsOsAgnostic();

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be(Path.GetFileNameWithoutExtension(_romFile.Path));
        }

        [Test]
        public void use_file_name_when_sceneName_is_not_null()
        {
            _namingConfig.RenameGameFiles = false;
            _romFile.SceneName = "30.Rock.P01R01.xvid-LOL";
            _romFile.RelativePath = "30 Rock - P01R01 - Test";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("30.Rock.P01R01.xvid-LOL");
        }

        [Test]
        public void should_replace_illegal_characters_when_renaming_is_disabled()
        {
            _namingConfig.RenameGameFiles = false;
            _namingConfig.ReplaceIllegalCharacters = true;
            _namingConfig.ColonReplacementFormat = ColonReplacementFormat.Smart;

            _romFile.SceneName = "30.Rock.P01R01.xvid:LOL";
            _romFile.RelativePath = "30 Rock - P01R01 - Test";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                .Should().Be("30.Rock.P01R01.xvid-LOL");
        }

        [Test]
        public void should_use_standard_format_for_platform_zero()
        {
            _namingConfig.StandardGameFileFormat = "{Game Title} - P{platform:00}R{rom:00} - {Rom Title}";

            _series.Title = "Retro Game Collection";
            _series.GameType = GameTypes.Standard;

            _gameFile1.Title = "USA";
            _gameFile1.PlatformNumber = 0;
            _gameFile1.FileNumber = 5;

            _romFile.PlatformNumber = 0;

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("Retro Game Collection - P00R05 - USA");
        }

        [Test]
        public void should_not_clean_gameFile_title_if_there_is_only_one()
        {
            var title = "City Sushi (1)";
            _gameFile1.Title = title;

            _namingConfig.StandardGameFileFormat = "{Rom Title}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be(title);
        }

        [Test]
        public void should_should_replace_release_group()
        {
            _namingConfig.StandardGameFileFormat = "{Release Group}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be(_romFile.ReleaseGroup);
        }

        [Test]
        public void should_be_able_to_use_original_title()
        {
            _series.Title = "30 Rock";
            _namingConfig.StandardGameFileFormat = "{Game Title} - {Original Title}";

            _romFile.SceneName = "30.Rock.P01R01.xvid-LOL";
            _romFile.RelativePath = "30 Rock - P01R01 - Test";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("30 Rock - 30.Rock.P01R01.xvid-LOL");
        }

        [Test]
        public void should_trim_periods_from_end_of_gameFile_title()
        {
            _namingConfig.StandardGameFileFormat = "{Game Title} - P{platform:00}R{rom:00} - {Rom Title}";
            _namingConfig.MultiGameFileStyle = MultiGameFileStyle.Scene;

            var rom = Builder<Rom>.CreateNew()
                            .With(e => e.Title = "Part 1.")
                            .With(e => e.PlatformNumber = 6)
                            .With(e => e.FileNumber = 6)
                            .Build();

            Subject.BuildFileName(new List<Rom> { rom }, new Game { Title = "30 Rock" }, _romFile)
                   .Should().Be("30 Rock - P06R06 - Part 1");
        }

        [Test]
        public void should_trim_question_marks_from_end_of_gameFile_title()
        {
            _namingConfig.StandardGameFileFormat = "{Game Title} - P{platform:00}R{rom:00} - {Rom Title}";
            _namingConfig.MultiGameFileStyle = MultiGameFileStyle.Scene;

            var rom = Builder<Rom>.CreateNew()
                            .With(e => e.Title = "Part 1?")
                            .With(e => e.PlatformNumber = 6)
                            .With(e => e.FileNumber = 6)
                            .Build();

            Subject.BuildFileName(new List<Rom> { rom }, new Game { Title = "30 Rock" }, _romFile)
                   .Should().Be("30 Rock - P06R06 - Part 1");
        }

        [Test]
        public void should_replace_double_period_with_single_period()
        {
            _namingConfig.StandardGameFileFormat = "{Game.Title}.P{platform:00}R{rom:00}.{Rom.Title}";

            var rom = Builder<Rom>.CreateNew()
                            .With(e => e.Title = "Part 1")
                            .With(e => e.PlatformNumber = 6)
                            .With(e => e.FileNumber = 6)
                            .Build();

            Subject.BuildFileName(new List<Rom> { rom }, new Game { Title = "Chicago P.D." }, _romFile)
                   .Should().Be("Chicago.P.D.P06R06.Part.1");
        }

        [Test]
        public void should_replace_triple_period_with_single_period()
        {
            _namingConfig.StandardGameFileFormat = "{Game.Title}.P{platform:00}R{rom:00}.{Rom.Title}";

            var rom = Builder<Rom>.CreateNew()
                            .With(e => e.Title = "Part 1")
                            .With(e => e.PlatformNumber = 6)
                            .With(e => e.FileNumber = 6)
                            .Build();

            Subject.BuildFileName(new List<Rom> { rom }, new Game { Title = "Chicago P.D.." }, _romFile)
                   .Should().Be("Chicago.P.D.P06R06.Part.1");
        }

        [Test]
        public void should_not_replace_absolute_numbering_when_series_is_not_anime()
        {
            _namingConfig.StandardGameFileFormat = "{Game.Title}.P{platform:00}R{rom:00}.{absolute:00}.{Rom.Title}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("South.Park.P15R06.City.Sushi");
        }

        [Test]
        public void should_strip_absolute_numbering_tokens_for_games()
        {
            _series.GameType = GameTypes.Standard;
            _namingConfig.StandardGameFileFormat = "{Game.Title}.P{platform:00}R{rom:00}.{absolute:00}.{Rom.Title}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("South.Park.P15R06.City.Sushi");
        }

        [Test]
        public void should_replace_standard_numbering_when_series_is_anime()
        {
            _series.GameType = GameTypes.Standard;
            _namingConfig.StandardGameFileFormat = "{Game.Title}.P{platform:00}R{rom:00}.{Rom.Title}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("South.Park.P15R06.City.Sushi");
        }

        [Test]
        public void should_strip_absolute_numbering_when_used_in_standard_format()
        {
            _series.GameType = GameTypes.Standard;
            _namingConfig.StandardGameFileFormat = "{Game.Title}.{absolute:00}.{Rom.Title}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("South.Park.City.Sushi");
        }

        [Test]
        public void should_replace_duplicate_numbering_individually()
        {
            _series.GameType = GameTypes.Standard;
            _namingConfig.StandardGameFileFormat = "{Game.Title}.{platform}x{rom:00}.{absolute:000}\\{Game.Title}.P{platform:00}R{rom:00}.{absolute:00}.{Rom.Title}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("South.Park.15x06\\South.Park.P15R06.City.Sushi".AsOsAgnostic());
        }

        [Test]
        public void should_replace_individual_platform_gameFile_tokens()
        {
            _series.GameType = GameTypes.Standard;
            _namingConfig.StandardGameFileFormat = "{Game Title} Platform {platform:0000} Rom {rom:0000}\\{Game.Title}.P{platform:00}R{rom:00}.{absolute:00}.{Rom.Title}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("South Park Platform 0015 Rom 0006\\South.Park.P15R06.City.Sushi".AsOsAgnostic());
        }

        [Test]
        public void should_use_standard_naming_when_anime_gameFile_has_no_absolute_number()
        {
            _series.GameType = GameTypes.Standard;
            _gameFile1.AbsoluteFileNumber = null;

            _namingConfig.StandardGameFileFormat = "{Game Title} - {platform:0}x{rom:00} - {Rom Title}";

            Subject.BuildFileName(new List<Rom> { _gameFile1, }, _series, _romFile)
                   .Should().Be("South Park - 15x06 - City Sushi");
        }

        [Test]
        public void should_include_affixes_if_value_not_empty()
        {
            _namingConfig.StandardGameFileFormat = "{Game.Title}.P{platform:00}R{rom:00}{_Rom.Title_}{Quality.Title}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("South.Park.P15R06_City.Sushi_Unknown");
        }

        [Test]
        public void should_not_include_affixes_if_value_empty()
        {
            _namingConfig.StandardGameFileFormat = "{Game.Title}.P{platform:00}R{rom:00}{_Rom.Title_}";

            _gameFile1.Title = "";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("South.Park.P15R06");
        }

        [Test]
        public void should_format_mediainfo_properly()
        {
            _namingConfig.StandardGameFileFormat = "{Game.Title}.P{platform:00}R{rom:00}.{Rom.Title}.{MEDIAINFO.FULL}";

            _romFile.MediaInfo = new Core.MediaFiles.MediaInfo.MediaInfoModel()
            {
                VideoFormat = "h264",
                AudioStreams =
                [
                    new MediaInfoAudioStreamModel
                    {
                        Format = "dts",
                        Language = "eng",
                    },
                    new MediaInfoAudioStreamModel
                    {
                        Format = "dts",
                        Language = "spa",
                    },
                ],
                SubtitleStreams =
                [
                    new MediaInfoSubtitleStreamModel { Language = "eng" },
                    new MediaInfoSubtitleStreamModel { Language = "spa" },
                    new MediaInfoSubtitleStreamModel { Language = "ita" },
                ],
            };

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("South.Park.P15R06.City.Sushi.H264.DTS[EN+ES].[EN+ES+IT]");
        }

        [TestCase("nob", "NB")]
        [TestCase("swe", "SV")]
        [TestCase("zho", "ZH")]
        [TestCase("chi", "ZH")]
        [TestCase("fre", "FR")]
        [TestCase("rum", "RO")]
        [TestCase("per", "FA")]
        [TestCase("ger", "DE")]
        [TestCase("gsw", "DE")]
        [TestCase("cze", "CS")]
        [TestCase("ice", "IS")]
        [TestCase("dut", "NL")]
        [TestCase("nor", "NO")]
        [TestCase("geo", "KA")]
        [TestCase("kat", "KA")]
        public void should_format_languagecodes_properly(string language, string code)
        {
            _namingConfig.StandardGameFileFormat = "{Game.Title}.P{platform:00}R{rom:00}.{Rom.Title}.{MEDIAINFO.FULL}";

            _romFile.MediaInfo = new Core.MediaFiles.MediaInfo.MediaInfoModel()
            {
                VideoFormat = "h264",
                AudioStreams =
                [
                    new MediaInfoAudioStreamModel
                    {
                        Format = "dts",
                        Channels = 6,
                        Language = "eng",
                    },
                ],
                SubtitleStreams =
                [
                    new MediaInfoSubtitleStreamModel { Language = language },
                ],
                SchemaRevision = 3
            };

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be($"South.Park.P15R06.City.Sushi.H264.DTS.[{code}]");
        }

        [Test]
        public void should_exclude_english_in_mediainfo_audio_language()
        {
            _namingConfig.StandardGameFileFormat = "{Game.Title}.P{platform:00}R{rom:00}.{Rom.Title}.{MEDIAINFO.FULL}";

            _romFile.MediaInfo = new Core.MediaFiles.MediaInfo.MediaInfoModel()
            {
                VideoFormat = "h264",
                AudioStreams =
                [
                    new MediaInfoAudioStreamModel
                    {
                        Format = "dts",
                        Language = "eng",
                    },
                ],
                SubtitleStreams =
                [
                    new MediaInfoSubtitleStreamModel { Language = "eng" },
                    new MediaInfoSubtitleStreamModel { Language = "spa" },
                    new MediaInfoSubtitleStreamModel { Language = "ita" },
                ],
            };

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("South.Park.P15R06.City.Sushi.H264.DTS.[EN+ES+IT]");
        }

        [Ignore("not currently supported")]
        [Test]
        public void should_remove_duplicate_non_word_characters()
        {
            _series.Title = "Venture Bros.";
            _namingConfig.StandardGameFileFormat = "{Game.Title}.{platform}x{rom:00}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("Venture.Bros.15x06");
        }

        [Test]
        public void should_use_existing_filename_when_scene_name_is_not_available()
        {
            _namingConfig.RenameGameFiles = true;
            _namingConfig.StandardGameFileFormat = "{Original Title}";

            _romFile.SceneName = null;
            _romFile.RelativePath = "existing.file.mkv";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be(Path.GetFileNameWithoutExtension(_romFile.RelativePath));
        }

        [Test]
        public void should_be_able_to_use_only_original_title()
        {
            _series.Title = "30 Rock";
            _namingConfig.StandardGameFileFormat = "{Original Title}";

            _romFile.SceneName = "30.Rock.P01R01.xvid-LOL";
            _romFile.RelativePath = "30 Rock - P01R01 - Test";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("30.Rock.P01R01.xvid-LOL");
        }

        [Test]
        public void should_allow_period_between_platform_and_gameFile()
        {
            _namingConfig.StandardGameFileFormat = "{Game.Title}.S{platform:00}.E{rom:00}.{Rom.Title}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("South.Park.S15.E06.City.Sushi");
        }

        [Test]
        public void should_allow_space_between_platform_and_gameFile()
        {
            _namingConfig.StandardGameFileFormat = "{Game Title} - S{platform:00} E{rom:00} - {Rom Title}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("South Park - S15 E06 - City Sushi");
        }

        [Test]
        public void should_replace_quality_proper_with_proper_when_proper()
        {
            _namingConfig.StandardGameFileFormat = "{Quality Proper}";

            GivenProper();

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("Proper");
        }

        [Test]
        public void should_not_include_quality_proper_when_release_is_not_a_proper()
        {
            _namingConfig.StandardGameFileFormat = "{Quality Title} {Quality Proper}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("Unknown");
        }

        [Test]
        public void should_wrap_proper_in_square_brackets()
        {
            _namingConfig.StandardGameFileFormat = "{Game Title} - P{platform:00}R{rom:00} [{Quality Title}] {[Quality Proper]}";

            GivenProper();

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("South Park - P15R06 [Unknown] [Proper]");
        }

        [Test]
        public void should_not_wrap_proper_in_square_brackets_when_not_a_proper()
        {
            _namingConfig.StandardGameFileFormat = "{Game Title} - P{platform:00}R{rom:00} [{Quality Title}] {[Quality Proper]}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("South Park - P15R06 [Unknown]");
        }

        [Test]
        public void should_replace_quality_full_with_quality_title_only_when_not_a_proper()
        {
            _namingConfig.StandardGameFileFormat = "{Game Title} - P{platform:00}R{rom:00} [{Quality Full}]";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("South Park - P15R06 [Unknown]");
        }

        [Test]
        public void should_replace_quality_full_with_quality_title_and_proper_only_when_a_proper()
        {
            _namingConfig.StandardGameFileFormat = "{Game Title} - P{platform:00}R{rom:00} [{Quality Full}]";

            GivenProper();

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("South Park - P15R06 [Unknown Proper]");
        }

        [Test]
        public void should_replace_quality_full_with_quality_title_and_real_when_a_real()
        {
            _namingConfig.StandardGameFileFormat = "{Game Title} - P{platform:00}R{rom:00} [{Quality Full}]";
            GivenReal();

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("South Park - P15R06 [Unknown REAL]");
        }

        [TestCase(' ')]
        [TestCase('-')]
        [TestCase('.')]
        [TestCase('_')]
        public void should_trim_extra_separators_from_end_when_quality_proper_is_not_included(char separator)
        {
            _namingConfig.StandardGameFileFormat = string.Format("{{Quality{0}Title}}{0}{{Quality{0}Proper}}", separator);

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("Unknown");
        }

        [TestCase(' ')]
        [TestCase('-')]
        [TestCase('.')]
        [TestCase('_')]
        public void should_trim_extra_separators_from_middle_when_quality_proper_is_not_included(char separator)
        {
            _namingConfig.StandardGameFileFormat = string.Format("{{Quality{0}Title}}{0}{{Quality{0}Proper}}{0}{{Rom{0}Title}}", separator);

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be(string.Format("Unknown{0}City{0}Sushi", separator));
        }

        [Test]
        public void should_not_require_a_separator_between_tokens()
        {
            _series.GameType = GameTypes.Standard;
            _namingConfig.StandardGameFileFormat = "[{Release Group}]{Game.CleanTitle}.{absolute:000}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("[RomarrTest]South.Park");
        }

        [Test]
        public void should_be_able_to_use_original_filename_only()
        {
            _series.Title = "30 Rock";
            _namingConfig.StandardGameFileFormat = "{Original Filename}";

            _romFile.SceneName = "30.Rock.P01R01.xvid-LOL";
            _romFile.RelativePath = "30 Rock - P01R01 - Test";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("30 Rock - P01R01 - Test");
        }

        [Test]
        public void should_use_Romarr_as_release_group_when_not_available()
        {
            _romFile.ReleaseGroup = null;
            _namingConfig.StandardGameFileFormat = "{Release Group}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("Romarr");
        }

        [TestCase("{Rom Title}{-Release Group}", "City Sushi")]
        [TestCase("{Rom Title}{ Release Group}", "City Sushi")]
        [TestCase("{Rom Title}{ [Release Group]}", "City Sushi")]
        public void should_not_use_Romarr_as_release_group_if_pattern_has_separator(string pattern, string expectedFileName)
        {
            _romFile.ReleaseGroup = null;
            _namingConfig.StandardGameFileFormat = pattern;

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be(expectedFileName);
        }

        [TestCase("0SEC")]
        [TestCase("2HD")]
        [TestCase("IMMERSE")]
        public void should_use_existing_casing_for_release_group(string releaseGroup)
        {
            _romFile.ReleaseGroup = releaseGroup;
            _namingConfig.StandardGameFileFormat = "{Release Group}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be(releaseGroup);
        }

        [TestCase("en-US")]
        [TestCase("fr-FR")]
        [TestCase("az")]
        [TestCase("tr-TR")]
        public void should_replace_all_tokens_for_different_cultures(string culture)
        {
            var oldCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);

                _romFile.ReleaseGroup = null;

                GivenMediaInfoModel(audioLanguages: "eng/deu");

                _namingConfig.StandardGameFileFormat = "{MediaInfo AudioLanguages}";

                Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                       .Should().Be("[EN+DE]");
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = oldCulture;
            }
        }

        [TestCase("eng", "")]
        [TestCase("eng/deu", "[EN+DE]")]
        public void should_format_audio_languages(string audioLanguages, string expected)
        {
            _romFile.ReleaseGroup = null;

            GivenMediaInfoModel(audioLanguages: audioLanguages);

            _namingConfig.StandardGameFileFormat = "{MediaInfo AudioLanguages}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be(expected);
        }

        [TestCase("eng", "[EN]")]
        [TestCase("eng/deu", "[EN+DE]")]
        public void should_format_audio_languages_all(string audioLanguages, string expected)
        {
            _romFile.ReleaseGroup = null;

            GivenMediaInfoModel(audioLanguages: audioLanguages);

            _namingConfig.StandardGameFileFormat = "{MediaInfo AudioLanguagesAll}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be(expected);
        }

        [TestCase("eng/deu", "", "[EN+DE]")]
        [TestCase("eng/nld/deu", "", "[EN+NL+DE]")]
        [TestCase("eng/deu", ":DE", "[DE]")]
        [TestCase("eng/nld/deu", ":EN+NL", "[EN+NL]")]
        [TestCase("eng/nld/deu", ":NL+EN", "[NL+EN]")]
        [TestCase("eng/nld/deu", ":-NL", "[EN+DE]")]
        [TestCase("eng/nld/deu", ":DE+", "[DE+-]")]
        [TestCase("eng/nld/deu", ":DE+NO.", "[DE].")]
        [TestCase("eng/nld/deu", ":-EN-", "[NL+DE]-")]
        public void should_format_subtitle_languages_all(string subtitleLanguages, string format, string expected)
        {
            _romFile.ReleaseGroup = null;

            GivenMediaInfoModel(subtitles: subtitleLanguages);

            _namingConfig.StandardGameFileFormat = "{MediaInfo SubtitleLanguages" + format + "}End";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be(expected + "End");
        }

        [Test]
        public void should_include_custom_formats_token()
        {
            _namingConfig.StandardGameFileFormat =
                "{Game.Title}.P{platform:00}R{rom:00}.{Rom.Title}.{Custom Formats}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile, customFormats: new List<CustomFormat>())
                .Should().Be("South.Park.P15R06.City.Sushi");
        }

        [Test]
        public void should_replace_release_hash_with_stored_hash()
        {
            _namingConfig.StandardGameFileFormat = "{Release Hash}";

            _romFile.ReleaseHash = "ABCDEFGH";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be("ABCDEFGH");
        }

        [Test]
        public void should_replace_null_release_hash_with_empty_string()
        {
            _namingConfig.StandardGameFileFormat = "{Release Hash}";

            _romFile.ReleaseHash = null;

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                   .Should().Be(string.Empty);
        }

        [Test]
        public void should_maintain_ellipsis_in_naming_format()
        {
            _namingConfig.StandardGameFileFormat = "{Game.Title}.S{platform:00}.E{rom:00}...{Rom.CleanTitle}";

            Subject.BuildFileName(new List<Rom> { _gameFile1 }, _series, _romFile)
                .Should().Be("South.Park.S15.E06...City.Sushi");
        }

        private void GivenMediaInfoModel(string videoCodec = "h264",
                                         string audioCodec = "dts",
                                         int audioChannels = 6,
                                         int videoBitDepth = 8,
                                         HdrFormat hdrFormat = HdrFormat.None,
                                         string audioLanguages = "eng",
                                         string subtitles = "eng/spa/ita",
                                         int schemaRevision = 5)
        {
            _romFile.MediaInfo = new MediaInfoModel
            {
                VideoFormat = videoCodec,
                AudioStreams = audioLanguages.Split('/')
                    .Select(language => new MediaInfoAudioStreamModel
                    {
                        Format = audioCodec,
                        Channels = audioChannels,
                        Language = language,
                    }).ToList(),
                SubtitleStreams = subtitles.Split('/')
                    .Select(language => new MediaInfoSubtitleStreamModel
                    {
                        Language = language
                    }).ToList(),
                VideoBitDepth = videoBitDepth,
                VideoHdrFormat = hdrFormat,
                SchemaRevision = schemaRevision
            };
        }
    }
}
