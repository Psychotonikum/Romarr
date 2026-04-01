import React, { useCallback, useEffect, useState } from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputButton from 'Components/Form/FormInputButton';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import { EnhancedSelectInputValue } from 'Components/Form/Select/EnhancedSelectInput';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import useDebounce from 'Helpers/Hooks/useDebounce';
import useModalOpenState from 'Helpers/Hooks/useModalOpenState';
import { inputTypes, kinds, sizes } from 'Helpers/Props';
import { useShowAdvancedSettings } from 'Settings/advancedSettingsStore';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import NamingModal from './NamingModal';
import {
  NamingSettingsModel,
  useManageNamingSettings,
  useNamingExamples,
} from './useNamingSettings';
import styles from './Naming.css';

interface NamingModalOptions {
  name: keyof Pick<
    NamingSettingsModel,
    | 'standardGameFileFormat'
    | 'gameFolderFormat'
    | 'platformFolderFormat'
  >;
  platform?: boolean;
  rom?: boolean;
  additional?: boolean;
}

interface NamingProps {
  setChildSave: (saveCallback: () => void) => void;
  onChildStateChange: (state: {
    isSaving: boolean;
    hasPendingChanges: boolean;
  }) => void;
}

function Naming({ setChildSave, onChildStateChange }: NamingProps) {
  const advancedSettings = useShowAdvancedSettings();
  const {
    settings,
    updateSetting,
    isFetching,
    error,
    hasSettings,
    hasPendingChanges,
    isSaving,
    saveSettings,
  } = useManageNamingSettings();

  const debouncedSettings = useDebounce(settings, 300);
  const { examples } = useNamingExamples(debouncedSettings);
  const examplesPopulated = !!examples;

  const [isNamingModalOpen, setNamingModalOpen, setNamingModalClosed] =
    useModalOpenState(false);
  const [namingModalOptions, setNamingModalOptions] =
    useState<NamingModalOptions | null>(null);

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      const key = change.name as keyof NamingSettingsModel;

      updateSetting(key, change.value as NamingSettingsModel[typeof key]);
    },
    [updateSetting]
  );

  const handleStandardNamingModalOpenClick = useCallback(() => {
    setNamingModalOpen();

    setNamingModalOptions({
      name: 'standardGameFileFormat',
      platform: true,
      rom: true,
      additional: true,
    });
  }, [setNamingModalOpen, setNamingModalOptions]);

  const handleGameFolderNamingModalOpenClick = useCallback(() => {
    setNamingModalOpen();

    setNamingModalOptions({
      name: 'gameFolderFormat',
    });
  }, [setNamingModalOpen, setNamingModalOptions]);

  const handlePlatformFolderNamingModalOpenClick = useCallback(() => {
    setNamingModalOpen();

    setNamingModalOptions({
      name: 'platformFolderFormat',
      platform: true,
    });
  }, [setNamingModalOpen, setNamingModalOptions]);

  const renameRoms = hasSettings && settings.renameRoms.value;
  const replaceIllegalCharacters =
    hasSettings && settings.replaceIllegalCharacters.value;

  const multiGameFileStyleOptions: EnhancedSelectInputValue<number>[] = [
    { key: 0, value: translate('Extend'), hint: 'Platform1-File01-02-03' },
    { key: 1, value: translate('Duplicate'), hint: 'Platform1.File01.Platform1.File02' },
    { key: 2, value: translate('Repeat'), hint: 'Platform1.File01.File02.File03' },
    { key: 3, value: translate('Scene'), hint: 'Platform1-File01-File02-File03' },
    { key: 4, value: translate('Range'), hint: 'Platform1-File01-03' },
    { key: 5, value: translate('PrefixedRange'), hint: 'Platform1-File01-File03' },
  ];

  const colonReplacementOptions: EnhancedSelectInputValue<number>[] = [
    { key: 0, value: translate('Delete') },
    { key: 1, value: translate('ReplaceWithDash') },
    { key: 2, value: translate('ReplaceWithSpaceDash') },
    { key: 3, value: translate('ReplaceWithSpaceDashSpace') },
    {
      key: 4,
      value: translate('SmartReplace'),
      hint: translate('SmartReplaceHint'),
    },
    {
      key: 5,
      value: translate('Custom'),
      hint: translate('CustomColonReplacementFormatHint'),
    },
  ];

  const standardGameFileFormatHelpTexts = [];
  const standardGameFileFormatErrors = [];
  const gameFolderFormatHelpTexts = [];
  const gameFolderFormatErrors = [];
  const platformFolderFormatHelpTexts = [];
  const platformFolderFormatErrors = [];

  if (examplesPopulated) {
    if (examples.singleGameFileExample) {
      standardGameFileFormatHelpTexts.push(
        `${translate('SingleFile')}: ${examples.singleGameFileExample}`
      );
    } else {
      standardGameFileFormatErrors.push({
        message: translate('SingleFileInvalidFormat'),
      });
    }

    if (examples.multiGameFileExample) {
      standardGameFileFormatHelpTexts.push(
        `${translate('MultiFile')}: ${examples.multiGameFileExample}`
      );
    } else {
      standardGameFileFormatErrors.push({
        message: translate('MultiFileInvalidFormat'),
      });
    }

    if (examples.gameFolderExample) {
      gameFolderFormatHelpTexts.push(
        `${translate('Example')}: ${examples.gameFolderExample}`
      );
    } else {
      gameFolderFormatErrors.push({ message: translate('InvalidFormat') });
    }

    if (examples.platformFolderExample) {
      platformFolderFormatHelpTexts.push(
        `${translate('Example')}: ${examples.platformFolderExample}`
      );
    } else {
      platformFolderFormatErrors.push({ message: translate('InvalidFormat') });
    }
  }

  useEffect(() => {
    onChildStateChange({
      hasPendingChanges,
      isSaving,
    });
  }, [hasPendingChanges, isSaving, onChildStateChange]);

  useEffect(() => {
    setChildSave(saveSettings);
  }, [setChildSave, saveSettings]);

  return (
    <FieldSet legend={translate('FileNaming')}>
      {isFetching ? <LoadingIndicator /> : null}

      {!isFetching && error ? (
        <Alert kind={kinds.DANGER}>
          {translate('NamingSettingsLoadError')}
        </Alert>
      ) : null}

      {hasSettings && !isFetching && !error ? (
        <Form>
          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('RenameRoms')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="renameRoms"
              helpText={translate('RenameRomsHelpText')}
              onChange={handleInputChange}
              {...settings.renameRoms}
            />
          </FormGroup>

          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('ReplaceIllegalCharacters')}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="replaceIllegalCharacters"
              helpText={translate('ReplaceIllegalCharactersHelpText')}
              onChange={handleInputChange}
              {...settings.replaceIllegalCharacters}
            />
          </FormGroup>

          {replaceIllegalCharacters ? (
            <FormGroup size={sizes.MEDIUM}>
              <FormLabel>{translate('ColonReplacement')}</FormLabel>

              <FormInputGroup
                type={inputTypes.SELECT}
                name="colonReplacementFormat"
                values={colonReplacementOptions}
                helpText={translate('ColonReplacementFormatHelpText')}
                onChange={handleInputChange}
                {...settings.colonReplacementFormat}
              />
            </FormGroup>
          ) : null}

          {replaceIllegalCharacters &&
          settings.colonReplacementFormat.value === 5 ? (
            <FormGroup size={sizes.MEDIUM}>
              <FormLabel>{translate('CustomColonReplacement')}</FormLabel>

              <FormInputGroup
                type={inputTypes.TEXT}
                name="customColonReplacementFormat"
                helpText={translate('CustomColonReplacementFormatHelpText')}
                onChange={handleInputChange}
                {...settings.customColonReplacementFormat}
              />
            </FormGroup>
          ) : null}

          {renameRoms ? (
            <>
              <FormGroup size={sizes.LARGE}>
                <FormLabel>{translate('StandardGameFileFormat')}</FormLabel>

                <FormInputGroup
                  inputClassName={styles.namingInput}
                  type={inputTypes.TEXT}
                  name="standardGameFileFormat"
                  buttons={
                    <FormInputButton
                      onPress={handleStandardNamingModalOpenClick}
                    >
                      ?
                    </FormInputButton>
                  }
                  onChange={handleInputChange}
                  {...settings.standardGameFileFormat}
                  helpTexts={standardGameFileFormatHelpTexts}
                  errors={[
                    ...standardGameFileFormatErrors,
                    ...settings.standardGameFileFormat.errors,
                  ]}
                />
              </FormGroup>
            </>
          ) : null}

          <FormGroup
            advancedSettings={advancedSettings}
            isAdvanced={true}
            size={sizes.MEDIUM}
          >
            <FormLabel>{translate('GameFolderFormat')}</FormLabel>

            <FormInputGroup
              inputClassName={styles.namingInput}
              type={inputTypes.TEXT}
              name="gameFolderFormat"
              buttons={
                <FormInputButton onPress={handleGameFolderNamingModalOpenClick}>
                  ?
                </FormInputButton>
              }
              onChange={handleInputChange}
              {...settings.gameFolderFormat}
              helpTexts={[
                translate('GameFolderFormatHelpText'),
                ...gameFolderFormatHelpTexts,
              ]}
              errors={[
                ...gameFolderFormatErrors,
                ...settings.gameFolderFormat.errors,
              ]}
            />
          </FormGroup>

          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('PlatformFolderFormat')}</FormLabel>

            <FormInputGroup
              inputClassName={styles.namingInput}
              type={inputTypes.TEXT}
              name="platformFolderFormat"
              buttons={
                <FormInputButton
                  onPress={handlePlatformFolderNamingModalOpenClick}
                >
                  ?
                </FormInputButton>
              }
              onChange={handleInputChange}
              {...settings.platformFolderFormat}
              helpTexts={platformFolderFormatHelpTexts}
              errors={[
                ...platformFolderFormatErrors,
                ...settings.platformFolderFormat.errors,
              ]}
            />
          </FormGroup>

          <FormGroup size={sizes.MEDIUM}>
            <FormLabel>{translate('MultiFileStyle')}</FormLabel>

            <FormInputGroup
              type={inputTypes.SELECT}
              name="multiGameFileStyle"
              values={multiGameFileStyleOptions}
              onChange={handleInputChange}
              {...settings.multiGameFileStyle}
            />
          </FormGroup>

          {namingModalOptions ? (
            <NamingModal
              isOpen={isNamingModalOpen}
              {...namingModalOptions}
              value={settings[namingModalOptions.name].value}
              onInputChange={handleInputChange}
              onModalClose={setNamingModalClosed}
            />
          ) : null}
        </Form>
      ) : null}
    </FieldSet>
  );
}

export default Naming;
