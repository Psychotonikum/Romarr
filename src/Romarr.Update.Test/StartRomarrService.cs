using System;
using Moq;
using NUnit.Framework;
using Romarr.Common;
using Romarr.Common.EnvironmentInfo;
using Romarr.Common.Extensions;
using Romarr.Common.Processes;
using Romarr.Test.Common;
using Romarr.Update.UpdateEngine;
using IServiceProvider = Romarr.Common.IServiceProvider;

namespace Romarr.Update.Test
{
    [TestFixture]
    public class StartRomarrServiceFixture : TestBase<StartRomarr>
    {
        [Test]
        public void should_start_service_if_app_type_was_serivce()
        {
            var targetFolder = "c:\\Romarr\\".AsOsAgnostic();

            Subject.Start(AppType.Service, targetFolder);

            Mocker.GetMock<IServiceProvider>().Verify(c => c.Start(ServiceProvider.SERVICE_NAME), Times.Once());
        }

        [Test]
        public void should_start_console_if_app_type_was_service_but_start_failed_because_of_permissions()
        {
            var targetFolder = "c:\\Romarr\\".AsOsAgnostic();
            var targetProcess = "c:\\Romarr\\Romarr.Console".AsOsAgnostic().ProcessNameToExe();

            Mocker.GetMock<IServiceProvider>().Setup(c => c.Start(ServiceProvider.SERVICE_NAME)).Throws(new InvalidOperationException());

            Subject.Start(AppType.Service, targetFolder);

            Mocker.GetMock<IProcessProvider>().Verify(c => c.SpawnNewProcess(targetProcess, "/" + StartupContext.NO_BROWSER, null, false), Times.Once());

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
