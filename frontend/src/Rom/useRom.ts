import { QueryKey, useQueryClient } from '@tanstack/react-query';
import { create } from 'zustand';
import useApiMutation from 'Helpers/Hooks/useApiMutation';
import { PagedQueryResponse } from 'Helpers/Hooks/usePagedApiQuery';
import Rom from './Rom';

export type RomEntity =
  | 'calendar'
  | 'roms'
  | 'interactiveImport.roms'
  | 'wanted.cutoffUnmet'
  | 'wanted.missing';

interface FileQueryKeyStore {
  calendar: QueryKey | null;
  roms: QueryKey | null;
  cutoffUnmet: QueryKey | null;
  missing: QueryKey | null;
}

const fileQueryKeyStore = create<FileQueryKeyStore>(() => ({
  calendar: null,
  roms: null,
  cutoffUnmet: null,
  missing: null,
}));

export const getQueryKey = (romEntity: RomEntity) => {
  switch (romEntity) {
    case 'calendar':
      return fileQueryKeyStore.getState().calendar;
    case 'roms':
      return fileQueryKeyStore.getState().roms;
    case 'wanted.cutoffUnmet':
      return fileQueryKeyStore.getState().cutoffUnmet;
    case 'wanted.missing':
      return fileQueryKeyStore.getState().missing;
    default:
      return null;
  }
};

export const setRomQueryKey = (
  romEntity: RomEntity,
  queryKey: QueryKey | null
) => {
  switch (romEntity) {
    case 'calendar':
      fileQueryKeyStore.setState({ calendar: queryKey });
      break;
    case 'roms':
      fileQueryKeyStore.setState({ roms: queryKey });
      break;
    case 'wanted.cutoffUnmet':
      fileQueryKeyStore.setState({ cutoffUnmet: queryKey });
      break;
    case 'wanted.missing':
      fileQueryKeyStore.setState({ missing: queryKey });
      break;
    default:
      break;
  }
};

const useRom = (romId: number | undefined, romEntity: RomEntity) => {
  const queryClient = useQueryClient();
  const queryKey = getQueryKey(romEntity);

  if (romEntity === 'calendar') {
    return queryKey
      ? queryClient
          .getQueryData<Rom[]>(queryKey)
          ?.find((e) => e.id === romId)
      : undefined;
  }

  if (romEntity === 'roms') {
    return queryKey
      ? queryClient.getQueryData<Rom[]>(queryKey)?.find((e) => e.id === romId)
      : undefined;
  }

  if (romEntity === 'wanted.cutoffUnmet' || romEntity === 'wanted.missing') {
    return queryKey
      ? queryClient
          .getQueryData<PagedQueryResponse<Rom>>(queryKey)
          ?.records?.find((e) => e.id === romId)
      : undefined;
  }

  return undefined;
};

export default useRom;

interface ToggleFilesMonitored {
  romIds: number[];
  monitored: boolean;
}

export const useToggleFilesMonitored = (queryKey: QueryKey) => {
  const queryClient = useQueryClient();

  const { mutate, isPending, variables } = useApiMutation<
    unknown,
    ToggleFilesMonitored
  >({
    path: '/rom/monitor',
    method: 'PUT',
    mutationOptions: {
      onSuccess: (_data, variables) => {
        queryClient.setQueryData<Rom[] | undefined>(queryKey, (oldFiles) => {
          if (!oldFiles) {
            return oldFiles;
          }

          return oldFiles.map((oldFile) => {
            if (variables.romIds.includes(oldFile.id)) {
              return {
                ...oldFile,
                monitored: variables.monitored,
              };
            }

            return oldFile;
          });
        });
      },
    },
  });

  return {
    toggleFilesMonitored: mutate,
    isToggling: isPending,
    togglingRomIds: variables?.romIds ?? [],
    togglingMonitored: variables?.monitored,
  };
};

const DEFAULT_FILES: Rom[] = [];

export const useRomsWithIds = (romIds: number[]) => {
  const queryClient = useQueryClient();
  const queryKey = getQueryKey('roms');

  return queryKey
    ? queryClient
        .getQueryData<Rom[]>(queryKey)
        ?.filter((e) => romIds.includes(e.id)) ?? DEFAULT_FILES
    : DEFAULT_FILES;
};
