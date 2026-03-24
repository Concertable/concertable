import type { ArtistHeader } from "@/types/header";
import { HeaderCard } from "@/components/headers/HeaderCard";

interface Props {
  data: ArtistHeader;
}

export function ArtistHeaderCard({ data }: Props) {
  return <HeaderCard data={data} />;
}
