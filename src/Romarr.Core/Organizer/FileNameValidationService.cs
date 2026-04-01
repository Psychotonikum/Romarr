using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentValidation.Results;
using Romarr.Core.Parser.Model;
using Romarr.Core.Games;

namespace Romarr.Core.Organizer
{
    public interface IFilenameValidationService
    {
        ValidationFailure ValidateStandardFilename(SampleResult sampleResult);
    }

    public class FileNameValidationService : IFilenameValidationService
    {
        private const string ERROR_MESSAGE = "Produces invalid file names";

        public ValidationFailure ValidateStandardFilename(SampleResult sampleResult)
        {
            var validationFailure = new ValidationFailure("StandardGameFileFormat", ERROR_MESSAGE);
            var parsedRomInfo = sampleResult.FileName.Contains(Path.DirectorySeparatorChar)
                ? Parser.Parser.ParsePath(sampleResult.FileName)
                : Parser.Parser.ParseTitle(sampleResult.FileName);

            if (parsedRomInfo == null)
            {
                return validationFailure;
            }

            if (!ValidatePlatformAndRomNumbers(sampleResult.Roms, parsedRomInfo))
            {
                return validationFailure;
            }

            return null;
        }

        private bool ValidatePlatformAndRomNumbers(List<Rom> roms, ParsedRomInfo parsedRomInfo)
        {
            if (parsedRomInfo.PlatformNumber != roms.First().PlatformNumber ||
                !parsedRomInfo.RomNumbers.OrderBy(e => e).SequenceEqual(roms.Select(e => e.FileNumber).OrderBy(e => e)))
            {
                return false;
            }

            return true;
        }
    }
}
