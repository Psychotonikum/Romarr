using System.Reflection;
using System.Text;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;

namespace Romarr.Common.Instrumentation
{
    [ThreadAgnostic]
    [LayoutRenderer("version")]
    public class VersionLayoutRenderer : LayoutRenderer
    {
        private static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            builder.Append(Version);
        }
    }
}
