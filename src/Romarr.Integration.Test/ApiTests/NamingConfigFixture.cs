using FluentAssertions;
using NUnit.Framework;

namespace Romarr.Integration.Test.ApiTests
{
    [TestFixture]
    public class NamingConfigFixture : IntegrationTest
    {
        [Test]
        public void should_be_able_to_get()
        {
            NamingConfig.GetSingle().Should().NotBeNull();
        }

        [Test]
        public void should_be_able_to_get_by_id()
        {
            var config = NamingConfig.GetSingle();
            NamingConfig.Get(config.Id).Should().NotBeNull();
            NamingConfig.Get(config.Id).Id.Should().Be(config.Id);
        }

        [Test]
        public void should_be_able_to_update()
        {
            var config = NamingConfig.GetSingle();
            config.RenameGameFiles = false;
            config.StandardGameFileFormat = "{Game Title} - S{platform:00}E{rom:00} - {Rom Title}";

            var result = NamingConfig.Put(config);
            result.RenameGameFiles.Should().BeFalse();
            result.StandardGameFileFormat.Should().Be(config.StandardGameFileFormat);
        }

        [Test]
        public void should_get_bad_request_if_standard_format_is_empty()
        {
            var config = NamingConfig.GetSingle();
            config.RenameGameFiles = true;
            config.StandardGameFileFormat = "";

            var errors = NamingConfig.InvalidPut(config);
            errors.Should().NotBeNull();
        }

        [Test]
        public void should_get_bad_request_if_standard_format_doesnt_contain_platform_and_gameFile()
        {
            var config = NamingConfig.GetSingle();
            config.RenameGameFiles = true;
            config.StandardGameFileFormat = "{platform}";

            var errors = NamingConfig.InvalidPut(config);
            errors.Should().NotBeNull();
        }

        [Test]
        public void should_not_require_format_when_rename_gameFiles_is_false()
        {
            var config = NamingConfig.GetSingle();
            config.RenameGameFiles = false;
            config.StandardGameFileFormat = "";

            var errors = NamingConfig.InvalidPut(config);
            errors.Should().NotBeNull();
        }

        [Test]
        public void should_require_format_when_rename_gameFiles_is_true()
        {
            var config = NamingConfig.GetSingle();
            config.RenameGameFiles = true;
            config.StandardGameFileFormat = "";

            var errors = NamingConfig.InvalidPut(config);
            errors.Should().NotBeNull();
        }

        [Test]
        public void should_get_bad_request_if_series_folder_format_does_not_contain_series_title()
        {
            var config = NamingConfig.GetSingle();
            config.RenameGameFiles = true;
            config.GameFolderFormat = "This and That";

            var errors = NamingConfig.InvalidPut(config);
            errors.Should().NotBeNull();
        }
    }
}
