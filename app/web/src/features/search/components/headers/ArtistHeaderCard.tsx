import { HeaderCard } from "./HeaderCard";
import { GenreTags } from "./GenreTags";
import type { ArtistHeader } from "../../types";

interface Props {
  data: ArtistHeader;
}

export function ArtistHeaderCard({ data }: Readonly<Props>) {
  return (
    <HeaderCard data={data} to={`/find/artist/${data.id}`}>
      <GenreTags genres={data.genres} />
    </HeaderCard>
  );
}
