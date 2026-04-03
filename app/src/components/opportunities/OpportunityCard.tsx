import { useState } from "react";
import type { Opportunity } from "@/types/opportunity";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { ContractDetails } from "@/components/opportunities/ContractDetails";
import { ContractSummaryLabel } from "@/components/opportunities/ContractSummaryLabel";
import { useApply } from "@/hooks/useApply";
import { useNavigate } from "@tanstack/react-router";
import { useAuthStore } from "@/store/useAuthStore";
import { isVenueManager } from "@/types/auth";
import dayjs from "dayjs";

interface Props {
  opportunity: Opportunity;
}

export function OpportunityCard({ opportunity }: Readonly<Props>) {
  const [open, setOpen] = useState(false);
  const { apply, isPending, error, canApply } = useApply(opportunity.id);
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);

  return (
    <>
      <div className="rounded-xl border border-border bg-card p-4 space-y-3">
        <div className="flex items-start justify-between gap-4">
          <div className="space-y-1">
            <p className="text-sm text-muted-foreground">
              {dayjs(opportunity.startDate).format("D MMM YYYY")} — {dayjs(opportunity.endDate).format("D MMM YYYY")}
            </p>
            <ContractSummaryLabel contract={opportunity.contract} />
          </div>
          <div className="flex gap-2 shrink-0">
            <Button variant="outline" size="sm" onClick={() => setOpen(true)}>
              View Contract
            </Button>
            {user && isVenueManager(user) && user.venueId === opportunity.venueId ? (
              <Button size="sm" onClick={() => navigate({ to: "/venue/my/applications/$id", params: { id: opportunity.id } })}>
                View Applications
              </Button>
            ) : (
              canApply && (
                <Button size="sm" disabled={isPending} onClick={() => apply()}>
                  {isPending ? "Applying..." : "Apply"}
                </Button>
              )
            )}
          </div>
        </div>

        {error && (
          <p className="text-sm text-destructive">{error.message}</p>
        )}

        {opportunity.genres.length > 0 && (
          <div className="flex flex-wrap gap-1.5">
            {opportunity.genres.map((genre) => (
              <span
                key={genre.id}
                className="rounded-full bg-muted px-2.5 py-0.5 text-xs text-muted-foreground"
              >
                {genre.name}
              </span>
            ))}
          </div>
        )}
      </div>

      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Contract Details</DialogTitle>
          </DialogHeader>
          <ContractDetails contract={opportunity.contract} />
          <DialogFooter showCloseButton />
        </DialogContent>
      </Dialog>
    </>
  );
}
