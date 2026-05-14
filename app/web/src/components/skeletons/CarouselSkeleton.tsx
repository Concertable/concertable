import { Skeleton } from "@/components/ui/skeleton";
import { HeaderCardSkeleton } from "@/components/skeletons/HeaderCardSkeleton";

interface Props {
  title: string;
}

export function CarouselSkeleton({ title }: Readonly<Props>) {
  return (
    <div className="space-y-3">
      <h2 className="text-lg font-semibold">{title}</h2>
      <div className="grid grid-cols-[auto_1fr_auto] items-center gap-2">
        <Skeleton className="size-9 rounded-md" />
        <div className="flex gap-4 overflow-hidden">
          {Array.from({ length: 5 }).map((_, i) => (
            <div key={i} className="w-[220px] shrink-0">
              <HeaderCardSkeleton />
            </div>
          ))}
        </div>
        <Skeleton className="size-9 rounded-md" />
      </div>
    </div>
  );
}
