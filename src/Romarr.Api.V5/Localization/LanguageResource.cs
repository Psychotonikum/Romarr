using System.Text.Json.Serialization;
using Romarr.Http.REST;

namespace Romarr.Api.V5.Localization;

public class LanguageResource : RestResource
{
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public new int Id { get; set; }
    public string? Name { get; set; }
    public string? NameLower => Name?.ToLowerInvariant();
}
