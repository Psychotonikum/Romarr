using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using Romarr.Core.Download;
using Romarr.Core.History;
using Romarr.Core.Indexers;
using Romarr.Core.MediaFiles;
using Romarr.Core.MediaFiles.Events;
using Romarr.Core.Parser.Model;
using Romarr.Core.Profiles.Qualities;
using Romarr.Core.Qualities;
using Romarr.Core.Test.Framework;
using Romarr.Core.Test.Qualities;
using Romarr.Core.Games;

namespace Romarr.Core.Test.HistoryTests
{
    public class HistoryServiceFixture : CoreTest<HistoryService>
    {
        private QualityProfile _profile;
        private QualityProfile _profileCustom;

        [SetUp]
        public void Setup()
        {
            _profile = new QualityProfile
                {
                    Cutoff = Quality.WEBDL720p.Id,
                    Items = QualityFixture.GetDefaultQualities(),
                };

            _profileCustom = new QualityProfile
                {
                    Cutoff = Quality.WEBDL720p.Id,
                    Items = QualityFixture.GetDefaultQualities(Quality.DVD),
                };
        }

        [Test]
        public void should_use_file_name_for_source_title_if_scene_name_is_null()
        {
            var game = Builder<Game>.CreateNew().Build();
            var roms = Builder<Rom>.CreateListOfSize(1).Build().ToList();
            var romFile = Builder<RomFile>.CreateNew()
                                                  .With(f => f.SceneName = null)
                                                  .Build();

            var localRom = new LocalGameFile
                               {
                                   Game = game,
                                   Roms = roms,
                                   Path = @"C:\Test\Unsorted\Game.s01e01.mkv"
                               };

            var downloadClientItem = new DownloadClientItem
                                     {
                                         DownloadClientInfo = new DownloadClientItemClientInfo
                                         {
                                             Protocol = DownloadProtocol.Usenet,
                                             Id = 1,
                                             Name = "sab"
                                         },
                                         DownloadId = "abcd"
                                     };

            Subject.Handle(new FileImportedEvent(localRom, romFile, new List<DeletedRomFile>(), true, downloadClientItem));

            Mocker.GetMock<IHistoryRepository>()
                .Verify(v => v.Insert(It.Is<FileHistory>(h => h.SourceTitle == Path.GetFileNameWithoutExtension(localRom.Path))));
        }
    }
}
