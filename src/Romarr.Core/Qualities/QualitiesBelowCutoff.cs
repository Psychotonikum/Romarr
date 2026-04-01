using System.Collections.Generic;

namespace Romarr.Core.Qualities
{
    public class QualitiesBelowCutoff
    {
        public int ProfileId { get; set; }
        public IEnumerable<int> QualityIds { get; set; }

        public QualitiesBelowCutoff(int profileId, IEnumerable<int> qualityIds)
        {
            ProfileId = profileId;
            QualityIds = qualityIds;
        }
    }
}
