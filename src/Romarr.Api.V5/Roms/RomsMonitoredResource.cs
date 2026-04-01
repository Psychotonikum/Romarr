namespace Romarr.Api.V5.Roms;

public class FilesMonitoredResource
{
    public required List<int> RomIds { get; set; }
    public bool Monitored { get; set; }
}
