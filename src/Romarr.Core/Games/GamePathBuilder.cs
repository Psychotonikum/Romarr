using System;
using System.IO;
using NLog;
using Romarr.Common.Extensions;
using Romarr.Core.Organizer;
using Romarr.Core.RootFolders;

namespace Romarr.Core.Games
{
    public interface IBuildSeriesPaths
    {
        string BuildPath(Game game, bool useExistingRelativeFolder);
    }

    public class SeriesPathBuilder : IBuildSeriesPaths
    {
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IRootFolderService _rootFolderService;
        private readonly IGameSystemService _gameSystemService;
        private readonly Logger _logger;

        public SeriesPathBuilder(IBuildFileNames fileNameBuilder, IRootFolderService rootFolderService, IGameSystemService gameSystemService, Logger logger)
        {
            _fileNameBuilder = fileNameBuilder;
            _rootFolderService = rootFolderService;
            _gameSystemService = gameSystemService;
            _logger = logger;
        }

        public string BuildPath(Game game, bool useExistingRelativeFolder)
        {
            if (game.RootFolderPath.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("Root folder was not provided", nameof(game));
            }

            if (useExistingRelativeFolder && game.Path.IsNotNullOrWhiteSpace())
            {
                var relativePath = GetExistingRelativePath(game);
                return Path.Combine(game.RootFolderPath, relativePath);
            }

            var basePath = game.RootFolderPath;

            // Add Batocera-style system folder if game has a system assigned
            if (game.GameSystemId.HasValue && game.GameSystemId.Value > 0)
            {
                var system = _gameSystemService.Get(game.GameSystemId.Value);

                if (system != null && system.FolderName.IsNotNullOrWhiteSpace())
                {
                    basePath = Path.Combine(basePath, system.FolderName);

                    // For classic (non-patchable) systems, the game path IS the system folder
                    // ROMs sit directly in the system folder, no per-game subfolder
                    if (system.SystemType == GameSystemType.Classic)
                    {
                        return basePath;
                    }
                }
            }

            // For patchable systems (or no system), create a per-game subfolder
            return Path.Combine(basePath, _fileNameBuilder.GetGameFolder(game));
        }

        private string GetExistingRelativePath(Game game)
        {
            var rootFolderPath = _rootFolderService.GetBestRootFolderPath(game.Path);

            if (rootFolderPath.IsParentPath(game.Path))
            {
                return rootFolderPath.GetRelativePath(game.Path);
            }

            var directoryName = game.Path.GetDirectoryName();

            _logger.Warn("Unable to get relative path for game path {0}, using game folder name {1}", game.Path, directoryName);

            return directoryName;
        }
    }
}
