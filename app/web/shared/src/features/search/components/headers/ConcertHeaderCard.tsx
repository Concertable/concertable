import dayjs from "dayjs";
import { HeaderCard } from "./HeaderCard";
import { GenreTags } from "./GenreTags";
import type { ConcertHeader } from "../../types";

interface Props {
  data: ConcertHeader;
}

export function ConcertHeaderCard({ data }: Readonly<Props>) {
  return (
    <HeaderCard data={data} to={`/find/concert/${data.id}`}>
      <p className="text-muted-foreground mt-1 text-xs">
        {dayjs(data.startDate).format("DD/MM/YYYY")} –{" "}
        {dayjs(data.endDate).format("DD/MM/YYYY")}
      </p>
      <GenreTags genres={data.genres} />
    </HeaderCard>
  );
}
