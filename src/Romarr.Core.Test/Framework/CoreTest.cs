using System;
using System.Collections.Generic;
using System.IO;
using Moq;
using NUnit.Framework;
using Romarr.Common.Cache;
using Romarr.Common.Cloud;
using Romarr.Common.EnvironmentInfo;
using Romarr.Common.Http;
using Romarr.Common.Http.Dispatchers;
using Romarr.Common.Http.Proxy;
using Romarr.Common.TPL;
using Romarr.Core.Configuration;
using Romarr.Core.Http;
using Romarr.Core.MetadataSource.Providers;
using Romarr.Core.MetadataSource.SkyHook;
using Romarr.Core.MetadataSource.Tinfoil;
using Romarr.Core.MetadataSource.WiiU;
using Romarr.Core.Security;
using Romarr.Test.Common;

namespace Romarr.Core.Test.Framework
{
    public abstract class CoreTest : TestBase
    {
        protected void UseRealHttp()
        {
            Mocker.GetMock<IPlatformInfo>().SetupGet(c => c.Version).Returns(new Version("3.0.0"));
            Mocker.GetMock<IOsInfo>().SetupGet(c => c.Version).Returns("1.0.0");
            Mocker.GetMock<IOsInfo>().SetupGet(c => c.Name).Returns("TestOS");

            Mocker.SetConstant<IHttpProxySettingsProvider>(new HttpProxySettingsProvider(Mocker.Resolve<ConfigService>()));
            Mocker.SetConstant<ICreateManagedWebProxy>(new ManagedWebProxyFactory(Mocker.Resolve<CacheManager>()));
            Mocker.SetConstant<ICertificateValidationService>(new X509CertificateValidationService(Mocker.Resolve<ConfigService>(), TestLogger));
            Mocker.SetConstant<IHttpDispatcher>(new ManagedHttpDispatcher(Mocker.Resolve<IHttpProxySettingsProvider>(), Mocker.Resolve<ICreateManagedWebProxy>(), Mocker.Resolve<ICertificateValidationService>(), Mocker.Resolve<UserAgentBuilder>(), Mocker.Resolve<CacheManager>(), TestLogger));
            Mocker.SetConstant<IHttpClient>(new HttpClient(Array.Empty<IHttpRequestInterceptor>(), Mocker.Resolve<CacheManager>(), Mocker.Resolve<RateLimitService>(), Mocker.Resolve<IHttpDispatcher>(), TestLogger));
            Mocker.SetConstant<IRomarrCloudRequestBuilder>(new RomarrCloudRequestBuilder());
        }

        protected void UseRealIgdb()
        {
            var clientId = Environment.GetEnvironmentVariable("TWITCH_CLIENT_ID") ?? string.Empty;
            var clientSecret = Environment.GetEnvironmentVariable("TWITCH_CLIENT_SECRET") ?? string.Empty;

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            {
                var searchDir = TestContext.CurrentContext.TestDirectory;

                for (var i = 0; i < 6 && searchDir != null; i++)
                {
                    var envFile = Path.Combine(searchDir, ".env.local");

                    if (File.Exists(envFile))
                    {
                        foreach (var line in File.ReadAllLines(envFile))
                        {
                            var trimmed = line.Trim();

                            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#'))
                            {
                                continue;
                            }

                            var eqIndex = trimmed.IndexOf('=');

                            if (eqIndex <= 0)
                            {
                                continue;
                            }

                            var key = trimmed[..eqIndex].Trim();
                            var value = trimmed[(eqIndex + 1)..].Trim();

                            if (key == "TWITCH_CLIENT_ID")
                            {
                                clientId = value;
                            }
                            else if (key == "TWITCH_CLIENT_SECRET")
                            {
                                clientSecret = value;
                            }
                        }

                        break;
                    }

                    searchDir = Directory.GetParent(searchDir)?.FullName;
                }
            }

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            {
                Assert.Ignore("TWITCH_CLIENT_ID and TWITCH_CLIENT_SECRET must be set (env vars or .env.local) to run IGDB integration tests.");
            }

            Mocker.GetMock<IConfigService>()
                .SetupGet(c => c.TwitchClientId)
                .Returns(clientId);

            Mocker.GetMock<IConfigService>()
                .SetupGet(c => c.TwitchClientSecret)
                .Returns(clientSecret);

            Mocker.GetMock<IMetadataSourceProviderFactory>()
                .Setup(f => f.All())
                .Returns(new List<MetadataSourceDefinition>());

            Mocker.GetMock<ITinfoilProxy>()
                .Setup(t => t.GetTitlesForGame(It.IsAny<string>()))
                .Returns(new List<TinfoilTitle>());

            Mocker.GetMock<ITinfoilProxy>()
                .Setup(t => t.GetTitleDetails(It.IsAny<string>()))
                .Returns(new List<TinfoilTitle>());

            Mocker.GetMock<IWiiUTitleProxy>()
                .Setup(t => t.GetTitleDetails(It.IsAny<string>()))
                .Returns(new List<WiiUTitle>());

            Mocker.SetConstant<IIgdbClient>(
                new IgdbClient(
                    Mocker.Resolve<IConfigService>(),
                    Mocker.Resolve<IMetadataSourceProviderFactory>(),
                    TestLogger));
        }
    }

    public abstract class CoreTest<TSubject> : CoreTest
        where TSubject : class
    {
        private TSubject _subject;

        [SetUp]
        public void CoreTestSetup()
        {
            _subject = null;
        }

        protected TSubject Subject
        {
            get
            {
                if (_subject == null)
                {
                    _subject = Mocker.Resolve<TSubject>();
                }

                return _subject;
            }
        }
    }
}
