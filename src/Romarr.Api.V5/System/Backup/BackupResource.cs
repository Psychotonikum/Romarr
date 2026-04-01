using Romarr.Core.Backup;
using Romarr.Http.REST;

namespace Romarr.Api.V5.System.Backup;

public class BackupResource : RestResource
{
    public required string Name { get; set; }
    public required string Path { get; set; }
    public BackupType Type { get; set; }
    public long Size { get; set; }
    public DateTime Time { get; set; }
}
