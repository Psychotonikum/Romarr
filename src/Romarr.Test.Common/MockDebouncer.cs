using System;
using Romarr.Common.TPL;

namespace Romarr.Test.Common
{
    public class MockDebouncer : Debouncer
    {
        public MockDebouncer(Action action, TimeSpan debounceDuration)
            : base(action, debounceDuration)
        {
        }

        public override void Execute()
        {
            lock (_timer)
            {
                _action();
            }
        }
    }
}
