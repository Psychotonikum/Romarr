import { GameStatus } from 'Game/Game';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

export function getGameStatusDetails(status: GameStatus) {
  let statusDetails = {
    icon: icons.SERIES_CONTINUING,
    title: translate('Continuing'),
    message: translate('ContinuingGameDescription'),
  };

  if (status === 'deleted') {
    statusDetails = {
      icon: icons.SERIES_DELETED,
      title: translate('Deleted'),
      message: translate('DeletedGameDescription'),
    };
  } else if (status === 'ended') {
    statusDetails = {
      icon: icons.SERIES_ENDED,
      title: translate('Complete'),
      message: translate('EndedGameDescription'),
    };
  } else if (status === 'upcoming') {
    statusDetails = {
      icon: icons.SERIES_CONTINUING,
      title: translate('Upcoming'),
      message: translate('UpcomingGameDescription'),
    };
  }

  return statusDetails;
}
