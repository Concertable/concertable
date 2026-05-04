import { Button } from "@/components/ui/button";
import { Plus } from "lucide-react";
import { Skeleton } from "@/components/ui/skeleton";
import { defaultContract } from "@/features/contracts";
import { useOpportunities } from "../../hooks/useOpportunities";
import { OpportunityCard } from "./OpportunityCard";
import dayjs from "dayjs";
import type { OpportunityDraft } from "../../types";

interface Props {
  venueId: number;
}

function buildDraft(): OpportunityDraft {
  const start = dayjs().add(7, "day").hour(19).minute(0).second(0).millisecond(0);
  const end = start.add(4, "hour");
  return {
    startDate: start.toISOString(),
    endDate: end.toISOString(),
    genres: [],
    contract: defaultContract("flatFee"),
  };
}

export function OpportunityList({ venueId }: Readonly<Props>) {
  const { opportunities, isLoading, add, update, remove } =
    useOpportunities(venueId);

  if (isLoading) return <OpportunityListSkeleton />;

  return (
    <div className="space-y-3">
      {opportunities.map((opportunity, index) => (
        <OpportunityCard
          key={
            "id" in opportunity ? `existing-${opportunity.id}` : `draft-${index}`
          }
          opportunity={opportunity}
          onUpdate={(next) => update(index, next)}
          onRemove={() => remove(index)}
        />
      ))}

      <Button
        variant="outline"
        onClick={() => add(buildDraft())}
        data-testid="opportunity-add"
      >
        <Plus className="mr-1.5 size-4" />
        Add opportunity
      </Button>
    </div>
  );
}

function OpportunityListSkeleton() {
  return (
    <div className="space-y-3">
      {Array.from({ length: 3 }).map((_, i) => (
        <Skeleton key={i} className="h-24 w-full rounded-xl" />
      ))}
    </div>
  );
}
