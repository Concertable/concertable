import { type ReactNode } from "react";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { Button } from "@/components/ui/button";
import useEmblaCarousel from "embla-carousel-react";

interface Props<T> {
  title: string;
  items: T[];
  renderItem: (item: T) => ReactNode;
}

export function HeaderCarousel<T>({
  title,
  items,
  renderItem,
}: Readonly<Props<T>>) {
  const [emblaRef, emblaApi] = useEmblaCarousel({ dragFree: true });

  if (items.length === 0) return null;

  return (
    <div className="space-y-3">
      <h2 className="text-lg font-semibold">{title}</h2>
      <div className="grid grid-cols-[auto_1fr_auto] items-center gap-2">
        <Button
          variant="outline"
          size="icon"
          onClick={() => emblaApi?.scrollPrev()}
        >
          <ChevronLeft className="size-4" />
        </Button>
        <div className="overflow-hidden" ref={emblaRef}>
          <div className="flex gap-4">
            {items.map((item, i) => (
              <div key={i} className="w-[220px] shrink-0">
                {renderItem(item)}
              </div>
            ))}
          </div>
        </div>
        <Button
          variant="outline"
          size="icon"
          onClick={() => emblaApi?.scrollNext()}
        >
          <ChevronRight className="size-4" />
        </Button>
      </div>
    </div>
  );
}
