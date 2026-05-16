import { Skeleton } from "@/components/ui/skeleton";

interface Props {
  sections?: number;
}

export function DetailsPageSkeleton({ sections = 4 }: Readonly<Props>) {
  return (
    <div>
      {/* Hero */}
      <Skeleton className="h-72 w-full rounded-none" />

      {/* ScrollspyNav */}
      <div className="border-border border-b">
        <div className="mx-auto max-w-6xl px-6">
          <div className="flex justify-center gap-6 py-3">
            {Array.from({ length: sections }).map((_, i) => (
              <Skeleton key={i} className="h-4 w-16" />
            ))}
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="mx-auto max-w-4xl space-y-10 px-6 py-10">
        {Array.from({ length: sections }).map((_, i) => (
          <div key={i}>
            <div className="space-y-3">
              <Skeleton className="h-6 w-32" />
              <Skeleton className="h-4 w-full" />
              <Skeleton className="h-4 w-3/4" />
            </div>
            {i < sections - 1 && (
              <div className="border-border mt-10 border-t" />
            )}
          </div>
        ))}
      </div>
    </div>
  );
}
