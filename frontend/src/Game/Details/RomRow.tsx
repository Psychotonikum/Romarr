import React, { useCallback } from 'react';
import Icon from 'Components/Icon';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import RelativeDateCell from 'Components/Table/Cells/RelativeDateCell';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import Column from 'Components/Table/Column';
import TableRow from 'Components/Table/TableRow';
import Popover from 'Components/Tooltip/Popover';
import Tooltip from 'Components/Tooltip/Tooltip';
import { useSingleGame } from 'Game/useGame';
import { icons } from 'Helpers/Props';
import IndexerFlags from 'Rom/IndexerFlags';
import RomFormats from 'Rom/RomFormats';
import RomNumber from 'Rom/RomNumber';
import RomSearchCell from 'Rom/RomSearchCell';
import RomStatus from 'Rom/RomStatus';
import RomTitleLink from 'Rom/RomTitleLink';
import MediaInfo from 'RomFile/MediaInfo';
import RomFileLanguages from 'RomFile/RomFileLanguages';
import { useRomFile } from 'RomFile/RomFileProvider';
import MediaInfoModel from 'typings/MediaInfo';
import formatBytes from 'Utilities/Number/formatBytes';
import formatCustomFormatScore from 'Utilities/Number/formatCustomFormatScore';
import formatRuntime from 'Utilities/Number/formatRuntime';
import translate from 'Utilities/String/translate';
import styles from './RomRow.css';

interface RomRowProps {
  id: number;
  gameId: number;
  romFileId?: number;
  monitored: boolean;
  platformNumber: number;
  romNumber: number;
  absoluteRomNumber?: number;
  scenePlatformNumber?: number;
  sceneRomNumber?: number;
  sceneAbsoluteRomNumber?: number;
  airDateUtc?: string;
  runtime?: number;
  finaleType?: string;
  title: string;
  romType?: string;
  isSaving?: boolean;
  unverifiedSceneNumbering?: boolean;
  // romFilePath?: string;
  // romFileRelativePath?: string;
  // romFileSize?: number;
  // releaseGroup?: string;
  // customFormats?: CustomFormat[];
  // customFormatScore: number;
  // indexerFlags?: number;
  mediaInfo?: MediaInfoModel;
  columns: Column[];
  onMonitorFilePress: (
    romId: number,
    value: boolean,
    { shiftKey }: { shiftKey: boolean }
  ) => void;
}

