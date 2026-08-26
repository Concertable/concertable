import { useState } from "react";
import type { ReportCategory } from "../types";
import {
  REPORT_CATEGORY_LABELS,
  reportMessageRequestSchema,
  useReportMessage,
} from "@concertable/shared/features/messaging";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Select } from "@/components/Select";

interface CategoryOption {
  value: ReportCategory;
  label: string;
}

const categories: CategoryOption[] = reportMessageRequestSchema.shape.category.options.map(
  (value) => ({ value, label: REPORT_CATEGORY_LABELS[value] }),
);

interface Props {
  messageId: number;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function ReportMessageDialog({
  messageId,
  open,
  onOpenChange,
}: Readonly<Props>) {
  // No preselected reason: a default would let an untouched form file the most serious OSA category.
  const [category, setCategory] = useState<CategoryOption>();
  const [details, setDetails] = useState<string>();
  const { validate, submit, isPending, isSuccess, isError } =
    useReportMessage(messageId);

  const buffer = { category: category?.value, details };
  const parsed = validate(buffer);
  const detailsError = parsed.success
    ? undefined
    : parsed.error.issues.find((issue) => issue.path[0] === "details")?.message;

  // Closing mid-flight would unmount before the acknowledgement, leaving the reporter unsure whether
  // the report landed — and free to file a duplicate.
  const close = (next: boolean) => {
    if (isPending) return;
    onOpenChange(next);
  };

  return (
    <Dialog open={open} onOpenChange={close}>
      <DialogContent
        showCloseButton={!isPending}
        onEscapeKeyDown={(e) => isPending && e.preventDefault()}
        onInteractOutside={(e) => isPending && e.preventDefault()}
      >
        <DialogHeader>
          <DialogTitle>Report this message</DialogTitle>
          <DialogDescription>
            Tell us what is wrong with this message. We review every report and
            will email you a reference.
          </DialogDescription>
        </DialogHeader>

        {isSuccess ? (
          <p data-testid="report-confirmation" className="text-sm">
            Thanks — your report has been submitted. We have emailed you a
            reference for it.
          </p>
        ) : (
          <div className="space-y-4">
            <div className="space-y-2" data-testid="report-category">
              <Label>Reason</Label>
              <Select
                options={categories}
                value={category}
                onChange={setCategory}
                getLabel={(c) => c.label}
                getValue={(c) => c.value}
                placeholder="Choose a reason"
              />
              {!category && (
                <p className="text-muted-foreground text-sm">
                  Choose a reason to submit your report.
                </p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="report-details">Details (optional)</Label>
              <Textarea
                id="report-details"
                data-testid="report-details"
                aria-invalid={detailsError !== undefined}
                aria-describedby={
                  detailsError ? "report-details-error" : undefined
                }
                rows={4}
                value={details ?? ""}
                onChange={(e) => setDetails(e.target.value)}
              />
              {detailsError && (
                <p
                  id="report-details-error"
                  className="text-destructive text-sm"
                >
                  {detailsError}
                </p>
              )}
            </div>

            {isError && (
              <p className="text-destructive text-sm">
                We could not submit your report. Please try again.
              </p>
            )}
          </div>
        )}

        <DialogFooter>
          {isSuccess ? (
            <Button onClick={() => close(false)}>Close</Button>
          ) : (
            <>
              <Button
                variant="ghost"
                onClick={() => close(false)}
                disabled={isPending}
              >
                Cancel
              </Button>
              <Button
                data-testid="report-submit"
                disabled={isPending || !parsed.success}
                onClick={() => submit(buffer)}
              >
                {isPending ? "Submitting..." : "Submit report"}
              </Button>
            </>
          )}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
