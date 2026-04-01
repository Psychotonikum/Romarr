import React from 'react';
import RomNumber, { RomNumberProps } from './RomNumber';

interface PlatformRomNumberProps extends RomNumberProps {
  airDate?: string;
}

function PlatformRomNumber(props: PlatformRomNumberProps) {
  const { airDate, ...otherProps } = props;

  return (
    <RomNumber showPlatformNumber={true} {...otherProps} />
  );
}

export default PlatformRomNumber;
