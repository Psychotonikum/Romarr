import React from 'react';
import Icon from 'Components/Icon';
import Popover from 'Components/Tooltip/Popover';
import { AlternateTitle } from 'Game/Game';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import filterAlternateTitles from 'Utilities/Game/filterAlternateTitles';
import padNumber from 'Utilities/Number/padNumber';
import translate from 'Utilities/String/translate';
import SceneInfo from './SceneInfo';
import styles from './RomNumber.css';

function getWarningMessage(
  unverifiedSceneNumbering: boolean
) {
  const messages = [];

  if (unverifiedSceneNumbering) {
    messages.push(translate('SceneNumberNotVerified'));
  }

  return messages.join('\n');
}

export interface RomNumberProps {
  platformNumber: number;
  romNumber: number;
  absoluteRomNumber?: number;
  scenePlatformNumber?: number;
  sceneRomNumber?: number;
  sceneAbsoluteRomNumber?: number;
  useSceneNumbering?: boolean;
  unverifiedSceneNumbering?: boolean;
  alternateTitles?: AlternateTitle[];
  showPlatformNumber?: boolean;
}

function RomNumber(props: RomNumberProps) {
  const {
    platformNumber,
    romNumber,
    scenePlatformNumber,
    sceneRomNumber,
    sceneAbsoluteRomNumber,
    useSceneNumbering = false,
    unverifiedSceneNumbering = false,
    alternateTitles: seriesAlternateTitles = [],
    showPlatformNumber = false,
  } = props;

  const alternateTitles = filterAlternateTitles(
    seriesAlternateTitles,
    null,
    useSceneNumbering,
    platformNumber,
    scenePlatformNumber
  );

  const hasSceneInformation =
    scenePlatformNumber !== undefined ||
    sceneRomNumber !== undefined ||
    !!alternateTitles.length;

  const warningMessage = getWarningMessage(
    unverifiedSceneNumbering
  );

  return (
    <span>
      {hasSceneInformation ? (
        <Popover
          anchor={
            <span>
              {showPlatformNumber && platformNumber != null && (
                <>{platformNumber}x</>
              )}

              {showPlatformNumber ? padNumber(romNumber, 2) : romNumber}
            </span>
          }
          title={translate('SceneInformation')}
          body={
            <SceneInfo
              platformNumber={platformNumber}
              romNumber={romNumber}
              scenePlatformNumber={scenePlatformNumber}
              sceneRomNumber={sceneRomNumber}
              sceneAbsoluteRomNumber={sceneAbsoluteRomNumber}
              alternateTitles={alternateTitles}
            />
          }
          position={tooltipPositions.RIGHT}
        />
      ) : (
        <span>
          {showPlatformNumber && platformNumber != null && (
            <>{platformNumber}x</>
          )}

          {showPlatformNumber ? padNumber(romNumber, 2) : romNumber}
        </span>
      )}

      {warningMessage ? (
        <Icon
          className={styles.warning}
          name={icons.WARNING}
          kind={kinds.WARNING}
          title={warningMessage}
        />
      ) : null}
    </span>
  );
}

export default RomNumber;
