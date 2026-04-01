using NUnit.Framework;

namespace Romarr.Test.Common.Categories
{
    public class IntegrationTestAttribute : CategoryAttribute
    {
        public IntegrationTestAttribute()
            : base("IntegrationTest")
        {
        }
    }
}
