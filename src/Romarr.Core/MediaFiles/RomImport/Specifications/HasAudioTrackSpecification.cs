using NLog;
using Romarr.Core.Download;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.MediaFiles.GameFileImport.Specifications
{
    public class HasAudioTrackSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public HasAudioTrackSpecification(Logger logger)
        {
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalGameFile localRom, DownloadClientItem downloadClientItem)
        {
            if (localRom.MediaInfo == null)
            {
                _logger.Debug("Failed to get media info from the file, make sure ffprobe is available, skipping check");
                return ImportSpecDecision.Accept();
            }

            if (localRom.MediaInfo.AudioStreams == null || localRom.MediaInfo.AudioStreams.Count == 0)
            {
                _logger.Debug("No audio tracks found in file");

                return ImportSpecDecision.Reject(ImportRejectionReason.NoAudio, "No audio tracks detected");
            }

            return ImportSpecDecision.Accept();
        }
    }
}
