namespace Romarr.Core.MediaFiles
{
    public enum DeleteMediaFileReason
    {
        MissingFromDisk,
        Manual,
        Upgrade,
        NoLinkedGameFiles,
        ManualOverride
    }
}
