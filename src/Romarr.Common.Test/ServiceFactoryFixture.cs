using System.Linq;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Romarr.Common.Composition.Extensions;
using Romarr.Common.EnvironmentInfo;
using Romarr.Common.Instrumentation.Extensions;
using Romarr.Common.Options;
using Romarr.Core.Datastore;
using Romarr.Core.Datastore.Extensions;
using Romarr.Core.Lifecycle;
using Romarr.Core.Messaging.Events;
using Romarr.Host;
using Romarr.Test.Common;

namespace Romarr.Common.Test
{
    [TestFixture]
    public class ServiceFactoryFixture : TestBase<ServiceFactory>
    {
        [Test]
        public void event_handlers_should_be_unique()
        {
            var container = new Container(rules => rules.WithRomarrRules())
                .AddRomarrLogger()
                .AutoAddServices(Bootstrap.ASSEMBLIES)
                .AddDummyDatabase()
                .AddDummyLogDatabase()
                .AddStartupContext(new StartupContext("first", "second"));

            container.RegisterInstance(new Mock<IHostLifetime>().Object);
            container.RegisterInstance(new Mock<IOptions<PostgresOptions>>().Object);
            container.RegisterInstance(new Mock<IOptions<AppOptions>>().Object);
            container.RegisterInstance(new Mock<IOptions<AuthOptions>>().Object);
            container.RegisterInstance(new Mock<IOptions<ServerOptions>>().Object);
            container.RegisterInstance(new Mock<IOptions<LogOptions>>().Object);
            container.RegisterInstance(new Mock<IOptions<UpdateOptions>>().Object);

            var serviceProvider = container.GetServiceProvider();

            serviceProvider.GetRequiredService<IAppFolderFactory>().Register();

            Mocker.SetConstant<System.IServiceProvider>(serviceProvider);

            var handlers = Subject.BuildAll<IHandle<ApplicationStartedEvent>>()
                                  .Select(c => c.GetType().FullName);

            handlers.Should().OnlyHaveUniqueItems();
        }
    }
}
