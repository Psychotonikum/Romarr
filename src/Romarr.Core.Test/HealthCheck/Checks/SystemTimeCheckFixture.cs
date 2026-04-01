using System;
using System.Text;
using Moq;
using NUnit.Framework;
using Romarr.Common.Cloud;
using Romarr.Common.Http;
using Romarr.Common.Serializer;
using Romarr.Core.HealthCheck.Checks;
using Romarr.Core.Localization;
using Romarr.Core.Test.Framework;
using Romarr.Test.Common;

namespace Romarr.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class SystemTimeCheckFixture : CoreTest<SystemTimeCheck>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.SetConstant<IRomarrCloudRequestBuilder>(new RomarrCloudRequestBuilder());

            Mocker.GetMock<ILocalizationService>()
                .Setup(s => s.GetLocalizedString(It.IsAny<string>()))
                .Returns("Some Warning Message");
        }

        private void GivenServerTime(DateTime dateTime)
        {
            var json = new ServiceTimeResponse { DateTimeUtc = dateTime }.ToJson();

            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Execute(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), Encoding.ASCII.GetBytes(json)));
        }

        [Test]
        public void should_not_return_error_when_system_time_is_close_to_server_time()
        {
            GivenServerTime(DateTime.UtcNow);

            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_error_when_system_time_is_more_than_one_day_from_server_time()
        {
            GivenServerTime(DateTime.UtcNow.AddDays(2));

            Subject.Check().ShouldBeError();
            ExceptionVerification.ExpectedErrors(1);
        }
    }
}
