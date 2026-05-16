import { Skeleton } from "@/components/ui/skeleton";

export function HeaderCardSkeleton() {
  return (
    <div className="border-border bg-card flex w-full flex-col items-center gap-2 rounded-xl border p-4">
      <Skeleton className="h-4 w-24" />
      <Skeleton className="h-[180px] w-[180px] rounded-lg" />
      <Skeleton className="h-3 w-28" />
    </div>
  );
}
