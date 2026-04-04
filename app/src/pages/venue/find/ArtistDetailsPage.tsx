import { getRouteApi } from "@tanstack/react-router";
import { useArtistQuery } from "@/hooks/query/useArtistQuery";
import { ArtistDetails } from "@/components/artist/ArtistDetails";

const routeApi = getRouteApi("/venue/find/artist/$id");

export default function ArtistDetailsPage() {
  const { id } = routeApi.useParams();
  const { data: artist, isLoading, isError } = useArtistQuery(Number(id));

  if (isLoading)
    return <div className="text-muted-foreground p-6">Loading...</div>;
  if (isError || !artist)
    return <div className="text-destructive p-6">Artist not found.</div>;

  return <ArtistDetails artist={artist} />;
}
