import React, { useCallback } from 'react';
import { SetFilter } from 'Components/Filter/Filter';
import FilterModal, { FilterModalProps } from 'Components/Filter/FilterModal';
import InteractiveSearchType from 'InteractiveSearch/InteractiveSearchType';
import InteractiveSearchPayload from './InteractiveSearchPayload';
import { setReleaseOption } from './releaseOptionsStore';
import useReleases, { FILTER_BUILDER, Release } from './useReleases';

interface InteractiveSearchFilterModalProps extends FilterModalProps<Release> {
  type: InteractiveSearchType;
  searchPayload: InteractiveSearchPayload;
}

export default function InteractiveSearchFilterModal({
  type,
  searchPayload,
  ...otherProps
}: InteractiveSearchFilterModalProps) {
  const { data } = useReleases(searchPayload);

  const handleFilterSelect = useCallback(
    (selectedFilter: SetFilter) => {
      if (type === 'rom') {
        setReleaseOption(
          'fileSelectedFilterKey',
          selectedFilter.selectedFilterKey
        );
      } else {
        setReleaseOption(
          'platformSelectedFilterKey',
          selectedFilter.selectedFilterKey
        );
      }
    },
    [type]
  );

  return (
    <FilterModal
      {...otherProps}
      sectionItems={data}
      filterBuilderProps={FILTER_BUILDER}
      customFilterType="releases"
      dispatchSetFilter={handleFilterSelect}
    />
  );
}
