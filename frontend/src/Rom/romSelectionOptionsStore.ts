import { createOptionsStore } from 'Helpers/Hooks/useOptionsStore';
import { SortDirection } from 'Helpers/Props/sortDirections';

interface FileSelectOptions {
  sortKey: string;
  sortDirection: SortDirection;
}

const { useOptions, useOption, setOptions, setOption, setSort } =
  createOptionsStore<FileSelectOptions>('file_selection_options', () => {
    return {
      sortKey: 'romNumber',
      sortDirection: 'ascending',
    };
  });

export const useRomSelectionOptions = useOptions;
export const setFileSelectionOptions = setOptions;
export const useRomSelectionOption = useOption;
export const setFileSelectionOption = setOption;
export const setFileSelectionSort = setSort;
