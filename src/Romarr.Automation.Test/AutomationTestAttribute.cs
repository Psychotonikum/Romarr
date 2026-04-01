using NUnit.Framework;

namespace Romarr.Automation.Test
{
    public class AutomationTestAttribute : CategoryAttribute
    {
        public AutomationTestAttribute()
            : base("AutomationTest")
        {
        }
    }
}
