import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
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
  defaultContract,
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

interface Props {
  opportunity: Opportunity | OpportunityDraft;
  onUpdate?: (next: Opportunity | OpportunityDraft) => void;
  onRemove?: () => void;
}

export function OpportunityCard({
  opportunity,
  onUpdate,
  onRemove,
}: Readonly<Props>) {
  return (
    <Editable
      view={<ReadView opportunity={opportunity as Opportunity} />}
      edit={
        <EditView
          opportunity={opportunity}
          onUpdate={onUpdate}
          onRemove={onRemove}
        />
      }
    />
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
        <div className="flex items-start justify-between gap-4">
          <div className="space-y-1">
            <p className="text-muted-foreground text-sm">
              {dayjs(opportunity.startDate).format("D MMM YYYY")} —{" "}
              {dayjs(opportunity.endDate).format("D MMM YYYY")}
            </p>
            <ContractSummaryLabel contract={opportunity.contract} />
          </div>
          <div className="flex shrink-0 gap-2">
            <Button variant="outline" size="sm" onClick={() => setOpen(true)}>
              View Contract
            </Button>
            {user &&
            isVenueManager(user) &&
            user.venueId === opportunity.venueId ? (
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
          </div>
        </div>

        {error && <p className="text-destructive text-sm">{error.message}</p>}

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

interface EditViewProps {
  opportunity: Opportunity | OpportunityDraft;
  onUpdate?: (next: Opportunity | OpportunityDraft) => void;
  onRemove?: () => void;
}

function EditView({ opportunity, onUpdate, onRemove }: EditViewProps) {
  const { data: genres } = useGenresQuery();
  const contract = opportunity.contract;

  function patch(next: Partial<OpportunityDraft>) {
    onUpdate?.({ ...opportunity, ...next });
  }

  function setContract(next: Contract) {
    patch({ contract: next });
  }

  function changeContractType(type: Contract["$type"]) {
    setContract(defaultContract(type, contract.paymentMethod));
  }

  function setPaymentMethod(paymentMethod: PaymentMethod) {
    setContract({ ...contract, paymentMethod });
  }

  function toggleGenre(genreId: number) {
    const exists = opportunity.genres.some((g) => g.id === genreId);
    if (exists) {
      patch({ genres: opportunity.genres.filter((g) => g.id !== genreId) });
    } else {
      const genre = genres?.find((g) => g.id === genreId);
      if (genre) patch({ genres: [...opportunity.genres, genre] });
    }
  }

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
            onChange={(start, end) => patch({ startDate: start, endDate: end })}
          />

          <div>
            <Label className="text-muted-foreground text-xs">
              Contract type
            </Label>
            <Select
              value={contract.$type}
              onValueChange={(v) => changeContractType(v as Contract["$type"])}
            >
              <SelectTrigger data-testid="opportunity-contract-type">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {(Object.keys(CONTRACT_TYPE_LABELS) as Contract["$type"][]).map(
                  (type) => (
                    <SelectItem key={type} value={type}>
                      {CONTRACT_TYPE_LABELS[type]}
                    </SelectItem>
                  ),
                )}
              </SelectContent>
            </Select>
          </div>

          <ContractFields contract={contract} onChange={setContract} />

          <div>
            <Label className="text-muted-foreground text-xs">
              Payment method
            </Label>
            <Select
              value={contract.paymentMethod}
              onValueChange={(v) => setPaymentMethod(v as PaymentMethod)}
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
                const checked = opportunity.genres.some(
                  (g) => g.id === genre.id,
                );
                return (
                  <label
                    key={genre.id}
                    className="bg-muted flex cursor-pointer items-center gap-1.5 rounded-full px-2.5 py-1 text-xs"
                    data-testid={`opportunity-genre-${genre.name.toLowerCase()}`}
                  >
                    <Checkbox
                      checked={checked}
                      onCheckedChange={() => toggleGenre(genre.id)}
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

