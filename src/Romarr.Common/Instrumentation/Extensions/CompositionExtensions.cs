using DryIoc;
using NLog;

namespace Romarr.Common.Instrumentation.Extensions
{
    public static class CompositionExtensions
    {
        public static IContainer AddRomarrLogger(this IContainer container)
        {
            container.Register(Made.Of<Logger>(() => LogManager.GetLogger(Arg.Index<string>(0)), r => r.Parent.ImplementationType.Name.ToString()), reuse: Reuse.Transient);
            return container;
        }
    }
}
