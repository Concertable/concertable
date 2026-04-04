import type { ConcertHeader } from "@/types/header";
import { HeaderCarousel } from "@/components/headers/HeaderCarousel";
import { ConcertHeaderCard } from "@/components/headers/ConcertHeaderCard";
import { useHeaderAmountQuery } from "@/hooks/query/useHeaderQuery";

interface Props {
  title: string;
  amount?: number;
}

export function ConcertHeaderCarousel({ title, amount = 15 }: Readonly<Props>) {
  const { data = [] } = useHeaderAmountQuery("Concert", amount);

  return (
    <HeaderCarousel
      title={title}
      items={data as ConcertHeader[]}
      renderItem={(item) => <ConcertHeaderCard data={item} />}
    />
  );
}
