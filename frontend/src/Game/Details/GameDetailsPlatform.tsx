import React, { useCallback, useEffect, useRef, useState } from 'react';
import { useAppDimension } from 'App/appStore';
import CommandNames from 'Commands/CommandNames';
import { useCommands, useExecuteCommand } from 'Commands/useCommands';
import Icon from 'Components/Icon';
import Label from 'Components/Label';
import IconButton from 'Components/Link/IconButton';
import Link from 'Components/Link/Link';
import SpinnerIconButton from 'Components/Link/SpinnerIconButton';
import Menu from 'Components/Menu/Menu';
import MenuButton from 'Components/Menu/MenuButton';
import MenuContent from 'Components/Menu/MenuContent';
import MenuItem from 'Components/Menu/MenuItem';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import SpinnerIcon from 'Components/SpinnerIcon';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import Popover from 'Components/Tooltip/Popover';
import { Statistics } from 'Game/Game';
import GameHistoryModal from 'Game/History/GameHistoryModal';
import PlatformInteractiveSearchModal from 'Game/Search/PlatformInteractiveSearchModal';
import { useSingleGame, useToggleSeasonMonitored } from 'Game/useGame';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { align, icons, sortDirections, tooltipPositions } from 'Helpers/Props';
import { SortDirection } from 'Helpers/Props/sortDirections';
import InteractiveImportModal from 'InteractiveImport/InteractiveImportModal';
import OrganizePreviewModal from 'Organize/OrganizePreviewModal';
import Rom from 'Rom/Rom';
import {
  setFileOptions,
  setFileSort,
  useRomOptions,
} from 'Rom/romOptionsStore';
import { getQueryKey, useToggleFilesMonitored } from 'Rom/useRom';
import { usePlatformFiles } from 'Rom/useRoms';
import { TableOptionsChangePayload } from 'typings/Table';
import { findCommand, isCommandExecuting } from 'Utilities/Command';
import isAfter from 'Utilities/Date/isAfter';
import isBefore from 'Utilities/Date/isBefore';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import getToggledRange from 'Utilities/Table/getToggledRange';
import PlatformInfo from './PlatformInfo';
import PlatformProgressLabel from './PlatformProgressLabel';
import RomRow from './RomRow';
import styles from './GameDetailsPlatform.css';

function getPlatformStatistics(roms: Rom[]) {
  let fileCount = 0;
  let downloadedFileCount = 0;
  let totalFileCount = 0;
  let monitoredFileCount = 0;
  let hasMonitoredRoms = false;
  const sizeOnDisk = 0;

  roms.forEach((rom) => {
    if (rom.romFileId || (rom.monitored && isBefore(rom.airDateUtc))) {
      fileCount++;
    }

    if (rom.romFileId) {
      downloadedFileCount++;
    }

    if (rom.monitored) {
      monitoredFileCount++;
      hasMonitoredRoms = true;
    }

    totalFileCount++;
  });

  return {
    fileCount,
    downloadedFileCount,
    totalFileCount,
    monitoredFileCount,
    hasMonitoredRoms,
    sizeOnDisk,
  };
}

function useIsSearching(gameId: number, platformNumber: number) {
  const { data: commands } = useCommands();
  return isCommandExecuting(
    findCommand(commands, {
      name: CommandNames.SeasonSearch,
      gameId,
      platformNumber,
    })
  );
}

interface GameDetailsPlatformProps {
  gameId: number;
  monitored: boolean;
  platformNumber: number;
  statistics?: Statistics;
  isExpanded?: boolean;
  onExpandPress: (platformNumber: number, isExpanded: boolean) => void;
}

