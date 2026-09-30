using System.IO;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Common.EnvironmentInfo;
using Romarr.Core.MetadataSource.RomDatabase;
using Romarr.Core.Test.Framework;

namespace Romarr.Core.Test.MetadataSource
{
    [TestFixture]
    public class RomDatabaseServiceFixture : CoreTest<RomDatabaseService>
    {
        [SetUp]
        public void Setup()
        {
            Directory.CreateDirectory(Path.Combine(TempFolder, "rom_databases"));
            Mocker.GetMock<IAppFolderInfo>().SetupGet(info => info.AppDataFolder).Returns(TempFolder);
        }

        [Test]
        public void imports_dat_and_preserves_it_when_replacement_is_invalid()
        {
            using (var valid = new MemoryStream(Encoding.UTF8.GetBytes("<!DOCTYPE datafile SYSTEM 'http://example.com/dat.dtd'><datafile><game name='Test (USA)'><rom sha1='1234' /></game></datafile>")))
            {
                Subject.ImportDatabase("nointro-nes", valid);
            }

            Subject.GetSystemStatus("nointro-nes").EntryCount.Should().Be(1);
            Subject.LookupByHash("nointro-nes", "1234").Name.Should().Be("Test (USA)");

            using var invalid = new MemoryStream(Encoding.UTF8.GetBytes("<datafile><game>"));
            Assert.Throws<System.Xml.XmlException>(() => Subject.ImportDatabase("nointro-nes", invalid));
            Subject.GetSystemStatus("nointro-nes").EntryCount.Should().Be(1);
        }
    }
}
