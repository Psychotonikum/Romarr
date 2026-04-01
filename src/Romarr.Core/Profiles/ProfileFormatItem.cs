using Romarr.Core.CustomFormats;
using Romarr.Core.Datastore;

namespace Romarr.Core.Profiles
{
    public class ProfileFormatItem : IEmbeddedDocument
    {
        public CustomFormat Format { get; set; }
        public int Score { get; set; }
    }
}
