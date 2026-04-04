import type { ArtistHeader } from "@/types/header";
import { HeaderCarousel } from "@/components/headers/HeaderCarousel";
import { ArtistHeaderCard } from "@/components/headers/ArtistHeaderCard";
import { useHeaderAmountQuery } from "@/hooks/query/useHeaderQuery";

interface Props {
  title: string;
  amount?: number;
}

export function ArtistHeaderCarousel({ title, amount = 15 }: Readonly<Props>) {
  const { data = [] } = useHeaderAmountQuery("artist", amount);

  return (
    <HeaderCarousel
      title={title}
      items={data as ArtistHeader[]}
      renderItem={(item) => <ArtistHeaderCard data={item} />}
    />
  );
}
