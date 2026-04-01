using System.IO;
using Romarr.Common.Extensions;
using Romarr.Core.Parser;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.MediaFiles.GameFileImport
{
    public static class SceneNameCalculator
    {
        public static string GetSceneName(LocalGameFile localRom)
        {
            var otherVideoFiles = localRom.OtherVideoFiles;
            var downloadClientInfo = localRom.DownloadClientRomInfo;

            if (!otherVideoFiles && downloadClientInfo != null && !downloadClientInfo.FullPlatform)
            {
                return FileExtensions.RemoveFileExtension(downloadClientInfo.ReleaseTitle);
            }

            var fileName = Path.GetFileNameWithoutExtension(localRom.Path.CleanFilePath());

            if (SceneChecker.IsSceneTitle(fileName))
            {
                return fileName;
            }

            var folderTitle = localRom.FolderRomInfo?.ReleaseTitle;

            if (!otherVideoFiles &&
                localRom.FolderRomInfo?.FullPlatform == false &&
                folderTitle.IsNotNullOrWhiteSpace() &&
                SceneChecker.IsSceneTitle(folderTitle))
            {
                return folderTitle;
            }

            return null;
        }
    }
}