function RomRow({
  id,
  gameId,
  romFileId,
  monitored,
  platformNumber,
  romNumber,
  absoluteRomNumber,
  scenePlatformNumber,
  sceneRomNumber,
  sceneAbsoluteRomNumber,
  airDateUtc,
  runtime,
  finaleType,
  title,
  romType,
  unverifiedSceneNumbering,
  isSaving,
  // romFilePath,
  // romFileRelativePath,
  // romFileSize,
  // releaseGroup,
  // customFormats = [],
  // customFormatScore,
  // indexerFlags = 0,
  columns,
  onMonitorFilePress,
}: RomRowProps) {
  const {
    useSceneNumbering,
    alternateTitles = [],
  } = useSingleGame(gameId)!;
  const romFile = useRomFile(romFileId);

  const customFormats = romFile?.customFormats ?? [];
  const customFormatScore = romFile?.customFormatScore ?? 0;

  const handleMonitorFilePress = useCallback(
    (monitored: boolean, options: { shiftKey: boolean }) => {
      onMonitorFilePress(id, monitored, options);
    },
    [id, onMonitorFilePress]
  );

  return (
    <TableRow>
      {columns.map((column) => {
        const { name, isVisible } = column;

        if (!isVisible) {
          return null;
        }

        if (name === 'monitored') {
          return (
            <TableRowCell key={name} className={styles.monitored}>
              <MonitorToggleButton
                monitored={monitored}
                isSaving={isSaving}
                onPress={handleMonitorFilePress}
              />
            </TableRowCell>
          );
        }

        if (name === 'romNumber') {
          return (
            <TableRowCell
              key={name}
              className={styles.romNumber}
            >
              <RomNumber
                platformNumber={platformNumber}
                romNumber={romNumber}
                absoluteRomNumber={absoluteRomNumber}
                useSceneNumbering={useSceneNumbering}
                unverifiedSceneNumbering={unverifiedSceneNumbering}
                scenePlatformNumber={scenePlatformNumber}
                sceneRomNumber={sceneRomNumber}
                sceneAbsoluteRomNumber={sceneAbsoluteRomNumber}
                alternateTitles={alternateTitles}
              />
            </TableRowCell>
          );
        }

        if (name === 'title') {
          return (
            <TableRowCell key={name} className={styles.title}>
              <RomTitleLink
                romId={id}
                gameId={gameId}
                romTitle={title}
                romEntity="roms"
                finaleType={finaleType}
                showOpenSeriesButton={false}
              />
            </TableRowCell>
          );
        }

        if (name === 'romType') {
          const typeLabel =
            romType === 'update'
              ? 'UPT'
              : romType === 'dlc'
                ? 'DLC'
                : 'BASE';

          return (
            <TableRowCell key={name} className={styles.romType}>
              {typeLabel}
            </TableRowCell>
          );
        }

        if (name === 'region') {
          return (
            <TableRowCell key={name}>
              {romFile?.region ?? ''}
            </TableRowCell>
          );
        }

        if (name === 'romReleaseType') {
          const releaseTypeLabels: Record<number, string> = {
            0: '',
            1: 'Retail',
            2: 'Prototype',
            3: 'Beta',
            4: 'Demo',
            5: 'Sample',
            6: 'Promo',
          };

          return (
            <TableRowCell key={name}>
              {releaseTypeLabels[romFile?.romReleaseType ?? 0] ?? ''}
            </TableRowCell>
          );
        }

        if (name === 'modification') {
          const modLabels: Record<number, string> = {
            0: '',
            1: 'Original',
            2: 'Hack',
            3: 'Translation',
            4: 'Homebrew',
            5: 'Unlicensed',
          };

          return (
            <TableRowCell key={name}>
              {romFile?.modificationName || modLabels[romFile?.modification ?? 0] || ''}
            </TableRowCell>
          );
        }

        if (name === 'path') {
          return <TableRowCell key={name}>{romFile?.path}</TableRowCell>;
        }

        if (name === 'relativePath') {
          return (
            <TableRowCell key={name}>{romFile?.relativePath}</TableRowCell>
          );
        }

        if (name === 'airDateUtc') {
          return <RelativeDateCell key={name} date={airDateUtc} />;
        }

        if (name === 'runtime') {
          return (
            <TableRowCell key={name} className={styles.runtime}>
              {formatRuntime(runtime)}
            </TableRowCell>
          );
        }

        if (name === 'customFormats') {
          return (
            <TableRowCell key={name}>
              <RomFormats formats={customFormats} />
            </TableRowCell>
          );
        }

        if (name === 'customFormatScore') {
          return (
            <TableRowCell key={name} className={styles.customFormatScore}>
              <Tooltip
                anchor={formatCustomFormatScore(
                  customFormatScore,
                  customFormats.length
                )}
                tooltip={<RomFormats formats={customFormats} />}
                position="left"
              />
            </TableRowCell>
          );
        }

        if (name === 'languages') {
          return (
            <TableRowCell key={name} className={styles.languages}>
              <RomFileLanguages romFileId={romFileId} />
            </TableRowCell>
          );
        }

        if (name === 'audioInfo') {
          return (
            <TableRowCell key={name} className={styles.audio}>
              <MediaInfo type="audio" romFileId={romFileId} />
            </TableRowCell>
          );
        }

        if (name === 'audioLanguages') {
          return (
            <TableRowCell key={name} className={styles.audioLanguages}>
              <MediaInfo type="audioLanguages" romFileId={romFileId} />
            </TableRowCell>
          );
        }

        if (name === 'subtitleLanguages') {
          return (
            <TableRowCell key={name} className={styles.subtitles}>
              <MediaInfo type="subtitles" romFileId={romFileId} />
            </TableRowCell>
          );
        }

        if (name === 'videoCodec') {
          return (
            <TableRowCell key={name} className={styles.video}>
              <MediaInfo type="file" romFileId={romFileId} />
            </TableRowCell>
          );
        }

        if (name === 'videoDynamicRangeType') {
          return (
            <TableRowCell key={name} className={styles.videoDynamicRangeType}>
              <MediaInfo type="videoDynamicRangeType" romFileId={romFileId} />
            </TableRowCell>
          );
        }

        if (name === 'size') {
          return (
            <TableRowCell key={name} className={styles.size}>
              {!!romFile?.size && formatBytes(romFile?.size)}
            </TableRowCell>
          );
        }

        if (name === 'releaseGroup') {
          return (
            <TableRowCell key={name} className={styles.releaseGroup}>
              {romFile?.releaseGroup}
            </TableRowCell>
          );
        }

        if (name === 'indexerFlags') {
          return (
            <TableRowCell key={name} className={styles.indexerFlags}>
              {romFile?.indexerFlags ? (
                <Popover
                  anchor={<Icon name={icons.FLAG} kind="default" />}
                  title={translate('IndexerFlags')}
                  body={<IndexerFlags indexerFlags={romFile?.indexerFlags} />}
                  position="left"
                />
              ) : null}
            </TableRowCell>
          );
        }

        if (name === 'status') {
          return (
            <TableRowCell key={name} className={styles.status}>
              <RomStatus romId={id} romFileId={romFileId} />
            </TableRowCell>
          );
        }

        if (name === 'actions') {
          return (
            <RomSearchCell
              key={name}
              romId={id}
              romEntity="roms"
              gameId={gameId}
              romTitle={title}
              showOpenSeriesButton={false}
            />
          );
        }

        return null;
      })}
    </TableRow>
  );
}

export default RomRow;
