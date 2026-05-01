import { usePagination } from "@/hooks/usePagination";
import { PaginationControls } from "@/components/ui/PaginationControls";
import { Skeleton } from "@/components/ui/skeleton";
import { useOpportunitiesByVenueQuery } from "../../hooks/useOpportunityQuery";
import { OpportunityCard } from "./OpportunityCard";

function OpportunitySectionSkeleton() {
  return (
    <div className="space-y-3">
      {Array.from({ length: 3 }).map((_, i) => (
        <div
          key={i}
          className="border-border bg-card space-y-3 rounded-xl border p-4"
        >
          <div className="flex items-start justify-between gap-4">
            <div className="space-y-1.5">
              <Skeleton className="h-4 w-48" />
              <Skeleton className="h-4 w-32" />
            </div>
            <div className="flex gap-2">
              <Skeleton className="h-8 w-28" />
              <Skeleton className="h-8 w-24" />
            </div>
          </div>
          <div className="flex gap-1.5">
            <Skeleton className="h-5 w-16 rounded-full" />
            <Skeleton className="h-5 w-20 rounded-full" />
          </div>
        </div>
      ))}
    </div>
  );
}

interface Props {
  venueId: number;
}

export function OpportunitySection({ venueId }: Readonly<Props>) {
  const { params, nextPage, prevPage } = usePagination();
  const { data, isLoading, isError } = useOpportunitiesByVenueQuery(
    venueId,
    params,
  );

  if (isLoading) return <OpportunitySectionSkeleton />;
  if (isError)
    return <p className="text-destructive">Failed to load opportunities.</p>;
  if (!data?.data.length)
    return <p className="text-muted-foreground">No opportunities yet.</p>;

  return (
    <div className="space-y-3">
      {data.data.map((opportunity) => (
        <OpportunityCard key={opportunity.id} opportunity={opportunity} />
      ))}

      <PaginationControls
        pageNumber={params.pageNumber}
        totalPages={data.totalPages}
        onPrev={prevPage}
        onNext={nextPage}
      />
    </div>
  );
}
