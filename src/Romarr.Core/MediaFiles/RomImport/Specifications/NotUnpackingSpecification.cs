using System;
using System.IO;
using NLog;
using Romarr.Common.Disk;
using Romarr.Common.EnvironmentInfo;
using Romarr.Core.Configuration;
using Romarr.Core.Download;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.MediaFiles.GameFileImport.Specifications
{
    public class NotUnpackingSpecification : IImportDecisionEngineSpecification
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public NotUnpackingSpecification(IDiskProvider diskProvider, IConfigService configService, Logger logger)
        {
            _diskProvider = diskProvider;
            _configService = configService;
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalGameFile localRom, DownloadClientItem downloadClientItem)
        {
            if (localRom.ExistingFile)
            {
                _logger.Debug("{0} is in game folder, skipping unpacking check", localRom.Path);
                return ImportSpecDecision.Accept();
            }

            foreach (var workingFolder in _configService.DownloadClientWorkingFolders.Split('|'))
            {
                var parent = Directory.GetParent(localRom.Path);
                while (parent != null)
                {
                    if (parent.Name.StartsWith(workingFolder))
                    {
                        if (OsInfo.IsNotWindows)
                        {
                            _logger.Debug("{0} is still being unpacked", localRom.Path);
                            return ImportSpecDecision.Reject(ImportRejectionReason.Unpacking, "File is still being unpacked");
                        }

                        if (_diskProvider.FileGetLastWrite(localRom.Path) > DateTime.UtcNow.AddMinutes(-5))
                        {
                            _logger.Debug("{0} appears to be unpacking still", localRom.Path);
                            return ImportSpecDecision.Reject(ImportRejectionReason.Unpacking, "File is still being unpacked");
                        }
                    }

                    parent = parent.Parent;
                }
            }

            return ImportSpecDecision.Accept();
        }
    }
}
