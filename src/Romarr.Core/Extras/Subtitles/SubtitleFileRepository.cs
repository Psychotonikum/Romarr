using Romarr.Core.Datastore;
using Romarr.Core.Extras.Files;
using Romarr.Core.Messaging.Events;

namespace Romarr.Core.Extras.Subtitles
{
    public interface ISubtitleFileRepository : IExtraFileRepository<SubtitleFile>
    {
    }

    public class SubtitleFileRepository : ExtraFileRepository<SubtitleFile>, ISubtitleFileRepository
    {
        public SubtitleFileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
