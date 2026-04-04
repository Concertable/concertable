import type { VenueHeader } from "@/types/header";
import { HeaderCarousel } from "@/components/headers/HeaderCarousel";
import { VenueHeaderCard } from "@/components/headers/VenueHeaderCard";
import { useHeaderAmountQuery } from "@/hooks/query/useHeaderQuery";
import { CarouselSkeleton } from "@/components/skeletons/CarouselSkeleton";

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
