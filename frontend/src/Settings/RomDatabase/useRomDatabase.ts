import { useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import getQueryPath from 'Utilities/Fetch/getQueryPath';

export interface RomDatabaseSystem {
  id: string;
  name: string;
  source: string;
  estimatedSize: string;
}

export interface RomDatabaseStatus {
  isDownloaded: boolean;
  entryCount: number;
  lastUpdated: string | null;
  filePath: string;
}

const DEFAULT_SYSTEMS: RomDatabaseSystem[] = [];

export const useRomDatabaseSystems = () => {
  const result = useApiQuery<RomDatabaseSystem[]>({
    path: '/romdatabase/systems',
  });

  return {
    ...result,
    data: result.data ?? DEFAULT_SYSTEMS,
  };
};

export const useRomDatabaseStatus = (systemId: string) => {
  const result = useApiQuery<RomDatabaseStatus>({
    path: `/romdatabase/systems/${encodeURIComponent(systemId)}/status`,
    queryOptions: {
      enabled: !!systemId,
    },
  });

  return result;
};

export const useImportRomDatabase = (systemId: string) => {
  const queryClient = useQueryClient();
  const [isImporting, setIsImporting] = useState(false);
  const [progress, setProgress] = useState<number | null>(null);
  const [importError, setImportError] = useState<string | null>(null);

  const importDatabase = (file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    const request = new XMLHttpRequest();

    setIsImporting(true);
    setProgress(0);
    setImportError(null);

    request.upload.onprogress = (event) => {
      if (event.lengthComputable) {
        setProgress(Math.round((event.loaded / event.total) * 100));
      }
    };

    request.onload = () => {
      setIsImporting(false);

      if (request.status >= 200 && request.status < 300) {
        queryClient.invalidateQueries({
          queryKey: [`/romdatabase/systems/${systemId}/status`],
        });
      } else {
        try {
          setImportError(
            JSON.parse(request.responseText).message || 'Import failed'
          );
        } catch {
          setImportError('Import failed');
        }
      }
    };

    request.onerror = () => {
      setIsImporting(false);
      setImportError('Import failed: network error');
    };

    request.open(
      'POST',
      getQueryPath(
        `/romdatabase/systems/${encodeURIComponent(systemId)}/import`
      )
    );
    request.setRequestHeader('X-Api-Key', window.Romarr.apiKey);
    request.setRequestHeader('X-Romarr-Client', 'Romarr');
    request.send(formData);
  };

  return {
    importDatabase,
    isImporting,
    progress,
    importError,
  };
};
