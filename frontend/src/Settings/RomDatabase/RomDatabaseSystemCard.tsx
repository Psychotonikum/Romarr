import React, { ChangeEvent, useCallback, useRef } from 'react';
import Icon from 'Components/Icon';
import SpinnerButton from 'Components/Link/SpinnerButton';
import { icons, kinds } from 'Helpers/Props';
import {
  RomDatabaseSystem,
  useImportRomDatabase,
  useRomDatabaseStatus,
} from './useRomDatabase';
import styles from './RomDatabaseSystemCard.css';

interface RomDatabaseSystemCardProps {
  system: RomDatabaseSystem;
}

function RomDatabaseSystemCard({ system }: RomDatabaseSystemCardProps) {
  const { data: status, isLoading } = useRomDatabaseStatus(system.id);
  const { importDatabase, isImporting, progress, importError } =
    useImportRomDatabase(system.id);
  const fileInput = useRef<HTMLInputElement>(null);

  const handleFileChange = useCallback(
    (event: ChangeEvent<HTMLInputElement>) => {
      const file = event.target.files?.[0];

      if (file) {
        importDatabase(file);
        event.target.value = '';
      }
    },
    [importDatabase]
  );

  const handleImportPress = useCallback(() => fileInput.current?.click(), []);

  let statusContent = (
    <span className={styles.notDownloaded}>Not downloaded</span>
  );

  if (isLoading) {
    statusContent = <span>Loading...</span>;
  }

  if (status?.isDownloaded) {
    statusContent = (
      <span className={styles.downloaded}>
        <Icon name={icons.CHECK} /> Downloaded
        <div className={styles.entryCount}>{status.entryCount} entries</div>
      </span>
    );
  }

  if (isImporting) {
    statusContent = (
      <span>
        Importing{progress === null ? '...' : `: ${progress}% uploaded`}
      </span>
    );
  }

  if (importError) {
    statusContent = <span role="alert">{importError}</span>;
  }

  return (
    <div className={styles.system}>
      <div className={styles.name}>{system.name}</div>
      <div className={styles.source}>
        <a
          href={
            system.source === 'No-Intro'
              ? 'https://datomatic.no-intro.org/index.php?page=download'
              : 'https://redump.org/downloads/'
          }
          target="_blank"
          rel="noopener noreferrer"
        >
          {system.source} DATs
        </a>
      </div>
      <div className={styles.size}>{system.estimatedSize}</div>

      <div className={styles.status}>{statusContent}</div>

      <div className={styles.actions}>
        <input
          ref={fileInput}
          type="file"
          accept=".dat,.xml"
          hidden={true}
          aria-label={`Select ${system.name} DAT file`}
          onChange={handleFileChange}
        />
        <SpinnerButton
          kind={status?.isDownloaded ? kinds.DEFAULT : kinds.PRIMARY}
          isSpinning={isImporting}
          onPress={handleImportPress}
        >
          {status?.isDownloaded ? 'Replace DAT' : 'Import DAT'}
        </SpinnerButton>
      </div>
    </div>
  );
}

export default RomDatabaseSystemCard;
