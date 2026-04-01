using Romarr.Common.Extensions;
using Romarr.Core.DataAugmentation.Scene;

namespace Romarr.Api.V5.Game;

public class AlternateTitleResource
{
    public string? Title { get; set; }
    public int? PlatformNumber { get; set; }
    public int? ScenePlatformNumber { get; set; }
    public string? SceneOrigin { get; set; }
    public string? Comment { get; set; }
}

public static class AlternateTitleResourceMapper
{
    public static AlternateTitleResource ToResource(this SceneMapping sceneMapping)
    {
        var comment = sceneMapping.Comment;

        if (comment.IsNullOrWhiteSpace() && sceneMapping.FilterRegex.IsNotNullOrWhiteSpace())
        {
            comment = "Limited matching";
        }

        return new AlternateTitleResource
        {
            Title = sceneMapping.Title,
            PlatformNumber = sceneMapping.PlatformNumber,
            ScenePlatformNumber = sceneMapping.ScenePlatformNumber,
            SceneOrigin = sceneMapping.SceneOrigin,
            Comment = comment
        };
    }
}
