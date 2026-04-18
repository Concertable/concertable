import { useParams, useNavigate } from "@tanstack/react-router";
import {
  useApplicationQuery,
  useAcceptApplicationMutation,
} from "@/hooks/query/useApplicationQuery";
import { usePayoutAccountStatusQuery } from "@/hooks/query/useStripeAccountQuery";
import { AcceptContractSummary } from "@/components/applications/AcceptContractSummary";
import { StripeOnboardingBanner } from "@/components/stripe/StripeOnboardingBanner";
import { Button } from "@/components/ui/button";
import { toast } from "sonner";
import dayjs from "dayjs";

export default function AcceptApplicationPage() {
  const { applicationId } = useParams({ from: "/venue/accept/$applicationId" });
  const navigate = useNavigate();
  const { data: application, isLoading } = useApplicationQuery(applicationId);
  const { mutate: accept, isPending } = useAcceptApplicationMutation(
    application?.opportunity.id ?? 0,
  );
  const { data: accountStatus } = usePayoutAccountStatusQuery(true);

  if (isLoading || !application) return null;

  const { artist, opportunity } = application;

  function handleConfirm() {
    accept(application!.id, {
      onSuccess: () => toast.success("Application accepted"),
    });
  }

  return (
    <div className="mx-auto max-w-lg space-y-6 p-6">
      <div>
        <h1 className="text-xl font-semibold">Review &amp; Accept</h1>
        <p className="text-muted-foreground mt-1 text-sm">
          {artist.name} · {dayjs(opportunity.startDate).format("D MMM YYYY")} —{" "}
          {dayjs(opportunity.endDate).format("D MMM YYYY")}
        </p>
      </div>

      <StripeOnboardingBanner />

      <div className="border-border bg-card rounded-xl border p-4">
        <AcceptContractSummary contract={opportunity.contract} />
      </div>

      <div className="flex gap-3">
        <Button
          variant="outline"
          onClick={() =>
            navigate({
              to: "/venue/my/applications/$id",
              params: { id: opportunity.id },
            })
          }
        >
          Cancel
        </Button>
        <Button
          disabled={isPending || accountStatus !== "Verified"}
          onClick={handleConfirm}
        >
          {isPending ? "Confirming..." : "Confirm"}
        </Button>
      </div>
    </div>
  );
}
