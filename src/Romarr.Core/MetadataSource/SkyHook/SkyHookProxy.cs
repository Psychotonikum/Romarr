using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using IGDB;
using NLog;
using Romarr.Common.Disk;
using Romarr.Common.Extensions;
using Romarr.Common.Http;
using Romarr.Core.Configuration;
using Romarr.Core.Exceptions;
using Romarr.Core.Games;
using Romarr.Core.Languages;
using Romarr.Core.MediaCover;
using Romarr.Core.MetadataSource.Metacritic;
using Romarr.Core.MetadataSource.Tinfoil;
using Romarr.Core.MetadataSource.WiiU;

namespace Romarr.Core.MetadataSource.SkyHook
{
    public class SkyHookProxy : IProvideGameInfo, ISearchForNewGame
    {
        private const string GameFields = "fields id,name,summary,first_release_date,cover.image_id,platforms.name,platforms.abbreviation,genres.name,rating,rating_count,slug,game_status,screenshots.image_id,artworks.image_id,involved_companies.developer,involved_companies.publisher,involved_companies.company.name,dlcs,expansions,release_dates.date,release_dates.platform.name,release_dates.platform.abbreviation;";

        private readonly IIgdbClient _igdbClient;
        private readonly Logger _logger;
        private readonly IGameService _gameService;
        private readonly IConfigService _configService;
        private readonly IMetacriticProxy _metacriticProxy;
        private readonly ITinfoilProxy _tinfoilProxy;
        private readonly IWiiUTitleProxy _wiiuProxy;

        public SkyHookProxy(IIgdbClient igdbClient,
                            IGameService seriesService,
                            IConfigService configService,
                            IMetacriticProxy metacriticProxy,
                            ITinfoilProxy tinfoilProxy,
                            IWiiUTitleProxy wiiuProxy,
                            Logger logger)
        {
            _igdbClient = igdbClient;
            _logger = logger;
            _gameService = seriesService;
            _configService = configService;
            _metacriticProxy = metacriticProxy;
            _tinfoilProxy = tinfoilProxy;
            _wiiuProxy = wiiuProxy;
        }

