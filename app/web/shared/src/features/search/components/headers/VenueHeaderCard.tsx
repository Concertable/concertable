import { HeaderCard } from "./HeaderCard";
import type { VenueHeader } from "../../types";

interface Props {
  data: VenueHeader;
}

export function VenueHeaderCard({ data }: Readonly<Props>) {
  return <HeaderCard data={data} to={`/find/venue/${data.id}`} />;
}
