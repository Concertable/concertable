import type { VenueHeader } from "@/types/header";
import { HeaderCard } from "@/components/headers/HeaderCard";

interface Props {
  data: VenueHeader;
}

export function VenueHeaderCard({ data }: Props) {
  return <HeaderCard data={data} />;
}