function GameDetailsPlatform({
  gameId,
  monitored,
  platformNumber,
  statistics = {} as Statistics,
  isExpanded,
  onExpandPress,
}: GameDetailsPlatformProps) {
  const executeCommand = useExecuteCommand();
  const { monitored: seriesMonitored, path } = useSingleGame(gameId)!;
  const { data: items } = usePlatformFiles(gameId, platformNumber);

  const { columns, sortKey, sortDirection } = useRomOptions();

  const isSmallScreen = useAppDimension('isSmallScreen');
  const isSearching = useIsSearching(gameId, platformNumber);

  const { sizeOnDisk = 0 } = statistics;

  const {
    fileCount,
    downloadedFileCount,
    totalFileCount,
    monitoredFileCount,
    hasMonitoredRoms,
  } = getPlatformStatistics(items);

  const previousFileCount = usePrevious(downloadedFileCount);

  const [isOrganizeModalOpen, setIsOrganizeModalOpen] = useState(false);
  const [isManageFilesOpen, setIsManageFilesOpen] = useState(false);
  const [isHistoryModalOpen, setIsHistoryModalOpen] = useState(false);
  const [isInteractiveSearchModalOpen, setIsInteractiveSearchModalOpen] =
    useState(false);

  const { toggleFilesMonitored, isToggling, togglingRomIds } =
    useToggleFilesMonitored(getQueryKey('roms')!);

  const { toggleSeasonMonitored, isTogglingSeasonMonitored } =
    useToggleSeasonMonitored(gameId);

  const lastToggledFile = useRef<number | null>(null);
  const hasSetInitalExpand = useRef(false);

  const platformTitle = useSingleGame(gameId)?.platforms?.find(
    (p) => p.platformNumber === platformNumber
  )?.title;

  const platformNumberTitle =
    platformNumber === 0
      ? translate('Specials')
      : platformTitle || translate('PlatformNumberToken', { platformNumber });

  const handleMonitorSeasonPress = useCallback(
    (value: boolean) => {
      toggleSeasonMonitored({
        platformNumber,
        monitored: value,
      });
    },
    [platformNumber, toggleSeasonMonitored]
  );

  const handleExpandPress = useCallback(() => {
    onExpandPress(platformNumber, !isExpanded);
  }, [platformNumber, isExpanded, onExpandPress]);

  const handleMonitorFilePress = useCallback(
    (romId: number, value: boolean, { shiftKey }: { shiftKey: boolean }) => {
      const lastToggled = lastToggledFile.current;
      const romIds = new Set([romId]);

      if (shiftKey && lastToggled) {
        const { lower, upper } = getToggledRange(items, romId, lastToggled);
        for (let i = lower; i < upper; i++) {
          romIds.add(items[i].id);
        }
      }

      lastToggledFile.current = romId;

      toggleFilesMonitored({
        romIds: Array.from(romIds),
        monitored: value,
      });
    },
    [items, toggleFilesMonitored]
  );

  const handleSearchPress = useCallback(() => {
    executeCommand({
      name: CommandNames.SeasonSearch,
      gameId,
      platformNumber,
    });
  }, [gameId, platformNumber, executeCommand]);

  const handleOrganizePress = useCallback(() => {
    setIsOrganizeModalOpen(true);
  }, []);

  const handleOrganizeModalClose = useCallback(() => {
    setIsOrganizeModalOpen(false);
  }, []);

  const handleManageFilesPress = useCallback(() => {
    setIsManageFilesOpen(true);
  }, []);

  const handleManageFilesModalClose = useCallback(() => {
    setIsManageFilesOpen(false);
  }, []);

  const handleHistoryPress = useCallback(() => {
    setIsHistoryModalOpen(true);
  }, []);

  const handleHistoryModalClose = useCallback(() => {
    setIsHistoryModalOpen(false);
  }, []);

  const handleInteractiveSearchPress = useCallback(() => {
    setIsInteractiveSearchModalOpen(true);
  }, []);

  const handleInteractiveSearchModalClose = useCallback(() => {
    setIsInteractiveSearchModalOpen(false);
  }, []);

  const handleSortPress = useCallback(
    (sortKey: string, sortDirection?: SortDirection) => {
      setFileSort({
        sortKey,
        sortDirection,
      });
    },
    []
  );

  const handleTableOptionChange = useCallback(
    (payload: TableOptionsChangePayload) => {
      setFileOptions(payload);
    },
    []
  );

  useEffect(() => {
    if (hasSetInitalExpand.current || items.length === 0) {
      return;
    }

    hasSetInitalExpand.current = true;

    const expand =
      items.some(
        (item) =>
          isAfter(item.airDateUtc) || isAfter(item.airDateUtc, { days: -30 })
      ) || items.every((item) => !item.airDateUtc);

    onExpandPress(platformNumber, expand && platformNumber > 0);
  }, [items, gameId, platformNumber, onExpandPress]);

  useEffect(() => {
    if ((previousFileCount ?? 0) > 0 && downloadedFileCount === 0) {
      setIsOrganizeModalOpen(false);
      setIsManageFilesOpen(false);
    }
  }, [downloadedFileCount, previousFileCount]);

  return (
    <div className={styles.platform}>
      <div className={styles.header}>
        <div className={styles.left}>
          <MonitorToggleButton
            monitored={monitored}
            isSaving={isTogglingSeasonMonitored}
            size={24}
            onPress={handleMonitorSeasonPress}
          />

          <div className={styles.seasonInfo}>
            <div className={styles.platformNumber}>{platformNumberTitle}</div>
          </div>

          <div className={styles.seasonStats}>
            <Popover
              className={styles.fileCountTooltip}
              canFlip={true}
              anchor={
                <PlatformProgressLabel
                  className={styles.seasonStatsLabel}
                  gameId={gameId}
                  platformNumber={platformNumber}
                  monitored={monitored}
                  fileCount={fileCount}
                  downloadedFileCount={downloadedFileCount}
                />
              }
              title={translate('PlatformInformation')}
              body={
                <div>
                  <PlatformInfo
                    totalFileCount={totalFileCount}
                    monitoredFileCount={monitoredFileCount}
                    downloadedFileCount={downloadedFileCount}
                    sizeOnDisk={sizeOnDisk}
                  />
                </div>
              }
              position={tooltipPositions.BOTTOM}
            />

            {sizeOnDisk ? (
              <Label
                className={styles.seasonStatsLabel}
                kind="default"
                size="large"
              >
                {formatBytes(sizeOnDisk)}
              </Label>
            ) : null}
          </div>
        </div>

        <Link className={styles.expandButton} onPress={handleExpandPress}>
          <Icon
            className={styles.expandButtonIcon}
            name={isExpanded ? icons.COLLAPSE : icons.EXPAND}
            title={
              isExpanded ? translate('HideFiles') : translate('ShowFiles')
            }
            size={24}
          />
          {isSmallScreen ? null : <span>&nbsp;</span>}
        </Link>

        {isSmallScreen ? (
          <Menu
            className={styles.actionsMenu}
            alignMenu={align.RIGHT}
            enforceMaxHeight={false}
          >
            <MenuButton>
              <Icon name={icons.ACTIONS} size={22} />
            </MenuButton>

            <MenuContent className={styles.actionsMenuContent}>
              <MenuItem
                isDisabled={
                  isSearching || !hasMonitoredRoms || !seriesMonitored
                }
                onPress={handleSearchPress}
              >
                <SpinnerIcon
                  className={styles.actionMenuIcon}
                  name={icons.SEARCH}
                  isSpinning={isSearching}
                />

                {translate('Search')}
              </MenuItem>

              <MenuItem
                isDisabled={!totalFileCount}
                onPress={handleInteractiveSearchPress}
              >
                <Icon
                  className={styles.actionMenuIcon}
                  name={icons.INTERACTIVE}
                />

                {translate('InteractiveSearch')}
              </MenuItem>

              <MenuItem
                isDisabled={!fileCount}
                onPress={handleOrganizePress}
              >
                <Icon className={styles.actionMenuIcon} name={icons.ORGANIZE} />

                {translate('PreviewRename')}
              </MenuItem>

              <MenuItem
                isDisabled={!fileCount}
                onPress={handleManageFilesPress}
              >
                <Icon className={styles.actionMenuIcon} name={icons.ROM_FILE} />

                {translate('ManageFiles')}
              </MenuItem>

              <MenuItem
                isDisabled={!totalFileCount}
                onPress={handleHistoryPress}
              >
                <Icon className={styles.actionMenuIcon} name={icons.HISTORY} />

                {translate('History')}
              </MenuItem>
            </MenuContent>
          </Menu>
        ) : (
          <div className={styles.actions}>
            <SpinnerIconButton
              className={styles.actionButton}
              name={icons.SEARCH}
              title={
                hasMonitoredRoms && seriesMonitored
                  ? translate('SearchForMonitoredFilesSeason')
                  : translate('NoMonitoredFilesPlatform')
              }
              size={24}
              isSpinning={isSearching}
              isDisabled={
                isSearching || !hasMonitoredRoms || !seriesMonitored
              }
              onPress={handleSearchPress}
            />

            <IconButton
              className={styles.actionButton}
              name={icons.INTERACTIVE}
              title={translate('InteractiveSearchSeason')}
              size={24}
              isDisabled={!totalFileCount}
              onPress={handleInteractiveSearchPress}
            />

            <IconButton
              className={styles.actionButton}
              name={icons.ORGANIZE}
              title={translate('PreviewRenameSeason')}
              size={24}
              isDisabled={!fileCount}
              onPress={handleOrganizePress}
            />

            <IconButton
              className={styles.actionButton}
              name={icons.ROM_FILE}
              title={translate('ManageFilesSeason')}
              size={24}
              isDisabled={!fileCount}
              onPress={handleManageFilesPress}
            />

            <IconButton
              className={styles.actionButton}
              name={icons.HISTORY}
              title={translate('HistorySeason')}
              size={24}
              isDisabled={!totalFileCount}
              onPress={handleHistoryPress}
            />
          </div>
        )}
      </div>

      <div>
        {isExpanded ? (
          <div className={styles.roms}>
            {items.length ? (
              <Table
                columns={columns}
                sortKey={sortKey}
                sortDirection={sortDirection}
                onSortPress={handleSortPress}
                onTableOptionChange={handleTableOptionChange}
              >
                <TableBody>
                  {items.map((item) => {
                    return (
                      <RomRow
                        key={item.id}
                        columns={columns}
                        {...item}
                        isSaving={
                          isToggling && togglingRomIds.includes(item.id)
                        }
                        onMonitorFilePress={handleMonitorFilePress}
                      />
                    );
                  })}
                </TableBody>
              </Table>
            ) : (
              <div className={styles.noRoms}>
                {translate('NoFilesInThisPlatform')}
              </div>
            )}

            <div className={styles.collapseButtonContainer}>
              <IconButton
                iconClassName={styles.collapseButtonIcon}
                name={icons.COLLAPSE}
                size={20}
                title={translate('HideFiles')}
                onPress={handleExpandPress}
              />
            </div>
          </div>
        ) : null}
      </div>

      <OrganizePreviewModal
        isOpen={isOrganizeModalOpen}
        gameId={gameId}
        platformNumber={platformNumber}
        onModalClose={handleOrganizeModalClose}
      />

      <InteractiveImportModal
        isOpen={isManageFilesOpen}
        gameId={gameId}
        platformNumber={platformNumber}
        title={platformNumberTitle}
        folder={path}
        initialSortKey="relativePath"
        initialSortDirection={sortDirections.DESCENDING}
        showSeries={false}
        allowSeriesChange={false}
        showDelete={true}
        showImportMode={false}
        modalTitle={translate('ManageFiles')}
        onModalClose={handleManageFilesModalClose}
      />

      <GameHistoryModal
        isOpen={isHistoryModalOpen}
        gameId={gameId}
        platformNumber={platformNumber}
        onModalClose={handleHistoryModalClose}
      />

      <PlatformInteractiveSearchModal
        isOpen={isInteractiveSearchModalOpen}
        fileCount={totalFileCount}
        gameId={gameId}
        platformNumber={platformNumber}
        onModalClose={handleInteractiveSearchModalClose}
      />
    </div>
  );
}

export default GameDetailsPlatform;
