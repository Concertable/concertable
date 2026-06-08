import { DetailsLayout } from "@/components/details/DetailsLayout";
import { DetailsPageSkeleton } from "@/components/skeletons/DetailsPageSkeleton";
import { useArtistQuery } from "../hooks/useArtistQuery";
import { ArtistHero } from "../components/ArtistHero";
import { artistSections } from "../artistSections";

interface Props {
  id: number;
}

export function ArtistDetailsPage({ id }: Readonly<Props>) {
  const { data: artist, isLoading, isError } = useArtistQuery(id);

  if (isLoading) return <DetailsPageSkeleton sections={4} />;
  if (isError || !artist)
    return <div className="text-destructive p-6">Artist not found.</div>;

  const hero = <ArtistHero artist={artist} />;
  const { about, location, concerts, reviews } = artistSections(artist);
  const sections = [about, location, concerts, reviews];

  return <DetailsLayout hero={hero} sections={sections} />;
}
