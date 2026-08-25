export { ArtistHero } from "./components/ArtistHero";
export { ArtistDetailsPage } from "./pages/ArtistDetailsPage";
export { artistSections } from "./artistSections";
export {
  useArtistQuery,
  useArtistByIdQuery,
  useMyArtistQuery,
  artistKeys,
} from "./hooks/useArtistQuery";
export { useArtist, useArtistById } from "@concertable/shared/features/artists";
export { useArtistStore } from "./store/useArtistStore";
export type { Artist, ArtistSummary } from "./types";
