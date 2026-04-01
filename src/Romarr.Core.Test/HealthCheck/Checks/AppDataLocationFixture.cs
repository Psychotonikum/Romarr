using Moq;
using NUnit.Framework;
using Romarr.Common.EnvironmentInfo;
using Romarr.Core.HealthCheck.Checks;
using Romarr.Core.Localization;
using Romarr.Core.Test.Framework;
using Romarr.Test.Common;

namespace Romarr.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class AppDataLocationFixture : CoreTest<AppDataLocationCheck>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<ILocalizationService>()
                  .Setup(s => s.GetLocalizedString(It.IsAny<string>()))
                  .Returns("Some Warning Message");
        }

        [Test]
        public void should_return_warning_when_app_data_is_child_of_startup_folder()
        {
            Mocker.GetMock<IAppFolderInfo>()
                  .Setup(s => s.StartUpFolder)
                  .Returns(@"C:\Romarr".AsOsAgnostic());

            Mocker.GetMock<IAppFolderInfo>()
                  .Setup(s => s.AppDataFolder)
                  .Returns(@"C:\Romarr\AppData".AsOsAgnostic());

            Subject.Check().ShouldBeWarning();
        }

        [Test]
        public void should_return_warning_when_app_data_is_same_as_startup_folder()
        {
            Mocker.GetMock<IAppFolderInfo>()
                  .Setup(s => s.StartUpFolder)
                  .Returns(@"C:\Romarr".AsOsAgnostic());

            Mocker.GetMock<IAppFolderInfo>()
                  .Setup(s => s.AppDataFolder)
                  .Returns(@"C:\Romarr".AsOsAgnostic());

            Subject.Check().ShouldBeWarning();
        }

        [Test]
        public void should_return_ok_when_no_conflict()
        {
            Mocker.GetMock<IAppFolderInfo>()
                  .Setup(s => s.StartUpFolder)
                  .Returns(@"C:\Romarr".AsOsAgnostic());

            Mocker.GetMock<IAppFolderInfo>()
                  .Setup(s => s.AppDataFolder)
                  .Returns(@"C:\ProgramData\Romarr".AsOsAgnostic());

            Subject.Check().ShouldBeOk();
        }
    }
}
