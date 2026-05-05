import { useParams, useNavigate } from "@tanstack/react-router";
import { usePayoutAccountStatusQuery, StripeOnboardingBanner } from "@/features/payments";
import { Button } from "@/components/ui/button";
import dayjs from "dayjs";
import {
  useApplicationQuery,
  useAcceptApplicationMutation,
} from "../hooks/useApplicationQuery";
import { AcceptContractSummary } from "../components/applications/AcceptContractSummary";

export function AcceptApplicationPage() {
  const { applicationId } = useParams({ from: "/venue/accept/$applicationId" });
  const navigate = useNavigate();
  const { data: application, isLoading } = useApplicationQuery(applicationId);
  const { data: accountStatus } = usePayoutAccountStatusQuery(true);
  const acceptMutation = useAcceptApplicationMutation(
    application?.opportunity.id ?? 0,
  );

  if (isLoading || !application) return null;

  const { artist, opportunity, actions } = application;
  const requiresCheckout = actions.checkout != null;

  function handleConfirm() {
    if (requiresCheckout) {
      void navigate({
        to: "/venue/application/checkout/$applicationId",
        params: { applicationId },
      });
      return;
    }

    acceptMutation.mutate(
      { applicationId },
      {
        onSuccess: () => {
          void navigate({
            to: "/venue/my/applications/$id",
            params: { id: opportunity.id },
          });
        },
      },
    );
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
          disabled={accountStatus !== "Verified" || acceptMutation.isPending}
          onClick={handleConfirm}
          data-testid="confirm"
        >
          {requiresCheckout ? "Continue" : "Accept"}
        </Button>
      </div>
    </div>
  );
}
