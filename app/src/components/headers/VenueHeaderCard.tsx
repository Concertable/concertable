import type { VenueHeader } from "@/types/header";
import { HeaderCard } from "@/components/headers/HeaderCard";

interface Props {
  data: VenueHeader;
}

export function VenueHeaderCard({ data }: Readonly<Props>) {
  return <HeaderCard data={data} to={`/find/venue/${data.id}`} />;
}
