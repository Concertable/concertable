import { useArtistQuery } from "../hooks/useArtistQuery";
import { ArtistDetails } from "../components/ArtistDetails";
import { DetailsPageSkeleton } from "@/components/skeletons/DetailsPageSkeleton";

interface Props {
  id: number;
}

export function ArtistDetailsPage({ id }: Props) {
  const { data: artist, isLoading, isError } = useArtistQuery(id);

  if (isLoading) return <DetailsPageSkeleton sections={5} />;
  if (isError || !artist)
    return <div className="text-destructive p-6">Artist not found.</div>;

  return <ArtistDetails artist={artist} />;
}
