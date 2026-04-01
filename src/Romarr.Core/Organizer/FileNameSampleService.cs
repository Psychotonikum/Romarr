using System.Collections.Generic;
using Romarr.Core.CustomFormats;
using Romarr.Core.MediaFiles;
using Romarr.Core.Qualities;
using Romarr.Core.Games;

namespace Romarr.Core.Organizer
{
    public interface IFilenameSampleService
    {
        SampleResult GetStandardSample(NamingConfig nameSpec);
        SampleResult GetMultiGameFileSample(NamingConfig nameSpec);
        string GetGameFolderSample(NamingConfig nameSpec);
        string GetPlatformFolderSample(NamingConfig nameSpec);
    }

    public class FileNameSampleService : IFilenameSampleService
    {
        private readonly IBuildFileNames _buildFileNames;
        private static Game _standardSeries;
        private static Rom _gameFile1;
        private static Rom _gameFile2;
        private static Rom _gameFile3;
        private static List<Rom> _singleGameFile;
        private static List<Rom> _multiGameFiles;
        private static RomFile _singleRomFile;
        private static RomFile _multiRomFile;
        private static List<CustomFormat> _customFormats;

        public FileNameSampleService(IBuildFileNames buildFileNames)
        {
            _buildFileNames = buildFileNames;

            _standardSeries = new Game
            {
                GameType = GameTypes.Standard,
                Title = "Super Mario Bros",
                Year = 1985,
                ImdbId = "tt12345",
                IgdbId = 12345,
                RawgId = 54321,
                TmdbId = 11223
            };

            _gameFile1 = new Rom
            {
                PlatformNumber = 3,
                FileNumber = 1,
                Title = "USA",
                AbsoluteFileNumber = 1,
            };

            _gameFile2 = new Rom
            {
                PlatformNumber = 3,
                FileNumber = 2,
                Title = "Japan",
                AbsoluteFileNumber = 2
            };

            _gameFile3 = new Rom
            {
                PlatformNumber = 3,
                FileNumber = 3,
                Title = "Europe",
                AbsoluteFileNumber = 3
            };

            _singleGameFile = new List<Rom> { _gameFile1 };
            _multiGameFiles = new List<Rom> { _gameFile1, _gameFile2, _gameFile3 };

            _customFormats = new List<CustomFormat>
            {
                new CustomFormat
                {
                    Name = "No-Intro Verified",
                    IncludeCustomFormatWhenRenaming = true
                },
                new CustomFormat
                {
                    Name = "Redump",
                    IncludeCustomFormatWhenRenaming = true
                }
            };

            _singleRomFile = new RomFile
            {
                Quality = new QualityModel(Quality.Verified, new Revision(1)),
                RelativePath = "Super Mario Bros (USA).nes",
                SceneName = "Super.Mario.Bros.S03E01.USA-NOINTRO",
                ReleaseGroup = "NOINTRO",
                MediaInfo = null
            };

            _multiRomFile = new RomFile
            {
                Quality = new QualityModel(Quality.Verified, new Revision(1)),
                RelativePath = "Super Mario Bros (USA+Japan+Europe).nes",
                SceneName = "Super.Mario.Bros.S03E01-E03.USA.Japan.Europe-NOINTRO",
                ReleaseGroup = "NOINTRO",
                MediaInfo = null,
            };
        }

        public SampleResult GetStandardSample(NamingConfig nameSpec)
        {
            var result = new SampleResult
            {
                FileName = BuildSample(_singleGameFile, _standardSeries, _singleRomFile, nameSpec, _customFormats),
                Game = _standardSeries,
                Roms = _singleGameFile,
                RomFile = _singleRomFile
            };

            return result;
        }

        public SampleResult GetMultiGameFileSample(NamingConfig nameSpec)
        {
            var result = new SampleResult
            {
                FileName = BuildSample(_multiGameFiles, _standardSeries, _multiRomFile, nameSpec, _customFormats),
                Game = _standardSeries,
                Roms = _multiGameFiles,
                RomFile = _multiRomFile
            };

            return result;
        }

        public string GetGameFolderSample(NamingConfig nameSpec)
        {
            return _buildFileNames.GetGameFolder(_standardSeries, nameSpec);
        }

        public string GetPlatformFolderSample(NamingConfig nameSpec)
        {
            return _buildFileNames.GetPlatformFolder(_standardSeries, _gameFile1.PlatformNumber, nameSpec);
        }

        private string BuildSample(List<Rom> roms, Game game, RomFile romFile, NamingConfig nameSpec, List<CustomFormat> customFormats)
        {
            try
            {
                return _buildFileNames.BuildFileName(roms, game, romFile, "", nameSpec, customFormats);
            }
            catch (NamingFormatException)
            {
                return string.Empty;
            }
        }
    }
}
