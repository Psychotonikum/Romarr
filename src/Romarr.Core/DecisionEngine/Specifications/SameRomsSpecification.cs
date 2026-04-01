using System.Collections.Generic;
using System.Linq;
using Romarr.Common.Extensions;
using Romarr.Core.Games;

namespace Romarr.Core.DecisionEngine.Specifications
{
    public class SameFilesSpecification
    {
        private readonly IRomService _romService;

        public SameFilesSpecification(IRomService gameFileService)
        {
            _romService = gameFileService;
        }

        public bool IsSatisfiedBy(List<Rom> roms)
        {
            var romIds = roms.SelectList(e => e.Id);
            var romFileIds = roms.Where(c => c.RomFileId != 0).Select(c => c.RomFileId).Distinct();

            foreach (var romFileId in romFileIds)
            {
                var gameFilesInFile = _romService.GetRomsByFileId(romFileId);

                if (gameFilesInFile.Select(e => e.Id).Except(romIds).Any())
                {
                    return false;
                }
            }

            return true;
        }
    }
}
