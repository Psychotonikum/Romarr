import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import translate from 'Utilities/String/translate';

function GameTypePopoverContent() {
  return (
    <DescriptionList>
      <DescriptionListItem
        title={translate('Standard')}
        data={translate('StandardEpisodeTypeDescription')}
      />
    </DescriptionList>
  );
}

export default GameTypePopoverContent;
