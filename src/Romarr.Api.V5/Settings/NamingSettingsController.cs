using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Romarr.Common.Extensions;
using Romarr.Core.Organizer;
using Romarr.Http;
using Romarr.Http.REST;
using Romarr.Http.REST.Attributes;

namespace Romarr.Api.V5.Settings;

[V5ApiController("settings/naming")]
public class NamingSettingsController : RestController<NamingSettingsResource>
{
    private readonly INamingConfigService _namingConfigService;
    private readonly IFilenameSampleService _filenameSampleService;
    private readonly IFilenameValidationService _filenameValidationService;

    public NamingSettingsController(INamingConfigService namingConfigService,
                              IFilenameSampleService filenameSampleService,
                              IFilenameValidationService filenameValidationService)
    {
        _namingConfigService = namingConfigService;
        _filenameSampleService = filenameSampleService;
        _filenameValidationService = filenameValidationService;

        SharedValidator.RuleFor(c => c.MultiGameFileStyle).InclusiveBetween(0, 5);
        SharedValidator.RuleFor(c => c.StandardGameFileFormat).ValidGameFileFormat();
        SharedValidator.RuleFor(c => c.GameFolderFormat).ValidGameFolderFormat();
        SharedValidator.RuleFor(c => c.PlatformFolderFormat).ValidPlatformFolderFormat();
        SharedValidator.RuleFor(c => c.CustomColonReplacementFormat).ValidCustomColonReplacement().When(c => c.ColonReplacementFormat == (int)ColonReplacementFormat.Custom);
    }

    protected override NamingSettingsResource GetResourceById(int id)
    {
        return GetNamingConfig();
    }

    [HttpGet]
    public NamingSettingsResource GetNamingConfig()
    {
        var nameSpec = _namingConfigService.GetConfig();
        var resource = nameSpec.ToResource();

        return resource;
    }

    [RestPutById]
    public ActionResult<NamingSettingsResource> UpdateNamingConfig([FromBody] NamingSettingsResource resource)
    {
        var nameSpec = resource.ToModel();
        ValidateFormatResult(nameSpec);

        _namingConfigService.Save(nameSpec);

        return Accepted(resource.Id);
    }

    [HttpGet("examples")]
    public object GetExamples([FromQuery]NamingSettingsResource settings)
    {
        if (settings.Id == 0)
        {
            settings = GetNamingConfig();
        }

        var nameSpec = settings.ToModel();
        var sampleResource = new NamingExampleResource();

        var singleGameFileSampleResult = _filenameSampleService.GetStandardSample(nameSpec);
        var multiGameFileSampleResult = _filenameSampleService.GetMultiGameFileSample(nameSpec);

        sampleResource.SingleGameFileExample = _filenameValidationService.ValidateStandardFilename(singleGameFileSampleResult) != null
                ? null
                : singleGameFileSampleResult.FileName;

        sampleResource.MultiGameFileExample = _filenameValidationService.ValidateStandardFilename(multiGameFileSampleResult) != null
                ? null
                : multiGameFileSampleResult.FileName;

        sampleResource.GameFolderExample = nameSpec.GameFolderFormat.IsNullOrWhiteSpace()
            ? null
            : _filenameSampleService.GetGameFolderSample(nameSpec);

        sampleResource.PlatformFolderExample = nameSpec.PlatformFolderFormat.IsNullOrWhiteSpace()
            ? null
            : _filenameSampleService.GetPlatformFolderSample(nameSpec);

        return sampleResource;
    }

    private void ValidateFormatResult(NamingConfig nameSpec)
    {
        var singleGameFileSampleResult = _filenameSampleService.GetStandardSample(nameSpec);
        var multiGameFileSampleResult = _filenameSampleService.GetMultiGameFileSample(nameSpec);

        var singleGameFileValidationResult = _filenameValidationService.ValidateStandardFilename(singleGameFileSampleResult);
        var multiGameFileValidationResult = _filenameValidationService.ValidateStandardFilename(multiGameFileSampleResult);

        var validationFailures = new List<ValidationFailure>();

        validationFailures.AddIfNotNull(singleGameFileValidationResult);
        validationFailures.AddIfNotNull(multiGameFileValidationResult);

        if (validationFailures.Any())
        {
            throw new ValidationException(validationFailures.DistinctBy(v => v.PropertyName).ToArray());
        }
    }
}
