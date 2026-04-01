using System;
using Equ;
using Romarr.Core.Indexers;
using Romarr.Core.ThingiProvider;

namespace Romarr.Core.Download
{
    public class DownloadClientDefinition : ProviderDefinition, IEquatable<DownloadClientDefinition>
    {
        private static readonly MemberwiseEqualityComparer<DownloadClientDefinition> Comparer = MemberwiseEqualityComparer<DownloadClientDefinition>.ByProperties;

        [MemberwiseEqualityIgnore]
        public DownloadProtocol Protocol { get; set; }

        public int Priority { get; set; } = 1;

        public bool RemoveCompletedDownloads { get; set; } = true;
        public bool RemoveFailedDownloads { get; set; } = true;

        public bool Equals(DownloadClientDefinition other)
        {
            return Comparer.Equals(this, other);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DownloadClientDefinition);
        }

        public override int GetHashCode()
        {
            return Comparer.GetHashCode(this);
        }
    }
}
