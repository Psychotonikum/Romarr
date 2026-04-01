using Romarr.Core.Download;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.MediaFiles.GameFileImport
{
    public interface IImportDecisionEngineSpecification
    {
        ImportSpecDecision IsSatisfiedBy(LocalGameFile localRom, DownloadClientItem downloadClientItem);
    }
}
