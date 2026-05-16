import { CarouselSkeleton } from "@/components/skeletons/CarouselSkeleton";
import { useHeaderAmountQuery } from "../../hooks/useHeaderQuery";
import { HeaderCarousel } from "./HeaderCarousel";
import { ConcertHeaderCard } from "./ConcertHeaderCard";
import type { ConcertHeader } from "../../types";

interface Props {
  title: string;
  amount?: number;
}

export function ConcertHeaderCarousel({ title, amount = 15 }: Readonly<Props>) {
  const { data = [], isLoading } = useHeaderAmountQuery("concert", amount);

  if (isLoading) return <CarouselSkeleton title={title} />;

  return (
    <HeaderCarousel
      title={title}
      items={data as ConcertHeader[]}
      renderItem={(item) => <ConcertHeaderCard data={item} />}
    />
  );
}
