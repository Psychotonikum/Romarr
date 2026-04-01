using NUnit.Framework;

namespace Romarr.Test.Common.Categories
{
    public class ManualTestAttribute : CategoryAttribute
    {
        public ManualTestAttribute()
            : base("ManualTest")
        {
        }
    }
}
