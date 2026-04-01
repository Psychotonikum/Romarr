using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using Romarr.Common.Disk;
using Romarr.Common.Extensions;
using Romarr.Common.Instrumentation.Extensions;
using Romarr.Core.MediaFiles.Commands;
using Romarr.Core.MediaFiles.Events;
using Romarr.Core.Messaging.Commands;
using Romarr.Core.Messaging.Events;
using Romarr.Core.Organizer;
using Romarr.Core.Games;

namespace Romarr.Core.MediaFiles
{
    public interface IRenameRomFileService
    {
        List<RenameRomFilePreview> GetRenamePreviews(int gameId);
        List<RenameRomFilePreview> GetRenamePreviews(int gameId, int platformNumber);
        List<RenameRomFilePreview> GetRenamePreviews(List<int> gameIds);
    }

    public class RenameRomFileService : IRenameRomFileService,
                                            IExecute<RenameFilesCommand>,
                                            IExecute<RenameGameCommand>
    {
        private readonly IGameService _gameService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IMoveRomFiles _romFileMover;
        private readonly IEventAggregator _eventAggregator;
        private readonly IRomService _romService;
        private readonly IBuildFileNames _filenameBuilder;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public RenameRomFileService(IGameService seriesService,
                                        IMediaFileService mediaFileService,
                                        IMoveRomFiles romFileMover,
                                        IEventAggregator eventAggregator,
                                        IRomService gameFileService,
                                        IBuildFileNames filenameBuilder,
                                        IDiskProvider diskProvider,
                                        Logger logger)
        {
            _gameService = seriesService;
            _mediaFileService = mediaFileService;
            _romFileMover = romFileMover;
            _eventAggregator = eventAggregator;
            _romService = gameFileService;
            _filenameBuilder = filenameBuilder;
            _diskProvider = diskProvider;
            _logger = logger;
        }

        public List<RenameRomFilePreview> GetRenamePreviews(int gameId)
        {
            var game = _gameService.GetGame(gameId);
            var roms = _romService.GetGameFileBySeries(gameId);
            var files = _mediaFileService.GetFilesBySeries(gameId);

            return GetPreviews(game, roms, files)
                .OrderByDescending(e => e.PlatformNumber)
                .ThenByDescending(e => e.RomNumbers.First())
                .ToList();
        }

        public List<RenameRomFilePreview> GetRenamePreviews(int gameId, int platformNumber)
        {
            var game = _gameService.GetGame(gameId);
            var roms = _romService.GetRomsByPlatform(gameId, platformNumber);
            var files = _mediaFileService.GetFilesByPlatform(gameId, platformNumber);

            return GetPreviews(game, roms, files)
                .OrderByDescending(e => e.RomNumbers.First()).ToList();
        }

        public List<RenameRomFilePreview> GetRenamePreviews(List<int> gameIds)
        {
            var gameList = _gameService.GetGame(gameIds);
            var gameFilesList = _romService.GetRomsByGame(gameIds).ToLookup(e => e.GameId);
            var filesList = _mediaFileService.GetFilesByGameIds(gameIds).ToLookup(f => f.GameId);

            return gameList.SelectMany(game =>
                {
                    var roms = gameFilesList[game.Id].ToList();
                    var files = filesList[game.Id].ToList();

                    return GetPreviews(game, roms, files);
                })
                .OrderByDescending(e => e.GameId)
                .ThenByDescending(e => e.PlatformNumber)
                .ThenByDescending(e => e.RomNumbers.First())
                .ToList();
        }

        private IEnumerable<RenameRomFilePreview> GetPreviews(Game game, List<Rom> roms, List<RomFile> files)
        {
            foreach (var f in files)
            {
                var file = f;
                var gameFilesInFile = roms.Where(e => e.RomFileId == file.Id).ToList();
                var romFilePath = Path.Combine(game.Path, file.RelativePath);

                if (!gameFilesInFile.Any())
                {
                    _logger.Warn("File ({0}) is not linked to any roms", romFilePath);
                    continue;
                }

                var platformNumber = gameFilesInFile.First().PlatformNumber;
                var newPath = _filenameBuilder.BuildFilePath(gameFilesInFile, game, file, Path.GetExtension(romFilePath));

                if (!romFilePath.PathEquals(newPath, StringComparison.Ordinal))
                {
                    yield return new RenameRomFilePreview
                    {
                        GameId = game.Id,
                        PlatformNumber = platformNumber,
                        RomNumbers = gameFilesInFile.Select(e => e.FileNumber).ToList(),
                        RomFileId = file.Id,
                        ExistingPath = file.RelativePath,
                        NewPath = game.Path.GetRelativePath(newPath)
                    };
                }
            }
        }

        private List<RenamedRomFile> RenameFiles(List<RomFile> romFiles, Game game)
        {
            var renamed = new List<RenamedRomFile>();

            foreach (var romFile in romFiles)
            {
                var previousRelativePath = romFile.RelativePath;
                var previousPath = Path.Combine(game.Path, romFile.RelativePath);

                try
                {
                    _logger.Debug("Renaming rom file: {0}", romFile);
                    _romFileMover.MoveRomFile(romFile, game);

                    _mediaFileService.Update(romFile);

                    renamed.Add(new RenamedRomFile
                                {
                                    RomFile = romFile,
                                    PreviousRelativePath = previousRelativePath,
                                    PreviousPath = previousPath
                                });

                    _logger.Debug("Renamed rom file: {0}", romFile);

                    _eventAggregator.PublishEvent(new RomFileRenamedEvent(game, romFile, previousPath));
                }
                catch (FileAlreadyExistsException ex)
                {
                    _logger.Warn("File not renamed, there is already a file at the destination: {0}", ex.Filename);
                }
                catch (SameFilenameException ex)
                {
                    _logger.Debug("File not renamed, source and destination are the same: {0}", ex.Filename);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to rename file {0}", previousPath);
                }
            }

            if (renamed.Any())
            {
                _diskProvider.RemoveEmptySubfolders(game.Path);

                _eventAggregator.PublishEvent(new SeriesRenamedEvent(game, renamed));
            }

            return renamed;
        }

        public void Execute(RenameFilesCommand message)
        {
            var game = _gameService.GetGame(message.GameId);
            var romFiles = _mediaFileService.Get(message.Files);

            _logger.ProgressInfo("Renaming {0} files for {1}", romFiles.Count, game.Title);
            var renamedFiles = RenameFiles(romFiles, game);
            _logger.ProgressInfo("{0} selected rom files renamed for {1}", renamedFiles.Count, game.Title);

            _eventAggregator.PublishEvent(new RenameCompletedEvent());
        }

        public void Execute(RenameGameCommand message)
        {
            _logger.Debug("Renaming all files for selected game");
            var gamesToRename = _gameService.GetGame(message.GameIds);

            foreach (var game in gamesToRename)
            {
                var romFiles = _mediaFileService.GetFilesBySeries(game.Id);
                _logger.ProgressInfo("Renaming all files in game: {0}", game.Title);
                var renamedFiles = RenameFiles(romFiles, game);
                _logger.ProgressInfo("{0} rom files renamed for {1}", renamedFiles.Count, game.Title);
            }

            _eventAggregator.PublishEvent(new RenameCompletedEvent());
        }
    }
}
