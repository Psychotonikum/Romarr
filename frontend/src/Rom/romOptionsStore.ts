import { createElement } from 'react';
import Icon from 'Components/Icon';
import Column from 'Components/Table/Column';
import { createOptionsStore } from 'Helpers/Hooks/useOptionsStore';
import { icons } from 'Helpers/Props';
import { SortDirection } from 'Helpers/Props/sortDirections';
import translate from 'Utilities/String/translate';

interface FileSelectOptions {
  sortKey: string;
  sortDirection: SortDirection;
  columns: Column[];
}

const { useOptions, useOption, setOptions, setOption, setSort } =
  createOptionsStore<FileSelectOptions>('file_options', () => {
    return {
      sortKey: 'romNumber',
      sortDirection: 'descending',
      columns: [
        {
          name: 'monitored',
          label: '',
          columnLabel: () => translate('Monitored'),
          isVisible: true,
          isModifiable: false,
        },
        {
          name: 'romNumber',
          label: '#',
          isVisible: true,
          isSortable: true,
        },
        {
          name: 'romType',
          label: () => translate('Type'),
          isVisible: true,
          isSortable: true,
        },
        {
          name: 'region',
          label: () => translate('Region'),
          isVisible: false,
        },
        {
          name: 'romReleaseType',
          label: () => translate('ReleaseType'),
          isVisible: false,
        },
        {
          name: 'modification',
          label: () => translate('Modification'),
          isVisible: false,
        },
        {
          name: 'title',
          label: () => translate('Title'),
          isVisible: true,
          isSortable: true,
        },
        {
          name: 'path',
          label: () => translate('Path'),
          isVisible: false,
          isSortable: true,
        },
        {
          name: 'relativePath',
          label: () => translate('RelativePath'),
          isVisible: false,
          isSortable: true,
        },
        {
          name: 'airDateUtc',
          label: () => translate('ReleaseDate'),
          isVisible: true,
          isSortable: true,
        },
        {
          name: 'languages',
          label: () => translate('Languages'),
          isVisible: false,
        },
        {
          name: 'size',
          label: () => translate('Size'),
          isVisible: false,
          isSortable: true,
        },
        {
          name: 'releaseGroup',
          label: () => translate('ReleaseGroup'),
          isVisible: false,
        },
        {
          name: 'customFormats',
          label: () => translate('Formats'),
          isVisible: false,
        },
        {
          name: 'customFormatScore',
          columnLabel: () => translate('CustomFormatScore'),
          label: createElement(Icon, {
            name: icons.SCORE,
            title: () => translate('CustomFormatScore'),
          }),
          isVisible: false,
          isSortable: true,
        },
        {
          name: 'indexerFlags',
          columnLabel: () => translate('IndexerFlags'),
          label: createElement(Icon, {
            name: icons.FLAG,
            title: () => translate('IndexerFlags'),
          }),
          isVisible: false,
        },
        {
          name: 'status',
          label: () => translate('Status'),
          isVisible: true,
        },
        {
          name: 'actions',
          label: '',
          columnLabel: () => translate('Actions'),
          isVisible: true,
          isModifiable: false,
        },
      ],
    };
  });

export const useRomOptions = useOptions;
export const setFileOptions = setOptions;
export const useRomOption = useOption;
export const setFileOption = setOption;
export const setFileSort = setSort;
