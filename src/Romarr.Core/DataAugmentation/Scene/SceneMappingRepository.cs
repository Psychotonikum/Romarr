using System.Collections.Generic;
using Romarr.Core.Datastore;
using Romarr.Core.Messaging.Events;

namespace Romarr.Core.DataAugmentation.Scene
{
    public interface ISceneMappingRepository : IBasicRepository<SceneMapping>
    {
        List<SceneMapping> FindByIgdbid(int igdbId);
        void Clear(string type);
    }

    public class SceneMappingRepository : BasicRepository<SceneMapping>, ISceneMappingRepository
    {
        public SceneMappingRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<SceneMapping> FindByIgdbid(int igdbId)
        {
            return Query(x => x.IgdbId == igdbId);
        }

        public void Clear(string type)
        {
            Delete(s => s.Type == type);
        }
    }
}
