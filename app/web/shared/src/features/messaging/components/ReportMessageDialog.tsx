import { Controller, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import type { ReportCategory } from "../types";
import {
  REPORT_CATEGORY_LABELS,
  reportMessageRequestSchema,
  useReportMessage,
  type ReportMessageFormValues,
  type ReportMessageRequest,
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

const categories: CategoryOption[] = (
  Object.keys(REPORT_CATEGORY_LABELS) as ReportCategory[]
).map((value) => ({ value, label: REPORT_CATEGORY_LABELS[value] }));

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
  const { submit, isPending, isSuccess, isError } = useReportMessage(messageId);
  const {
    control,
    register,
    handleSubmit,
    formState: { errors, isValid },
  } = useForm<ReportMessageFormValues, unknown, ReportMessageRequest>({
    resolver: zodResolver(reportMessageRequestSchema),
    defaultValues: { details: "" },
    mode: "onChange",
  });

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
          <form
            id="report-message-form"
            onSubmit={handleSubmit(submit)}
            className="space-y-4"
          >
            <div className="space-y-2" data-testid="report-category">
              <Label>Reason</Label>
              <Controller
                control={control}
                name="category"
                render={({ field }) => (
                  <Select
                    options={categories}
                    value={categories.find(
                      (category) => category.value === field.value,
                    )}
                    onChange={(category) => field.onChange(category.value)}
                    getLabel={(category) => category.label}
                    getValue={(category) => category.value}
                    placeholder="Choose a reason"
                  />
                )}
              />
              {errors.category && (
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
                aria-invalid={errors.details !== undefined}
                aria-describedby={
                  errors.details ? "report-details-error" : undefined
                }
                rows={4}
                {...register("details")}
              />
              {errors.details && (
                <p
                  id="report-details-error"
                  className="text-destructive text-sm"
                >
                  {errors.details.message}
                </p>
              )}
            </div>

            {isError && (
              <p className="text-destructive text-sm">
                We could not submit your report. Please try again.
              </p>
            )}
          </form>
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
                type="submit"
                form="report-message-form"
                data-testid="report-submit"
                disabled={isPending || !isValid}
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
