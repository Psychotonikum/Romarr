using Romarr.Core.DecisionEngine;

namespace Romarr.Core.MediaFiles.GameFileImport;

public class ImportRejection : Rejection<ImportRejectionReason>
{
    public ImportRejection(ImportRejectionReason reason, string message, RejectionType type = RejectionType.Permanent)
        : base(reason, message, type)
    {
    }
}
