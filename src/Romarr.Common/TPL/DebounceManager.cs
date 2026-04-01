using System;

namespace Romarr.Common.TPL
{
    public interface IDebounceManager
    {
        Debouncer CreateDebouncer(Action action, TimeSpan debounceDuration);
    }

    public class DebounceManager : IDebounceManager
    {
        public Debouncer CreateDebouncer(Action action, TimeSpan debounceDuration)
        {
            return new Debouncer(action, debounceDuration);
        }
    }
}
