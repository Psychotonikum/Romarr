import React, { useCallback, useMemo, useState } from 'react';
import { SelectProvider, useSelect } from 'App/Select/SelectContext';
import TextInput from 'Components/Form/TextInput';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Scroller from 'Components/Scroller/Scroller';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import { kinds, scrollDirections } from 'Helpers/Props';
import { SortDirection } from 'Helpers/Props/sortDirections';
import Rom from 'Rom/Rom';
import {
  setFileSelectionSort,
  useRomSelectionOptions,
} from 'Rom/romSelectionOptionsStore';
import useRoms from 'Rom/useRoms';
import { CheckInputChanged, InputChanged } from 'typings/inputs';
import clientSideFilterAndSort from 'Utilities/Filter/clientSideFilterAndSort';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import translate from 'Utilities/String/translate';
import SelectRomRow from './SelectRomRow';
import styles from './SelectRomModalContent.css';

const columns = [
  {
    name: 'romNumber',
    label: '#',
    isSortable: true,
    isVisible: true,
  },
  {
    name: 'title',
    label: () => translate('Title'),
    isVisible: true,
  },
  {
    name: 'airDate',
    label: () => translate('AirDate'),
    isVisible: true,
  },
];

export interface SelectedFile {
  id: number;
  roms: Rom[];
}

interface SelectRomModalContentProps {
  selectedIds: number[] | string[];
  gameId?: number;
  platformNumber?: number;
  selectedDetails?: string;
  modalTitle: string;
  onFilesSelect(selectedFiles: SelectedFile[]): unknown;
  onModalClose(): unknown;
}

function SelectRomModalContentInner(props: SelectRomModalContentProps) {
  const {
    selectedIds,
    gameId,
    platformNumber,
    selectedDetails,
    modalTitle,
    onFilesSelect,
    onModalClose,
  } = props;

  const [filter, setFilter] = useState('');

  const { isFetching, isFetched, data, error } = useRoms({
    gameId,
    platformNumber,
    isSelection: true,
  });

  const { sortKey, sortDirection } = useRomSelectionOptions();

  const {
    allSelected,
    allUnselected,
    selectedCount: selectedFilesCount,
    getSelectedIds,
    selectAll,
    unselectAll,
  } = useSelect<Rom>();

  const filterRomNumber = parseInt(filter);
  const errorMessage = getErrorMessage(error, translate('EpisodesLoadError'));
  const selectedCount = selectedIds.length;
  const selectionIsValid =
    selectedFilesCount > 0 && selectedFilesCount % selectedCount === 0;

  const onFilterChange = useCallback(
    ({ value }: InputChanged<string>) => {
      setFilter(value.toLowerCase());
    },
    [setFilter]
  );

  const onSelectAllChange = useCallback(
    ({ value }: CheckInputChanged) => {
      if (value) {
        selectAll();
      } else {
        unselectAll();
      }
    },
    [selectAll, unselectAll]
  );

  const onSortPress = useCallback(
    (newSortKey: string, newSortDirection?: SortDirection) => {
      setFileSelectionSort({
        sortKey: newSortKey,
        sortDirection: newSortDirection,
      });
    },
    []
  );

  const onFilesSelectWrapper = useCallback(() => {
    const romIds: number[] = getSelectedIds();

    const selectedFiles = data.reduce((acc: Rom[], item) => {
      if (romIds.indexOf(item.id) > -1) {
        acc.push(item);
      }

      return acc;
    }, []);

    const filesPerImport = selectedFiles.length / selectedIds.length;
    const sortedFiles = selectedFiles.sort((a, b) => {
      return a.platformNumber - b.platformNumber;
    });

    const mappedFiles = selectedIds.map((id, index): SelectedFile => {
      const startingIndex = index * filesPerImport;
      const roms = sortedFiles.slice(
        startingIndex,
        startingIndex + filesPerImport
      );

      return {
        id: id as number,
        roms,
      };
    });

    onFilesSelect(mappedFiles);
  }, [selectedIds, data, getSelectedIds, onFilesSelect]);

  let details = selectedDetails;

  if (!details) {
    details =
      selectedCount > 1
        ? translate('CountSelectedFiles', { selectedCount })
        : translate('CountSelectedFile', { selectedCount });
  }

  const { data: items } = useMemo(() => {
    return clientSideFilterAndSort<Rom>(data, {
      sortKey,
      sortDirection,
    });
  }, [data, sortKey, sortDirection]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {translate('SelectEpisodesModalTitle', { modalTitle })}
      </ModalHeader>

      <ModalBody
        className={styles.modalBody}
        scrollDirection={scrollDirections.NONE}
      >
        <TextInput
          className={styles.filterInput}
          placeholder={translate('FilterEpisodesPlaceholder')}
          name="filter"
          value={filter}
          autoFocus={true}
          onChange={onFilterChange}
        />

        <Scroller className={styles.scroller} autoFocus={false}>
          {isFetching ? <LoadingIndicator /> : null}

          {error ? <div>{errorMessage}</div> : null}

          {isFetched && !!items.length ? (
            <Table
              columns={columns}
              selectAll={true}
              allSelected={allSelected}
              allUnselected={allUnselected}
              sortKey={sortKey}
              sortDirection={sortDirection}
              onSortPress={onSortPress}
              onSelectAllChange={onSelectAllChange}
            >
              <TableBody>
                {items.map((item) => {
                  return item.title.toLowerCase().includes(filter) ||
                    item.romNumber === filterRomNumber ? (
                    <SelectRomRow
                      key={item.id}
                      id={item.id}
                      romNumber={item.romNumber}
                      absoluteRomNumber={item.absoluteRomNumber}
                      title={item.title}
                      airDate={item.airDate}
                      unverifiedSceneNumbering={item.unverifiedSceneNumbering}
                    />
                  ) : null;
                })}
              </TableBody>
            </Table>
          ) : null}

          {isFetched && !data.length
            ? translate('NoEpisodesFoundForSelectedSeason')
            : null}
        </Scroller>
      </ModalBody>

      <ModalFooter className={styles.footer}>
        <div className={styles.details}>{details}</div>

        <div className={styles.buttons}>
          <Button onPress={onModalClose}>{translate('Cancel')}</Button>

          <Button
            kind={kinds.SUCCESS}
            isDisabled={!selectionIsValid}
            onPress={onFilesSelectWrapper}
          >
            {translate('SelectFiles')}
          </Button>
        </div>
      </ModalFooter>
    </ModalContent>
  );
}

function SelectRomModalContent(props: SelectRomModalContentProps) {
  const { data } = useRoms({
    gameId: props.gameId,
    platformNumber: props.platformNumber,
    isSelection: true,
  });

  return (
    <SelectProvider items={data}>
      <SelectRomModalContentInner {...props} />
    </SelectProvider>
  );
}

export default SelectRomModalContent;
