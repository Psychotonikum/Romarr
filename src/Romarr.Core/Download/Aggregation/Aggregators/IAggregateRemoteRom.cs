using Romarr.Core.Parser.Model;

namespace Romarr.Core.Download.Aggregation.Aggregators
{
    public interface IAggregateRemoteGameFile
    {
        RemoteRom Aggregate(RemoteRom remoteRom);
    }
}
