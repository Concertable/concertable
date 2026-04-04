import { getRouteApi } from "@tanstack/react-router";
import { useArtistQuery } from "@/hooks/query/useArtistQuery";
import { ArtistDetails } from "@/components/artist/ArtistDetails";
import { DetailsPageSkeleton } from "@/components/skeletons/DetailsPageSkeleton";

const routeApi = getRouteApi("/_customer/find/artist/$id");

export default function ArtistDetailsPage() {
  const { id } = routeApi.useParams();
  const { data: artist, isLoading, isError } = useArtistQuery(Number(id));

  if (isLoading) return <DetailsPageSkeleton sections={5} />;
  if (isError || !artist)
    return <div className="text-destructive p-6">Artist not found.</div>;

  return <ArtistDetails artist={artist} />;
}
