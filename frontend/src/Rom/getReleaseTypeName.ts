import ReleaseType from 'InteractiveImport/ReleaseType';
import translate from 'Utilities/String/translate';

export default function getReleaseTypeName(
  releaseType?: ReleaseType
): string | null {
  switch (releaseType) {
    case 'singleFile':
      return translate('SingleEpisode');
    case 'multiFile':
      return translate('MultiEpisode');
    case 'platformPack':
      return translate('PlatformPack');
    default:
      return translate('Unknown');
  }
}
