import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import translate from 'Utilities/String/translate';

function GameMonitoringOptionsPopoverContent() {
  return (
    <DescriptionList>
      <DescriptionListItem
        title={translate('MonitorAllFiles')}
        data={translate('MonitorAllEpisodesDescription')}
      />

      <DescriptionListItem
        title={translate('MonitorFutureFiles')}
        data={translate('MonitorFutureEpisodesDescription')}
      />

      <DescriptionListItem
        title={translate('MonitorMissingFiles')}
        data={translate('MonitorMissingEpisodesDescription')}
      />

      <DescriptionListItem
        title={translate('MonitorExistingFiles')}
        data={translate('MonitorExistingEpisodesDescription')}
      />

      <DescriptionListItem
        title={translate('MonitorBaseGame')}
        data={translate('MonitorBaseGameDescription')}
      />

      <DescriptionListItem
        title={translate('MonitorAllDlcs')}
        data={translate('MonitorAllDlcsDescription')}
      />

      <DescriptionListItem
        title={translate('MonitorLatestUpdate')}
        data={translate('MonitorLatestUpdateDescription')}
      />

      <DescriptionListItem
        title={translate('MonitorAllAdditional')}
        data={translate('MonitorAllAdditionalDescription')}
      />

      <DescriptionListItem
        title={translate('MonitorSpecialFiles')}
        data={translate('MonitorSpecialEpisodesDescription')}
      />
    </DescriptionList>
  );
}

export default GameMonitoringOptionsPopoverContent;
