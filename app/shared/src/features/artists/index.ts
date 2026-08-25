export {
  useArtistQuery,
  useArtistByIdQuery,
  useMyArtistQuery,
  artistKeys,
} from "./hooks/useArtistQuery";
export { useArtist } from "./hooks/useArtist";
export type { UseArtistResult } from "./hooks/useArtist";
export { useArtistById } from "./hooks/useArtistById";
export type { UseArtistByIdResult } from "./hooks/useArtistById";
export { useMyArtist } from "./hooks/useMyArtist";
export type { UseMyArtistOptions, UseMyArtistResult } from "./hooks/useMyArtist";
export { useCreateArtist } from "./hooks/useCreateArtist";
export type {
  UseCreateArtistOptions,
  UseCreateArtistResult,
} from "./hooks/useCreateArtist";
export { Artist } from "./types";
export type {
  ArtistSummary,
  CreateArtistRequest,
  UpdateArtistRequest,
} from "./types";
