import { useState } from "react";
import type { ReportCategory } from "../types";
import { reportMessageRequestSchema } from "@concertable/shared/features/messaging";
import { useReportMessageMutation } from "../hooks/useMessageQuery";
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

const categories: { value: ReportCategory; label: string }[] = [
  { value: "IllegalContent", label: "Illegal content" },
  { value: "Harassment", label: "Harassment or abuse" },
  { value: "Fraud", label: "Fraud or scam" },
  { value: "Spam", label: "Spam" },
  { value: "Other", label: "Something else" },
];

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
  const [category, setCategory] = useState(categories[0]);
  const [details, setDetails] = useState<string>();
  const { mutate, isPending, isSuccess, isError, reset } =
    useReportMessageMutation();

  const parsed = reportMessageRequestSchema.safeParse({
    category: category.value,
    details: details?.trim() || undefined,
  });
  const detailsError = parsed.success
    ? undefined
    : parsed.error.issues.find((issue) => issue.path[0] === "details")?.message;

  const close = (next: boolean) => {
    onOpenChange(next);
    if (!next) {
      reset();
      setDetails(undefined);
      setCategory(categories[0]);
    }
  };

  return (
    <Dialog open={open} onOpenChange={close}>
      <DialogContent>
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
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="report-details">Details (optional)</Label>
              <Textarea
                id="report-details"
                data-testid="report-details"
                aria-invalid={detailsError !== undefined}
                aria-describedby={detailsError ? "report-details-error" : undefined}
                rows={4}
                value={details ?? ""}
                onChange={(e) => setDetails(e.target.value)}
              />
            </div>

            {detailsError && (
              <p id="report-details-error" className="text-destructive text-sm">
                {detailsError}
              </p>
            )}

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
                onClick={() =>
                  parsed.success && mutate({ messageId, request: parsed.data })
                }
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
