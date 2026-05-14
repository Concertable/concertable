import { CarouselSkeleton } from "@/components/skeletons/CarouselSkeleton";
import { useHeaderAmountQuery } from "../../hooks/useHeaderQuery";
import { HeaderCarousel } from "./HeaderCarousel";
import { ArtistHeaderCard } from "./ArtistHeaderCard";
import type { ArtistHeader } from "../../types";

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
