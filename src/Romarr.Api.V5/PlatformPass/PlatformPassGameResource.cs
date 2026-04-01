using Romarr.Api.V5.Game;

namespace Romarr.Api.V5.PlatformPass;

public class PlatformPassGameResource
{
    public int Id { get; set; }
    public bool? Monitored { get; set; }
    public List<PlatformResource> Platforms { get; set; } = [];
}
