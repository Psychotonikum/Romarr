using System;
using System.Collections.Generic;
using System.IO;
using FluentValidation.Results;
using Romarr.Core.Extras.Metadata.Files;
using Romarr.Core.MediaFiles;
using Romarr.Core.ThingiProvider;
using Romarr.Core.Games;

namespace Romarr.Core.Extras.Metadata
{
    public abstract class MetadataBase<TSettings> : IMetadata
        where TSettings : IProviderConfig, new()
    {
        public abstract string Name { get; }

        public Type ConfigContract => typeof(TSettings);

        public virtual ProviderMessage Message => null;

        public IEnumerable<ProviderDefinition> DefaultDefinitions => new List<ProviderDefinition>();

        public ProviderDefinition Definition { get; set; }

        public ValidationResult Test()
        {
            return new ValidationResult();
        }

        public virtual string GetFilenameAfterMove(Game game, RomFile romFile, MetadataFile metadataFile)
        {
            var existingFilename = Path.Combine(game.Path, metadataFile.RelativePath);
            var extension = Path.GetExtension(existingFilename).TrimStart('.');
            var newFileName = Path.ChangeExtension(Path.Combine(game.Path, romFile.RelativePath), extension);

            return newFileName;
        }

        public abstract MetadataFile FindMetadataFile(Game game, string path);

        public abstract MetadataFileResult SeriesMetadata(Game game, SeriesMetadataReason reason);
        public abstract MetadataFileResult GameFileMetadata(Game game, RomFile romFile);
        public abstract List<ImageFileResult> GameImages(Game game);
        public abstract List<ImageFileResult> PlatformImages(Game game, Platform platform);
        public abstract List<ImageFileResult> GameFileImages(Game game, RomFile romFile);

        public virtual object RequestAction(string action, IDictionary<string, string> query)
        {
            return null;
        }

        protected TSettings Settings => (TSettings)Definition.Settings;

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}
