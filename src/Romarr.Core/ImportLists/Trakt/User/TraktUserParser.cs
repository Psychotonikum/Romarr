using System.Collections.Generic;
using System.Linq;
using Romarr.Common.Extensions;
using Romarr.Common.Serializer;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.ImportLists.Trakt.User
{
    public class TraktUserParser : TraktParser
    {
        private readonly TraktUserSettings _settings;
        private ImportListResponse _importResponse;

        public TraktUserParser(TraktUserSettings settings)
        {
            _settings = settings;
        }

        public override IList<ImportListItemInfo> ParseResponse(ImportListResponse importResponse)
        {
            _importResponse = importResponse;

            var listItems = new List<ImportListItemInfo>();

            if (!PreProcess(_importResponse))
            {
                return listItems;
            }

            var traktSeries = new List<TraktGameResource>();

            if (_settings.TraktListType == (int)TraktUserListType.UserWatchedList)
            {
                var jsonWatchedResponse = STJson.Deserialize<List<TraktWatchedResponse>>(_importResponse.Content);

                switch (_settings.TraktWatchedListType)
                {
                    case (int)TraktUserWatchedListType.InProgress:
                        traktSeries = jsonWatchedResponse.Where(c => c.Platforms.Where(s => s.Number > 0).Sum(s => s.Roms.Count) < c.Show.AiredGameFiles).SelectList(c => c.Show);
                        break;
                    case (int)TraktUserWatchedListType.CompletelyWatched:
                        traktSeries = jsonWatchedResponse.Where(c => c.Platforms.Where(s => s.Number > 0).Sum(s => s.Roms.Count) == c.Show.AiredGameFiles).SelectList(c => c.Show);
                        break;
                    default:
                        traktSeries = jsonWatchedResponse.SelectList(c => c.Show);
                        break;
                }
            }
            else
            {
                traktSeries = STJson.Deserialize<List<TraktResponse>>(_importResponse.Content).SelectList(c => c.Show);
            }

            // no game were returned
            if (traktSeries == null)
            {
                return listItems;
            }

            foreach (var game in traktSeries)
            {
                listItems.AddIfNotNull(new ImportListItemInfo()
                {
                    Title = game.Title,
                    IgdbId = game.Ids.Igdb.GetValueOrDefault(),
                });
            }

            return listItems;
        }
    }
}
