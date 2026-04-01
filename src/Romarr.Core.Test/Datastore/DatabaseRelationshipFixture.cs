using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Romarr.Core.History;
using Romarr.Core.Languages;
using Romarr.Core.MediaFiles;
using Romarr.Core.Qualities;
using Romarr.Core.Test.Framework;
using Romarr.Core.Games;

namespace Romarr.Core.Test.Datastore
{
    [TestFixture]
    public class DatabaseRelationshipFixture : DbTest
    {
        [SetUp]
        public void Setup()
        {
            AssertionOptions.AssertEquivalencyUsing(options =>
            {
                options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation.ToUniversalTime(), TimeSpan.FromMilliseconds(20))).WhenTypeIs<DateTime>();
                options.Using<DateTime?>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation.Value.ToUniversalTime(), TimeSpan.FromMilliseconds(20))).WhenTypeIs<DateTime?>();
                return options;
            });
        }

        [Test]
        public void one_to_one()
        {
            var romFile = Builder<RomFile>.CreateNew()
                .With(c => c.Languages = new List<Language> { Language.English })
                .With(c => c.Quality = new QualityModel())
                .BuildNew();

            Db.Insert(romFile);

            var rom = Builder<Rom>.CreateNew()
                                          .With(c => c.RomFileId = romFile.Id)
                                          .BuildNew();

            Db.Insert(rom);

            var loadedRomFile = Db.Single<Rom>().RomFile.Value;

            loadedRomFile.Should().NotBeNull();
            loadedRomFile.Should().BeEquivalentTo(romFile,
                options => options
                    .IncludingAllRuntimeProperties()
                    .Excluding(c => c.DateAdded)
                    .Excluding(c => c.Path)
                    .Excluding(c => c.Game)
                    .Excluding(c => c.Roms));
        }

        [Test]
        public void one_to_one_should_not_query_db_if_foreign_key_is_zero()
        {
            var rom = Builder<Rom>.CreateNew()
                                          .With(c => c.RomFileId = 0)
                                          .BuildNew();

            Db.Insert(rom);

            Db.Single<Rom>().RomFile.Value.Should().BeNull();
        }

        [Test]
        public void embedded_document_as_json()
        {
            var quality = new QualityModel { Quality = Quality.Bluray720p, Revision = new Revision(version: 2) };

            var history = Builder<FileHistory>.CreateNew()
                .With(c => c.Languages = new List<Language> { Language.English })
                .With(c => c.Id = 0)
                .With(c => c.Quality = quality)
                .Build();

            Db.Insert(history);

            var loadedQuality = Db.Single<FileHistory>().Quality;
            loadedQuality.Should().Be(quality);
        }

        [Test]
        public void embedded_list_of_document_with_json()
        {
            var history = Builder<FileHistory>.CreateListOfSize(2)
                .All()
                .With(c => c.Id = 0)
                .With(c => c.Languages = new List<Language> { Language.English })
                .Build().ToList();

            history[0].Quality = new QualityModel(Quality.HDTV1080p, new Revision(version: 2));
            history[1].Quality = new QualityModel(Quality.Bluray720p, new Revision(version: 2));

            Db.InsertMany(history);

            var returnedHistory = Db.All<FileHistory>();

            returnedHistory[0].Quality.Quality.Should().Be(Quality.HDTV1080p);
        }
    }
}
