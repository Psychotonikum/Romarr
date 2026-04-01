using System;
using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Common.EnvironmentInfo;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;
using Romarr.Core.Validation.Paths;
using Romarr.Test.Common;

namespace Romarr.Core.Test.ValidationTests
{
    public class SystemFolderValidatorFixture : CoreTest<SystemFolderValidator>
    {
        private TestValidator<Game> _validator;

        [SetUp]
        public void Setup()
        {
            _validator = new TestValidator<Game>
                            {
                                v => v.RuleFor(s => s.Path).SetValidator(Subject)
                            };
        }

        [Test]
        public void should_not_be_valid_if_set_to_windows_folder()
        {
            WindowsOnly();

            var game = Builder<Game>.CreateNew()
                                        .With(s => s.Path = Environment.GetFolderPath(Environment.SpecialFolder.Windows))
                                        .Build();

            _validator.Validate(game).IsValid.Should().BeFalse();
        }

        [Test]
        public void should_not_be_valid_if_child_of_windows_folder()
        {
            WindowsOnly();

            var game = Builder<Game>.CreateNew()
                                        .With(s => s.Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Test"))
                                        .Build();

            _validator.Validate(game).IsValid.Should().BeFalse();
        }

        [Test]
        public void should_not_be_valid_if_set_to_bin_folder()
        {
            PosixOnly();

            var bin = OsInfo.IsOsx ? "/System" : "/bin";
            var game = Builder<Game>.CreateNew()
                                        .With(s => s.Path = bin)
                                        .Build();

            _validator.Validate(game).IsValid.Should().BeFalse();
        }

        [Test]
        public void should_not_be_valid_if_child_of_bin_folder()
        {
            PosixOnly();

            var bin = OsInfo.IsOsx ? "/System" : "/bin";
            var game = Builder<Game>.CreateNew()
                                        .With(s => s.Path = Path.Combine(bin, "test"))
                                        .Build();

            _validator.Validate(game).IsValid.Should().BeFalse();
        }
    }
}
