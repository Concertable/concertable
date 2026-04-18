import dayjs from "dayjs";
import type { ConcertHeader } from "@/types/header";
import { HeaderCard } from "@/components/headers/HeaderCard";
import { GenreTags } from "@/components/headers/GenreTags";

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
