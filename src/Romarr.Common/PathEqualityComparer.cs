using System.Collections.Generic;
using Romarr.Common.EnvironmentInfo;
using Romarr.Common.Extensions;

namespace Romarr.Common
{
    public class PathEqualityComparer : IEqualityComparer<string>
    {
        public static readonly PathEqualityComparer Instance = new PathEqualityComparer();

        private PathEqualityComparer()
        {
        }

        public bool Equals(string x, string y)
        {
            return x.PathEquals(y);
        }

        public int GetHashCode(string obj)
        {
            if (OsInfo.IsWindows)
            {
                return obj.CleanFilePath().Normalize().ToLower().GetHashCode();
            }

            return obj.CleanFilePath().Normalize().GetHashCode();
        }
    }
}
