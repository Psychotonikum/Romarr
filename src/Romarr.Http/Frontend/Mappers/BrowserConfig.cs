using System.IO;
using NLog;
using Romarr.Common.Disk;
using Romarr.Common.EnvironmentInfo;
using Romarr.Core.Configuration;

namespace Romarr.Http.Frontend.Mappers
{
    public class BrowserConfig : UrlBaseReplacementResourceMapperBase
    {
        public BrowserConfig(IAppFolderInfo appFolderInfo, IDiskProvider diskProvider, IConfigFileProvider configFileProvider, Logger logger)
            : base(diskProvider, configFileProvider, logger)
        {
            FilePath = Path.Combine(appFolderInfo.StartUpFolder, configFileProvider.UiFolder, "Content", "browserconfig.xml");
        }

        public override string Map(string resourceUrl)
        {
            return FilePath;
        }

        public override bool CanHandle(string resourceUrl)
        {
            return resourceUrl.StartsWith("/Content/browserconfig");
        }
    }
}
