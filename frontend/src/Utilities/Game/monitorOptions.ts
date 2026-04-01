import { GameMonitor } from 'Game/Game';
import translate from 'Utilities/String/translate';

interface MonitorOption {
  key: GameMonitor;
  value: string;
}

const monitorOptions: MonitorOption[] = [
  {
    key: 'all',
    get value() {
      return translate('MonitorAllFiles');
    },
  },
  {
    key: 'future',
    get value() {
      return translate('MonitorFutureFiles');
    },
  },
  {
    key: 'missing',
    get value() {
      return translate('MonitorMissingFiles');
    },
  },
  {
    key: 'existing',
    get value() {
      return translate('MonitorExistingFiles');
    },
  },
  {
    key: 'firstPlatform',
    get value() {
      return translate('MonitorFirstPlatform');
    },
  },
  {
    key: 'lastPlatform',
    get value() {
      return translate('MonitorLastPlatform');
    },
  },
  {
    key: 'baseGame',
    get value() {
      return translate('MonitorBaseGame');
    },
  },
  {
    key: 'allDlcs',
    get value() {
      return translate('MonitorAllDlcs');
    },
  },
  {
    key: 'latestUpdate',
    get value() {
      return translate('MonitorLatestUpdate');
    },
  },
  {
    key: 'allAdditional',
    get value() {
      return translate('MonitorAllAdditional');
    },
  },
  {
    key: 'none',
    get value() {
      return translate('MonitorNoFiles');
    },
  },
];

export default monitorOptions;
