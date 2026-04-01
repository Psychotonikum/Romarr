using NUnit.Framework;

namespace Romarr.Test.Common.Categories
{
    public class DiskAccessTestAttribute : CategoryAttribute
    {
        public DiskAccessTestAttribute()
            : base("DiskAccessTest")
        {
        }
    }
}
