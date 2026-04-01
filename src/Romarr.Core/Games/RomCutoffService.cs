using System.Collections.Generic;
using System.Linq;
using Romarr.Common.Extensions;
using Romarr.Core.Datastore;
using Romarr.Core.Profiles.Qualities;
using Romarr.Core.Qualities;

namespace Romarr.Core.Games
{
    public interface IFileCutoffService
    {
        PagingSpec<Rom> GameFilesWhereCutoffUnmet(PagingSpec<Rom> pagingSpec);
    }

    public class FileCutoffService : IFileCutoffService
    {
        private readonly IRomRepository _romRepository;
        private readonly IQualityProfileService _qualityProfileService;

        public FileCutoffService(IRomRepository gameFileRepository, IQualityProfileService qualityProfileService)
        {
            _romRepository = gameFileRepository;
            _qualityProfileService = qualityProfileService;
        }

        public PagingSpec<Rom> GameFilesWhereCutoffUnmet(PagingSpec<Rom> pagingSpec)
        {
            var qualitiesBelowCutoff = new List<QualitiesBelowCutoff>();
            var profiles = _qualityProfileService.All();

            // Get all items less than the cutoff
            foreach (var profile in profiles)
            {
                var cutoff = profile.UpgradeAllowed ? profile.Cutoff : profile.FirststAllowedQuality().Id;
                var cutoffIndex = profile.GetIndex(cutoff);
                var belowCutoff = profile.Items.Take(cutoffIndex.Index).ToList();

                if (belowCutoff.Any())
                {
                    qualitiesBelowCutoff.Add(new QualitiesBelowCutoff(profile.Id, belowCutoff.SelectMany(i => i.GetQualities().Select(q => q.Id))));
                }
            }

            if (qualitiesBelowCutoff.Empty())
            {
                pagingSpec.Records = new List<Rom>();

                return pagingSpec;
            }

            return _romRepository.GameFilesWhereCutoffUnmet(pagingSpec, qualitiesBelowCutoff, false);
        }
    }
}
