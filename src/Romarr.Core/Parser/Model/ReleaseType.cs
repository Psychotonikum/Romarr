using Romarr.Core.Annotations;

namespace Romarr.Core.Parser.Model
{
    public enum ReleaseType
    {
        Unknown = 0,

        [FieldOption(label: "Single Rom")]
        SingleGameFile = 1,

        [FieldOption(label: "Multi-Rom")]
        MultiGameFile = 2,

        [FieldOption(label: "Platform Pack")]
        PlatformPack = 3
    }
}
