import { useEffect, useMemo } from 'react';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import clientSideFilterAndSort from 'Utilities/Filter/clientSideFilterAndSort';
import Rom from './Rom';
import { useRomOptions } from './romOptionsStore';
import { setRomQueryKey } from './useRom';

const DEFAULT_FILES: Rom[] = [];

interface SeriesFiles {
  gameId: number;
}

interface SeasonFiles {
  gameId: number | undefined;
  platformNumber: number | undefined;
  isSelection: boolean;
}

interface RomIds {
  romIds: number[];
}

interface RomFileId {
  romFileId: number;
}

export type FileFilter =
  | SeriesFiles
  | SeasonFiles
  | RomIds
  | RomFileId;

const useRoms = (params: FileFilter) => {
  const setQueryKey = !('isSelection' in params);

  const { isPlaceholderData, queryKey, ...result } = useApiQuery<Rom[]>({
    path: '/rom',
    queryParams:
      'isSelection' in params
        ? {
            gameId: params.gameId,
            platformNumber: params.platformNumber,
          }
        : { ...params },
    queryOptions: {
      enabled:
        ('gameId' in params && params.gameId !== undefined) ||
        ('romIds' in params && params.romIds?.length > 0) ||
        ('romFileId' in params && params.romFileId !== undefined),
    },
  });

  useEffect(() => {
    if (setQueryKey && !isPlaceholderData) {
      setRomQueryKey('roms', queryKey);
    }
  }, [setQueryKey, isPlaceholderData, queryKey]);

  return {
    ...result,
    queryKey,
    data: result.data ?? DEFAULT_FILES,
  };
};

export default useRoms;

export const usePlatformFiles = (gameId: number, platformNumber: number) => {
  const { data, ...result } = useRoms({ gameId });
  const { sortKey, sortDirection } = useRomOptions();

  const seasonFiles = useMemo(() => {
    const { data: seasonFiles } = clientSideFilterAndSort(
      data.filter((rom) => rom.platformNumber === platformNumber),
      {
        sortKey,
        sortDirection,
      }
    );

    return seasonFiles;
  }, [data, platformNumber, sortKey, sortDirection]);

  return {
    ...result,
    data: seasonFiles,
  };
};
