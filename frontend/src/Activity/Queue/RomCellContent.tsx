import React from 'react';
import Game from 'Game/Game';
import PlatformRomNumber from 'Rom/PlatformRomNumber';
import Rom from 'Rom/Rom';
import translate from 'Utilities/String/translate';

interface RomCellContentProps {
  roms: Rom[];
  isFullPlatform: boolean;
  platformNumber?: number;
  game?: Game;
}

export default function RomCellContent({
  roms,
  isFullPlatform,
  platformNumber,
  game,
}: RomCellContentProps) {
  if (roms.length === 0) {
    return '-';
  }

  if (isFullPlatform && platformNumber != null) {
    return translate('PlatformNumberToken', { platformNumber });
  }

  if (roms.length === 1) {
    const rom = roms[0];

    return (
      <PlatformRomNumber
        platformNumber={rom.platformNumber}
        romNumber={rom.romNumber}
        absoluteRomNumber={rom.absoluteRomNumber}
        alternateTitles={game?.alternateTitles}
        scenePlatformNumber={rom.scenePlatformNumber}
        sceneRomNumber={rom.sceneRomNumber}
        sceneAbsoluteRomNumber={rom.sceneAbsoluteRomNumber}
        unverifiedSceneNumbering={rom.unverifiedSceneNumbering}
      />
    );
  }

  const firstFile = roms[0];
  const lastFile = roms[roms.length - 1];

  return (
    <>
      <PlatformRomNumber
        platformNumber={firstFile.platformNumber}
        romNumber={firstFile.romNumber}
        absoluteRomNumber={firstFile.absoluteRomNumber}
        alternateTitles={game?.alternateTitles}
        scenePlatformNumber={firstFile.scenePlatformNumber}
        sceneRomNumber={firstFile.sceneRomNumber}
        sceneAbsoluteRomNumber={firstFile.sceneAbsoluteRomNumber}
        unverifiedSceneNumbering={firstFile.unverifiedSceneNumbering}
      />
      {' - '}
      <PlatformRomNumber
        platformNumber={lastFile.platformNumber}
        romNumber={lastFile.romNumber}
        absoluteRomNumber={lastFile.absoluteRomNumber}
        alternateTitles={game?.alternateTitles}
        scenePlatformNumber={lastFile.scenePlatformNumber}
        sceneRomNumber={lastFile.sceneRomNumber}
        sceneAbsoluteRomNumber={lastFile.sceneAbsoluteRomNumber}
        unverifiedSceneNumbering={lastFile.unverifiedSceneNumbering}
      />
    </>
  );
}
