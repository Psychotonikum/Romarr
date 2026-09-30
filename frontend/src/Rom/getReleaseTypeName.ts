import ReleaseType from 'InteractiveImport/ReleaseType';
import translate from 'Utilities/String/translate';

export default function getReleaseTypeName(
  releaseType?: ReleaseType
): string | null {
  switch (releaseType) {
    case 'singleGameFile':
      return translate('SingleEpisode');
    case 'multiGameFile':
      return translate('MultiEpisode');
    case 'platformPack':
      return translate('PlatformPack');
    default:
      return translate('Unknown');
  }
}
