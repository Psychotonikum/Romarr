using System.Collections.Generic;
using System.IO;
using Romarr.Core.CustomFormats;
using Romarr.Core.Organizer;
using Romarr.Core.Parser.Model;

namespace Romarr.Core.MediaFiles.GameFileImport;

public interface ILocalGameFileCustomFormatCalculationService
{
    public List<CustomFormat> ParseGameFileCustomFormats(LocalGameFile localRom);
    public void UpdateGameFileCustomFormats(LocalGameFile localRom);
}

public class LocalGameFileCustomFormatCalculationService : ILocalGameFileCustomFormatCalculationService
{
    private readonly IBuildFileNames _fileNameBuilder;
    private readonly ICustomFormatCalculationService _formatCalculator;

    public LocalGameFileCustomFormatCalculationService(IBuildFileNames fileNameBuilder, ICustomFormatCalculationService formatCalculator)
    {
        _fileNameBuilder = fileNameBuilder;
        _formatCalculator = formatCalculator;
    }

    public List<CustomFormat> ParseGameFileCustomFormats(LocalGameFile localRom)
    {
        var fileNameUsedForCustomFormatCalculation = _fileNameBuilder.BuildFileName(localRom.Roms, localRom.Game, localRom.ToRomFile());
        return _formatCalculator.ParseCustomFormat(localRom, fileNameUsedForCustomFormatCalculation);
    }

    public void UpdateGameFileCustomFormats(LocalGameFile localRom)
    {
        var fileNameUsedForCustomFormatCalculation = _fileNameBuilder.BuildFileName(localRom.Roms, localRom.Game, localRom.ToRomFile());
        localRom.CustomFormats = _formatCalculator.ParseCustomFormat(localRom, fileNameUsedForCustomFormatCalculation);
        localRom.FileNameUsedForCustomFormatCalculation = fileNameUsedForCustomFormatCalculation;
        localRom.CustomFormatScore = localRom.Game.QualityProfile?.Value.CalculateCustomFormatScore(localRom.CustomFormats) ?? 0;

        localRom.OriginalFileNameCustomFormats = _formatCalculator.ParseCustomFormat(localRom, Path.GetFileName(localRom.Path));
        localRom.OriginalFileNameCustomFormatScore = localRom.Game.QualityProfile?.Value.CalculateCustomFormatScore(localRom.OriginalFileNameCustomFormats) ?? 0;
    }
}
