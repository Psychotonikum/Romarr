import React from 'react';
import Modal from 'Components/Modal/Modal';
import SelectRomModalContent, {
  SelectedFile,
} from './SelectRomModalContent';

interface SelectRomModalProps {
  isOpen: boolean;
  selectedIds: number[] | string[];
  gameId?: number;
  platformNumber?: number;
  selectedDetails?: string;
  modalTitle: string;
  onFilesSelect(selectedFiles: SelectedFile[]): void;
  onModalClose(): void;
}

function SelectRomModal(props: SelectRomModalProps) {
  const {
    isOpen,
    selectedIds,
    gameId,
    platformNumber,
    selectedDetails,
    modalTitle,
    onFilesSelect,
    onModalClose,
  } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <SelectRomModalContent
        selectedIds={selectedIds}
        gameId={gameId}
        platformNumber={platformNumber}
        selectedDetails={selectedDetails}
        modalTitle={modalTitle}
        onFilesSelect={onFilesSelect}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default SelectRomModal;
