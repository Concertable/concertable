import type { ConcertHeader } from "@/types/header";
import { HeaderCarousel } from "@/components/headers/HeaderCarousel";
import { ConcertHeaderCard } from "@/components/headers/ConcertHeaderCard";
import { useHeaderAmountQuery } from "@/hooks/query/useHeaderQuery";
import { CarouselSkeleton } from "@/components/skeletons/CarouselSkeleton";

interface Props {
  title: string;
  amount?: number;
}

export function ConcertHeaderCarousel({ title, amount = 15 }: Readonly<Props>) {
  const { data = [], isLoading } = useHeaderAmountQuery("Concert", amount);

  if (isLoading) return <CarouselSkeleton title={title} />;

  return (
    <HeaderCarousel
      title={title}
      items={data as ConcertHeader[]}
      renderItem={(item) => <ConcertHeaderCard data={item} />}
    />
  );
}
