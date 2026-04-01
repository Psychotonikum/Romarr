using Moq;
using NUnit.Framework;
using Romarr.Common.Disk;
using Romarr.Common.EnvironmentInfo;
using Romarr.Core.Configuration;
using Romarr.Core.HealthCheck.Checks;
using Romarr.Core.Localization;
using Romarr.Core.Test.Framework;
using Romarr.Core.Update;
using Romarr.Test.Common;

namespace Romarr.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class UpdateCheckFixture : CoreTest<UpdateCheck>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<ILocalizationService>()
                  .Setup(s => s.GetLocalizedString(It.IsAny<string>()))
                  .Returns("Some Warning Message");
        }

        [Test]
        public void should_return_error_when_app_folder_is_write_protected_and_update_automatically_is_enabled()
        {
            var startupFolder = @"C:\Romarr".AsOsAgnostic();

            Mocker.GetMock<IConfigFileProvider>()
                  .Setup(s => s.UpdateAutomatically)
                  .Returns(true);

            Mocker.GetMock<IAppFolderInfo>()
                  .Setup(s => s.StartUpFolder)
                  .Returns(startupFolder);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(c => c.FolderWritable(startupFolder))
                  .Returns(false);

            Subject.Check().ShouldBeError();
        }

        [Test]
        public void should_return_error_when_ui_folder_is_write_protected_and_update_automatically_is_enabled()
        {
            var startupFolder = @"C:\Romarr".AsOsAgnostic();
            var uiFolder = @"C:\Romarr\UI".AsOsAgnostic();

            Mocker.GetMock<IConfigFileProvider>()
                  .Setup(s => s.UpdateAutomatically)
                  .Returns(true);

            Mocker.GetMock<IAppFolderInfo>()
                  .Setup(s => s.StartUpFolder)
                  .Returns(startupFolder);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(c => c.FolderWritable(startupFolder))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(c => c.FolderWritable(uiFolder))
                  .Returns(false);

            Subject.Check().ShouldBeError();
        }

        [Test]
        public void should_not_return_error_when_app_folder_is_write_protected_and_external_script_enabled()
        {
            var startupFolder = @"C:\Romarr".AsOsAgnostic();

            Mocker.GetMock<IConfigFileProvider>()
                  .Setup(s => s.UpdateAutomatically)
                  .Returns(true);

            Mocker.GetMock<IConfigFileProvider>()
                  .Setup(s => s.UpdateMechanism)
                  .Returns(UpdateMechanism.Script);

            Mocker.GetMock<IAppFolderInfo>()
                  .Setup(s => s.StartUpFolder)
                  .Returns(startupFolder);

            Mocker.GetMock<IDiskProvider>()
                  .Verify(c => c.FolderWritable(It.IsAny<string>()), Times.Never());

            Subject.Check().ShouldBeOk();
        }
    }
}
