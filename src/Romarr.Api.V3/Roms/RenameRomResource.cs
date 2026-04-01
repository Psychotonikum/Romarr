using System.Collections.Generic;
using System.Linq;
using Romarr.Http.REST;

namespace Romarr.Api.V3.Roms
{
    public class RenameRomResource : RestResource
    {
        public int GameId { get; set; }
        public int PlatformNumber { get; set; }
        public List<int> RomNumbers { get; set; }
        public int RomFileId { get; set; }
        public string ExistingPath { get; set; }
        public string NewPath { get; set; }
    }

    public static class RenameRomResourceMapper
    {
        public static RenameRomResource ToResource(this Romarr.Core.MediaFiles.RenameRomFilePreview model)
        {
            if (model == null)
            {
                return null;
            }

            return new RenameRomResource
            {
                Id = model.RomFileId,
                GameId = model.GameId,
                PlatformNumber = model.PlatformNumber,
                RomNumbers = model.RomNumbers.ToList(),
                RomFileId = model.RomFileId,
                ExistingPath = model.ExistingPath,
                NewPath = model.NewPath
            };
        }

        public static List<RenameRomResource> ToResource(this IEnumerable<Romarr.Core.MediaFiles.RenameRomFilePreview> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
