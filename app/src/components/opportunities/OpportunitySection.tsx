import { usePagination } from "@/hooks/usePagination";
import { useOpportunitiesByVenueQuery } from "@/hooks/query/useOpportunityQuery";
import { OpportunityCard } from "@/components/opportunities/OpportunityCard";
import { PaginationControls } from "@/components/ui/PaginationControls";

interface Props {
  venueId: number;
}

export function OpportunitySection({ venueId }: Readonly<Props>) {
  const { params, nextPage, prevPage } = usePagination();
  const { data, isLoading, isError } = useOpportunitiesByVenueQuery(venueId, params);

  if (isLoading) return <p className="text-muted-foreground">Loading opportunities...</p>;
  if (isError) return <p className="text-destructive">Failed to load opportunities.</p>;
  if (!data?.data.length) return <p className="text-muted-foreground">No opportunities yet.</p>;

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
