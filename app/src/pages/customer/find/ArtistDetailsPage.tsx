import { getRouteApi } from "@tanstack/react-router";
import { useArtistQuery } from "@/hooks/query/useArtist";
import { ArtistDetails } from "@/components/artist/ArtistDetails";

const routeApi = getRouteApi("/_customer/find/artist/$id");

export default function ArtistDetailsPage() {
  const { id } = routeApi.useParams();
  const { data: artist, isLoading, isError } = useArtistQuery(Number(id));

  if (isLoading) return <div className="p-6 text-muted-foreground">Loading...</div>;
  if (isError || !artist) return <div className="p-6 text-destructive">Artist not found.</div>;

  return <ArtistDetails artist={artist} />;
}
