import { CarouselSkeleton } from "@/components/skeletons/CarouselSkeleton";
import { useHeaderAmountQuery } from "../../hooks/useHeaderQuery";
import { HeaderCarousel } from "./HeaderCarousel";
import { VenueHeaderCard } from "./VenueHeaderCard";
import type { VenueHeader } from "../../types";

interface Props {
  title: string;
  amount?: number;
}

export function VenueHeaderCarousel({ title, amount = 15 }: Readonly<Props>) {
  const { data = [], isLoading } = useHeaderAmountQuery("venue", amount);

  if (isLoading) return <CarouselSkeleton title={title} />;

  return (
    <HeaderCarousel
      title={title}
      items={data as VenueHeader[]}
      renderItem={(item) => <VenueHeaderCard data={item} />}
    />
  );
}
