import type { ArtistHeader } from "@/types/header";
import { HeaderCard } from "@/components/headers/HeaderCard";

interface Props {
  data: ArtistHeader;
}

export function ArtistHeaderCard({ data }: Readonly<Props>) {
  return <HeaderCard data={data} to={`/find/artist/${data.id}`} />;
}
