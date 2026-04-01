import React from 'react';
import { useQueueDetailsForSeries } from 'Activity/Queue/Details/QueueDetailsProvider';
import Label from 'Components/Label';
import { kinds, sizes } from 'Helpers/Props';

function getFileCountKind(
  monitored: boolean,
  fileCount: number,
  downloadedFileCount: number,
  isDownloading: boolean
) {
  if (isDownloading) {
    return kinds.PURPLE;
  }

  if (downloadedFileCount === fileCount && fileCount > 0) {
    return kinds.SUCCESS;
  }

  if (!monitored) {
    return kinds.WARNING;
  }

  return kinds.DANGER;
}

interface GameProgressLabelProps {
  className: string;
  gameId: number;
  monitored: boolean;
  fileCount: number;
  downloadedFileCount: number;
}

function GameProgressLabel({
  className,
  gameId,
  monitored,
  fileCount,
  downloadedFileCount,
}: GameProgressLabelProps) {
  const queueDetails = useQueueDetailsForSeries(gameId);

  const newDownloads = queueDetails.count - queueDetails.filesWithData;
  const text = newDownloads
    ? `${downloadedFileCount} + ${newDownloads} / ${fileCount}`
    : `${downloadedFileCount} / ${fileCount}`;

  return (
    <Label
      className={className}
      kind={getFileCountKind(
        monitored,
        fileCount,
        downloadedFileCount,
        queueDetails.count > 0
      )}
      size={sizes.LARGE}
    >
      <span>{text}</span>
    </Label>
  );
}

export default GameProgressLabel;
