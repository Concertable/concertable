import { useState } from "react";
import type { ReactNode } from "react";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import { Editable } from "@/components/editable/Editable";
import { DateRangeField } from "@/components/datetime/DateRangeField";
import {
  ContractDetails,
  ContractFields,
  ContractSummaryLabel,
  CONTRACT_TYPE_LABELS,
} from "@/features/contracts";
import { useApply } from "../../hooks/useApply";
import { useNavigate } from "@tanstack/react-router";
import { useAuthStore, isVenueManager } from "@/features/auth";
import { useGenresQuery } from "@/features/search/hooks/useGenreQuery";
import { X } from "lucide-react";
import dayjs from "dayjs";
import type { Opportunity, OpportunityDraft } from "../../types";
import type { Contract, PaymentMethod } from "@/features/contracts";
import type { Genre } from "@/types/common";

interface EditCallbacks {
  onRemove: () => void;
  onSetDates: (start: string, end: string) => void;
  onSetContractType: (type: Contract["$type"]) => void;
  onSetContract: (contract: Contract) => void;
  onSetPaymentMethod: (method: PaymentMethod) => void;
  onToggleGenre: (genre: Genre) => void;
}

// Persisted opportunity — toggles between read and edit view
interface OpportunityCardProps extends EditCallbacks {
  opportunity: Opportunity;
}

export function OpportunityCard({ opportunity, ...callbacks }: Readonly<OpportunityCardProps>) {
  return (
    <Editable
      view={<ReadView opportunity={opportunity} />}
      edit={<EditView opportunity={opportunity} {...callbacks} />}
    />
  );
}

// Unsaved draft — toggles between a simple read view and edit view
interface DraftOpportunityCardProps extends EditCallbacks {
  draft: OpportunityDraft;
}

export function DraftOpportunityCard({ draft, ...callbacks }: Readonly<DraftOpportunityCardProps>) {
  return (
    <Editable
      view={<DraftReadView draft={draft} />}
      edit={<EditView opportunity={draft} {...callbacks} />}
    />
  );
}

function OpportunityRead({ opportunity, actions }: { opportunity: OpportunityDraft; actions?: ReactNode }) {
  return (
    <>
      <div className="flex items-start justify-between gap-4">
        <div className="space-y-1">
          <p className="text-muted-foreground text-sm">
            {dayjs(opportunity.startDate).format("D MMM YYYY")} —{" "}
            {dayjs(opportunity.endDate).format("D MMM YYYY")}
          </p>
          <ContractSummaryLabel contract={opportunity.contract} />
        </div>
        {actions && <div className="flex shrink-0 gap-2">{actions}</div>}
      </div>

      {opportunity.genres.length > 0 && (
        <div className="flex flex-wrap gap-1.5">
          {opportunity.genres.map((genre) => (
            <span
              key={genre.id}
              className="bg-muted text-muted-foreground rounded-full px-2.5 py-0.5 text-xs"
            >
              {genre.name}
            </span>
          ))}
        </div>
      )}
    </>
  );
}

function DraftReadView({ draft }: { draft: OpportunityDraft }) {
  return (
    <div className="border-border bg-card space-y-3 rounded-xl border p-4">
      <OpportunityRead opportunity={draft} />
    </div>
  );
}

function ReadView({ opportunity }: { opportunity: Opportunity }) {
  const [open, setOpen] = useState(false);
  const { apply, isPending, error, canApply } = useApply(opportunity.id);
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);
  const isArtistManager = user?.role === "ArtistManager";

  return (
    <>
      <div
        className="border-border bg-card space-y-3 rounded-xl border p-4"
        data-testid={`opportunity-${opportunity.id}`}
      >
        <OpportunityRead
          opportunity={opportunity}
          actions={
            <>
              <Button variant="outline" size="sm" onClick={() => setOpen(true)}>
                View Contract
              </Button>
              {user && isVenueManager(user) && user.venueId === opportunity.venueId ? (
                <Button
                  size="sm"
                  onClick={() =>
                    navigate({
                      to: "/venue/my/applications/$id",
                      params: { id: opportunity.id },
                    })
                  }
                >
                  View Applications
                </Button>
              ) : (
                isArtistManager && (
                  <Button
                    size="sm"
                    disabled={!canApply || isPending}
                    data-testid="apply"
                    onClick={() =>
                      opportunity.actions.checkout != null
                        ? navigate({
                            to: "/artist/opportunity/checkout/$opportunityId",
                            params: { opportunityId: opportunity.id },
                          })
                        : apply()
                    }
                  >
                    {isPending ? "Applying..." : "Apply"}
                  </Button>
                )
              )}
            </>
          }
        />
        {error && <p className="text-destructive text-sm">{error.message}</p>}
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

interface EditViewProps extends EditCallbacks {
  opportunity: OpportunityDraft;
}

function EditView({ opportunity, onRemove, onSetDates, onSetContractType, onSetContract, onSetPaymentMethod, onToggleGenre }: EditViewProps) {
  const { data: genres } = useGenresQuery();
  const contract = opportunity.contract;

  return (
    <div
      className="border-border bg-card space-y-4 rounded-xl border p-4"
      data-testid="opportunity-card-edit"
    >
      <div className="flex items-start justify-between gap-4">
        <div className="flex-1 space-y-3">
          <DateRangeField
            startDate={opportunity.startDate}
            endDate={opportunity.endDate}
            onChange={onSetDates}
          />

          <div>
            <Label className="text-muted-foreground text-xs">Contract type</Label>
            <Select
              value={contract.$type}
              onValueChange={(v) => onSetContractType(v as Contract["$type"])}
            >
              <SelectTrigger data-testid="opportunity-contract-type">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {(Object.keys(CONTRACT_TYPE_LABELS) as Contract["$type"][]).map((type) => (
                  <SelectItem key={type} value={type}>
                    {CONTRACT_TYPE_LABELS[type]}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <ContractFields contract={contract} onChange={onSetContract} />

          <div>
            <Label className="text-muted-foreground text-xs">Payment method</Label>
            <Select
              value={contract.paymentMethod}
              onValueChange={(v) => onSetPaymentMethod(v as PaymentMethod)}
            >
              <SelectTrigger data-testid="opportunity-payment-method">
                <SelectValue placeholder="Select" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="Cash">Cash</SelectItem>
                <SelectItem value="Transfer">Transfer</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div>
            <Label className="text-muted-foreground text-xs">Genres</Label>
            <div className="flex flex-wrap gap-2 pt-1">
              {genres?.map((genre) => {
                const checked = opportunity.genres.some((g) => g.id === genre.id);
                return (
                  <label
                    key={genre.id}
                    className="bg-muted flex cursor-pointer items-center gap-1.5 rounded-full px-2.5 py-1 text-xs"
                    data-testid={`opportunity-genre-${genre.name.toLowerCase()}`}
                  >
                    <Checkbox
                      checked={checked}
                      onCheckedChange={() => onToggleGenre(genre)}
                    />
                    {genre.name}
                  </label>
                );
              })}
            </div>
          </div>
        </div>

        <Button
          variant="ghost"
          size="icon"
          onClick={onRemove}
          aria-label="Remove opportunity"
          data-testid="opportunity-remove"
        >
          <X className="size-4" />
        </Button>
      </div>
    </div>
  );
}
