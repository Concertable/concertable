import { useRef, type ReactNode } from "react";
import { ChevronLeft, ChevronRight } from "lucide-react";
import { Button } from "@/components/ui/button";

interface Props<T> {
  title: string;
  items: T[];
  renderItem: (item: T) => ReactNode;
}

export function HeaderCarousel<T>({ title, items, renderItem }: Readonly<Props<T>>) {
  const scrollRef = useRef<HTMLDivElement>(null);

  if (items.length === 0) return null;

  function scroll(dir: "left" | "right") {
    scrollRef.current?.scrollBy({ left: dir === "left" ? -600 : 600, behavior: "smooth" });
  }

  return (
    <div className="space-y-3">
      <h2 className="text-lg font-semibold">{title}</h2>
      <div className="grid grid-cols-[auto_1fr_auto] items-center gap-2">
        <Button variant="outline" size="icon" onClick={() => scroll("left")}>
          <ChevronLeft className="size-4" />
        </Button>
        <div
          ref={scrollRef}
          className="flex gap-4 overflow-x-auto scroll-smooth [&::-webkit-scrollbar]:hidden [-ms-overflow-style:none] [scrollbar-width:none]"
        >
          {items.map((item, i) => (
            <div key={i} className="shrink-0 w-[220px]">
              {renderItem(item)}
            </div>
          ))}
        </div>
        <Button variant="outline" size="icon" onClick={() => scroll("right")}>
          <ChevronRight className="size-4" />
        </Button>
      </div>
    </div>
  );
}
