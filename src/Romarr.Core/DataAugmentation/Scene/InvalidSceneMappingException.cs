using System.Collections.Generic;
using System.Linq;
using Romarr.Common.Exceptions;

namespace Romarr.Core.DataAugmentation.Scene
{
    public class InvalidSceneMappingException : RomarrException
    {
        public InvalidSceneMappingException(IEnumerable<SceneMapping> mappings, string releaseTitle)
            : base(FormatMessage(mappings, releaseTitle))
        {
        }

        private static string FormatMessage(IEnumerable<SceneMapping> mappings, string releaseTitle)
        {
            return string.Format("Scene Mappings contains a conflict for igdbids {0}. Please notify Romarr developers. ({1})", string.Join(",", mappings.Select(v => v.IgdbId.ToString())), releaseTitle);
        }
    }
}
