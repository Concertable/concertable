import type { ArtistHeader } from "@/types/header";
import { HeaderCarousel } from "@/components/headers/HeaderCarousel";
import { ArtistHeaderCard } from "@/components/headers/ArtistHeaderCard";
import { useHeaderAmountQuery } from "@/hooks/query/useHeaderQuery";
import { CarouselSkeleton } from "@/components/skeletons/CarouselSkeleton";

interface Props {
  title: string;
  amount?: number;
}

export function ArtistHeaderCarousel({ title, amount = 15 }: Readonly<Props>) {
  const { data = [], isLoading } = useHeaderAmountQuery("artist", amount);

  if (isLoading) return <CarouselSkeleton title={title} />;

  return (
    <HeaderCarousel
      title={title}
      items={data as ArtistHeader[]}
      renderItem={(item) => <ArtistHeaderCard data={item} />}
    />
  );
}
