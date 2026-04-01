interface FileSearchPayload {
  romId: number;
}

interface SeasonSearchPayload {
  gameId: number;
  platformNumber: number;
}

type InteractiveSearchPayload = FileSearchPayload | SeasonSearchPayload;

export default InteractiveSearchPayload;