        public Tuple<Game, List<Rom>> GetGameInfo(int igdbGameId)
        {
            var query = $"{GameFields} where id = {igdbGameId}; limit 1;";
            var games = _igdbClient.SearchGames(query);

            if (games == null || games.Length == 0)
            {
                throw new GameNotFoundException(igdbGameId);
            }

            var game = MapIgdbGame(games[0]);
            var roms = new List<Rom>();

            // Generate a base game ROM entry for each platform so the Game Details page shows content
            if (game.Platforms != null)
            {
                // Build a lookup of per-platform release dates from IGDB release_dates
                var platformReleaseDates = new Dictionary<string, DateTimeOffset>(StringComparer.OrdinalIgnoreCase);
                if (games[0].ReleaseDates?.Values != null)
                {
                    foreach (var rd in games[0].ReleaseDates.Values)
                    {
                        if (rd?.Date != null && rd.Platform?.Value != null)
                        {
                            var platName = rd.Platform.Value.Name ?? rd.Platform.Value.Abbreviation ?? "";
                            if (!string.IsNullOrWhiteSpace(platName) && !platformReleaseDates.ContainsKey(platName))
                            {
                                platformReleaseDates[platName] = rd.Date.Value;
                            }
                        }
                    }
                }

                foreach (var platform in game.Platforms)
                {
                    // Try to find a platform-specific release date; fall back to the game's first release date
                    DateTime? platformAirDate = null;
                    if (platform.Title != null && platformReleaseDates.TryGetValue(platform.Title, out var platDate))
                    {
                        platformAirDate = platDate.UtcDateTime;
                    }
                    else
                    {
                        platformAirDate = game.FirstAired;
                    }

                    roms.Add(new Rom
                    {
                        GameId = 0,
                        PlatformNumber = platform.PlatformNumber,
                        FileNumber = 1,
                        Title = game.Title,
                        Overview = game.Overview,
                        AirDate = platformAirDate?.ToString("yyyy-MM-dd"),
                        AirDateUtc = platformAirDate?.ToUniversalTime(),
                        Ratings = game.Ratings,
                        Monitored = true,
                        RomType = RomFileType.Base
                    });
                }
            }

            // Fetch DLCs and expansions from IGDB to populate additional ROM entries per platform
            try
            {
                var dlcIds = new List<long>();

                if (games[0].Dlcs?.Ids != null)
                {
                    dlcIds.AddRange(games[0].Dlcs.Ids);
                }

                if (games[0].Expansions?.Ids != null)
                {
                    dlcIds.AddRange(games[0].Expansions.Ids);
                }

                if (dlcIds.Any())
                {
                    var idList = string.Join(",", dlcIds.Take(50));
                    var dlcQuery = $"fields id,name,first_release_date,summary,platforms.name,platforms.abbreviation; where id = ({idList}); limit 50;";
                    var dlcResults = _igdbClient.SearchGames(dlcQuery);

                    if (dlcResults != null)
                    {
                        // Sort DLCs by release date, putting items without dates at the end
                        var sortedDlcs = dlcResults
                            .OrderBy(d => d.FirstReleaseDate?.DateTime ?? DateTime.MaxValue)
                            .ToList();

                        foreach (var dlc in sortedDlcs)
                        {
                            if (game.Platforms == null)
                            {
                                continue;
                            }

                            foreach (var platform in game.Platforms)
                            {
                                var romNumber = roms.Count(r => r.PlatformNumber == platform.PlatformNumber) + 1;

                                roms.Add(new Rom
                                {
                                    GameId = 0,
                                    PlatformNumber = platform.PlatformNumber,
                                    FileNumber = romNumber,
                                    Title = dlc.Name ?? "Unknown DLC",
                                    Overview = dlc.Summary,
                                    AirDate = dlc.FirstReleaseDate?.DateTime.ToString("yyyy-MM-dd"),
                                    AirDateUtc = dlc.FirstReleaseDate?.DateTime.ToUniversalTime(),
                                    Monitored = true,
                                    RomType = RomFileType.Dlc
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Failed to fetch DLC/expansion info for game {0}", game.Title);
            }

            // Enrich Nintendo Switch platforms with tinfoil title database (updates + DLCs)
            try
            {
                var switchPlatforms = game.Platforms?.Where(p =>
                    p.Title != null &&
                    p.Title.Contains("Switch", StringComparison.OrdinalIgnoreCase)).ToList();

                if (switchPlatforms != null && switchPlatforms.Any())
                {
                    // First find the Switch title ID via the bulk cache
                    var tinfoilTitles = _tinfoilProxy.GetTitlesForGame(game.Title);
                    var baseTitleId = tinfoilTitles.FirstOrDefault(t => t.Type == "Base")?.Id;

                    // Fetch per-title patches and DLCs from the Tinfoil page
                    var titleDetails = new List<TinfoilTitle>();
                    if (!string.IsNullOrEmpty(baseTitleId))
                    {
                        titleDetails = _tinfoilProxy.GetTitleDetails(baseTitleId);
                    }

                    if (titleDetails.Any())
                    {
                        foreach (var platform in switchPlatforms)
                        {
                            foreach (var tinfoilTitle in titleDetails)
                            {
                                var romNumber = roms.Count(r => r.PlatformNumber == platform.PlatformNumber) + 1;
                                var romType = tinfoilTitle.Type == "Update" ? RomFileType.Update : RomFileType.Dlc;

                                DateTime? airDateUtc = null;
                                if (!string.IsNullOrEmpty(tinfoilTitle.ReleaseDate) &&
                                    DateTime.TryParse(tinfoilTitle.ReleaseDate, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out var parsedDate))
                                {
                                    airDateUtc = parsedDate.ToUniversalTime();
                                }

                                roms.Add(new Rom
                                {
                                    GameId = 0,
                                    PlatformNumber = platform.PlatformNumber,
                                    FileNumber = romNumber,
                                    Title = tinfoilTitle.Name ?? game.Title,
                                    Overview = $"Tinfoil ID: {tinfoilTitle.Id}, Version: {tinfoilTitle.Version ?? "N/A"}",
                                    AirDate = tinfoilTitle.ReleaseDate,
                                    AirDateUtc = airDateUtc,
                                    Monitored = tinfoilTitle.Type == "Update",
                                    RomType = romType
                                });
                            }
                        }

                        _logger.Info("Added {0} tinfoil entries for Switch game '{1}'", titleDetails.Count, game.Title);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Failed to enrich Switch ROMs from Tinfoil for game {0}", game.Title);
            }

            // Enrich Wii U platforms with update/DLC data
            try
            {
                var wiiuPlatforms = game.Platforms?.Where(p =>
                    p.Title != null &&
                    p.Title.Contains("Wii U", StringComparison.OrdinalIgnoreCase)).ToList();

                if (wiiuPlatforms != null && wiiuPlatforms.Any())
                {
                    var wiiuTitles = _wiiuProxy.GetTitleDetails(game.Title);

                    if (wiiuTitles.Any())
                    {
                        foreach (var platform in wiiuPlatforms)
                        {
                            foreach (var wiiuTitle in wiiuTitles)
                            {
                                var romNumber = roms.Count(r => r.PlatformNumber == platform.PlatformNumber) + 1;
                                var romType = wiiuTitle.Type == "Update" ? RomFileType.Update : RomFileType.Dlc;

                                DateTime? airDateUtc = null;
                                if (!string.IsNullOrEmpty(wiiuTitle.ReleaseDate) &&
                                    DateTime.TryParse(wiiuTitle.ReleaseDate, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out var parsedDate))
                                {
                                    airDateUtc = parsedDate.ToUniversalTime();
                                }

                                roms.Add(new Rom
                                {
                                    GameId = 0,
                                    PlatformNumber = platform.PlatformNumber,
                                    FileNumber = romNumber,
                                    Title = wiiuTitle.Name ?? game.Title,
                                    Overview = $"Wii U Title ID: {wiiuTitle.Id}, Version: {wiiuTitle.Version ?? "N/A"}",
                                    AirDate = wiiuTitle.ReleaseDate,
                                    AirDateUtc = airDateUtc,
                                    Monitored = wiiuTitle.Type == "Update",
                                    RomType = romType
                                });
                            }
                        }

                        _logger.Info("Added {0} Wii U entries for game '{1}'", wiiuTitles.Count, game.Title);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Failed to enrich Wii U ROMs for game {0}", game.Title);
            }

            return new Tuple<Game, List<Rom>>(game, roms);
        }

        public List<Game> SearchForNewGameByImdbId(string imdbId)
        {
            return new List<Game>();
        }

        public List<Game> SearchForNewGameByAniListId(int aniListId)
        {
            return new List<Game>();
        }

        public List<Game> SearchForNewGameByMyAnimeListId(int malId)
        {
            return new List<Game>();
        }

        public List<Game> SearchForNewGameByTmdbId(int tmdbId)
        {
            return new List<Game>();
        }

        public List<Game> SearchForNewGame(string title)
        {
            if (title.IsPathValid(PathValidationType.AnyOs))
            {
                throw new InvalidSearchTermException("Invalid search term '{0}'", title);
            }

            try
            {
                var lowerTitle = title.ToLowerInvariant();

                if (lowerTitle.StartsWith("igdb:") || lowerTitle.StartsWith("igdbid:"))
                {
                    var slug = lowerTitle.Split(':')[1].Trim();

                    if (slug.IsNullOrWhiteSpace() || slug.Any(char.IsWhiteSpace) || !int.TryParse(slug, out var igdbId) || igdbId <= 0)
                    {
                        return new List<Game>();
                    }

                    try
                    {
                        var existingGame = _gameService.FindByIgdbId(igdbId);
                        if (existingGame != null)
                        {
                            return new List<Game> { existingGame };
                        }

                        return new List<Game> { GetGameInfo(igdbId).Item1 };
                    }
                    catch (GameNotFoundException)
                    {
                        return new List<Game>();
                    }
                }

                var escapedTitle = title.Replace("\"", "\\\"").Trim();
                var query = $"{GameFields} search \"{escapedTitle}\"; limit 20;";
                var games = _igdbClient.SearchGames(query);

                if (games == null)
                {
                    return new List<Game>();
                }

                return games.Select(MapSearchResult).ToList();
            }
            catch (HttpException ex)
            {
                _logger.Warn(ex);
                throw new SkyHookException("Search for '{0}' failed. Unable to communicate with IGDB. {1}", ex, title, ex.Message);
            }
            catch (WebException ex)
            {
                _logger.Warn(ex);
                throw new SkyHookException("Search for '{0}' failed. Unable to communicate with IGDB. {1}", ex, title, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
                throw new SkyHookException("Search for '{0}' failed. Invalid response received from IGDB. {1}", ex, title, ex.Message);
            }
        }

        private Game MapSearchResult(IGDB.Models.Game igdbGame)
        {
            if (igdbGame.Id == null)
            {
                return new Game();
            }

            var gameId = (int)igdbGame.Id.Value;
            var existingGame = _gameService.FindByIgdbId(gameId);

            if (existingGame != null)
            {
                return existingGame;
            }

            return MapIgdbGame(igdbGame);
        }

        private Game MapIgdbGame(IGDB.Models.Game igdbGame)
        {
            var gameId = igdbGame.Id.HasValue ? (int)igdbGame.Id.Value : 0;
            var title = igdbGame.Name ?? string.Empty;

            var game = new Game
            {
                IgdbId = gameId,
                Title = title,
                CleanTitle = Romarr.Core.Parser.Parser.CleanGameTitle(title),
                SortTitle = GameTitleNormalizer.Normalize(title, gameId),
                Overview = igdbGame.Summary,
                TitleSlug = igdbGame.Slug,
                Status = MapIgdbStatus(igdbGame),
                OriginalLanguage = Language.English,
                Monitored = true,
                Ratings = new Ratings(),
                Images = new List<MediaCover.MediaCover>(),
                Platforms = new List<Platform>(),
                Genres = new List<string>(),
                Actors = new List<Actor>()
            };

            if (igdbGame.FirstReleaseDate.HasValue)
            {
                game.FirstAired = igdbGame.FirstReleaseDate.Value.UtcDateTime;
                game.Year = igdbGame.FirstReleaseDate.Value.Year;
            }

            if (igdbGame.Rating.HasValue && igdbGame.RatingCount.HasValue)
            {
                game.Ratings = new Ratings
                {
                    Value = (decimal)(igdbGame.Rating.Value / 10.0),
                    Votes = igdbGame.RatingCount.Value
                };
            }

            if (_configService.RatingSource == "metacritic")
            {
                try
                {
                    var metacriticScore = _metacriticProxy.GetMetacriticScore(game.Title, game.Year);

                    if (metacriticScore.HasValue)
                    {
                        game.Ratings = new Ratings
                        {
                            Value = metacriticScore.Value,
                            Votes = game.Ratings?.Votes ?? 0
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "Failed to fetch Metacritic score for {0}", game.Title);
                }
            }

            if (igdbGame.Genres?.Values != null)
            {
                game.Genres = igdbGame.Genres.Values
                    .Where(g => g != null && !string.IsNullOrWhiteSpace(g.Name))
                    .Select(g => g.Name)
                    .ToList();
            }

            if (igdbGame.InvolvedCompanies?.Values != null)
            {
                var developer = igdbGame.InvolvedCompanies.Values.FirstOrDefault(c => c?.Developer == true && c.Company?.Value != null);
                if (developer?.Company?.Value != null)
                {
                    game.Network = developer.Company.Value.Name;
                }
                else
                {
                    var publisher = igdbGame.InvolvedCompanies.Values.FirstOrDefault(c => c?.Publisher == true && c.Company?.Value != null);
                    if (publisher?.Company?.Value != null)
                    {
                        game.Network = publisher.Company.Value.Name;
                    }
                }
            }

            if (igdbGame.Platforms?.Values != null)
            {
                game.Platforms = igdbGame.Platforms.Values
                    .Where(p => p != null)
                    .Select((p, i) => new Platform
                    {
                        PlatformNumber = i + 1,
                        Title = p.Name ?? p.Abbreviation ?? $"Platform {i + 1}",
                        Images = new List<MediaCover.MediaCover>(),
                        Monitored = true
                    }).ToList();
            }

            var coverImageId = igdbGame.Cover?.Value?.ImageId;
            if (!string.IsNullOrWhiteSpace(coverImageId))
            {
                game.Images.Add(new MediaCover.MediaCover
                {
                    CoverType = MediaCoverTypes.Poster,
                    RemoteUrl = NormalizeImageUrl(ImageHelper.GetImageUrl(coverImageId, ImageSize.CoverBig))
                });
            }

            if (igdbGame.Screenshots?.Values != null)
            {
                foreach (var screenshot in igdbGame.Screenshots.Values.Where(s => s != null && !string.IsNullOrWhiteSpace(s.ImageId)).Take(3))
                {
                    game.Images.Add(new MediaCover.MediaCover
                    {
                        CoverType = MediaCoverTypes.Fanart,
                        RemoteUrl = NormalizeImageUrl(ImageHelper.GetImageUrl(screenshot.ImageId, ImageSize.ScreenshotBig))
                    });
                }
            }

            if (igdbGame.Artworks?.Values != null && igdbGame.Artworks.Values.Any())
            {
                foreach (var artwork in igdbGame.Artworks.Values.Where(a => a != null && !string.IsNullOrWhiteSpace(a.ImageId)).Take(2))
                {
                    game.Images.Add(new MediaCover.MediaCover
                    {
                        CoverType = MediaCoverTypes.Banner,
                        RemoteUrl = NormalizeImageUrl(ImageHelper.GetImageUrl(artwork.ImageId, ImageSize.ScreenshotBig))
                    });
                }
            }

            return game;
        }

        private static string NormalizeImageUrl(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return imageUrl;
            }

            return imageUrl.StartsWith("//") ? $"https:{imageUrl}" : imageUrl;
        }

        private static GameStatusType MapIgdbStatus(IGDB.Models.Game game)
        {
            var statusId = game.GameStatus?.Id;

            // IGDB game_status IDs: 0 released, 2 alpha, 3 beta, 4 early_access, 5 offline, 6 cancelled, 7 rumored, 8 delisted
            if (!statusId.HasValue || statusId.Value == 0)
            {
                return GameStatusType.Ended;
            }

            if (statusId.Value == 2 || statusId.Value == 3 || statusId.Value == 4 || statusId.Value == 7)
            {
                return GameStatusType.Upcoming;
            }

            return GameStatusType.Continuing;
        }
    }
}
