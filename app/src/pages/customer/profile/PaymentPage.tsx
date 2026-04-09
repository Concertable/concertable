import { CreditCard, ExternalLink, CheckCircle, XCircle } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { useAuthStore } from "@/store/useAuthStore";
import { toast } from "sonner";
import {
  useStripeVerifiedQuery,
  useStripeOnboardingQuery,
  usePaymentMethodQuery,
} from "@/hooks/query/useStripeAccountQuery";
import { AddPaymentMethodModal } from "@/components/AddPaymentMethodModal";
import { useMountEffect } from "@/hooks/useMountEffect";

export default function PaymentPage() {
  const user = useAuthStore((s) => s.user);
  const isManager =
    user?.role === "VenueManager" || user?.role === "ArtistManager";
  const { data: isVerified, refetch: refetchVerified } =
    useStripeVerifiedQuery(isManager);
  const { refetch: openOnboarding, isFetching } = useStripeOnboardingQuery();

  useMountEffect(() => {
    function handleMessage(event: MessageEvent) {
      if (event.origin !== window.location.origin) return;
      if (event.data?.type === "stripe_return")
        refetchVerified().then(({ data: verified }) => {
          if (verified) toast.success("Payout account verified");
          else
            toast.info(
              "Setup incomplete — finish the remaining steps to get verified",
            );
        });
      else if (event.data?.type === "stripe_refresh")
        openOnboarding().then(({ data: link }) => {
          if (link) window.open(link, "_blank");
        });
    }

    window.addEventListener("message", handleMessage);
    return () => window.removeEventListener("message", handleMessage);
  });
  const { data: paymentMethod } = usePaymentMethodQuery();

  return (
    <div className="max-w-lg space-y-8">
      <div>
        <h2 className="text-lg font-semibold">Payment & Billing</h2>
        <p className="text-muted-foreground text-sm">
          Manage your payment methods and billing details
        </p>
      </div>

      <Separator />

      <div className="space-y-4">
        <h3 className="font-medium">Payment Method</h3>
        {paymentMethod ? (
          <div className="flex items-center justify-between rounded-lg border p-4">
            <div className="flex items-center gap-3">
              <CreditCard className="text-muted-foreground size-5" />
              <div>
                <p className="text-sm font-medium capitalize">
                  {paymentMethod.brand} •••• {paymentMethod.last4}
                </p>
                <p className="text-muted-foreground text-xs">
                  Expires {paymentMethod.expMonth}/{paymentMethod.expYear}
                </p>
              </div>
            </div>
            <AddPaymentMethodModal replace />
          </div>
        ) : (
          <>
            <p className="text-muted-foreground text-sm">
              {isManager
                ? "Save a card to pay venue hire fees and other bookings without entering your details each time."
                : "Save a card to pay for tickets and concerts without entering your details each time."}
            </p>
            <div className="pt-2">
              <AddPaymentMethodModal />
            </div>
          </>
        )}
      </div>

      {isManager && (
        <>
          <Separator />

          <div className="space-y-4">
            <h3 className="font-medium">Payout Account</h3>
            <p className="text-muted-foreground text-sm">
              Connect your Stripe account to receive payments for concerts and
              bookings.
            </p>
            <div className="flex items-center gap-3 pt-2">
              {isVerified !== undefined &&
                (isVerified ? (
                  <span className="flex items-center gap-1 text-sm text-green-600">
                    <CheckCircle className="size-4" /> Verified
                  </span>
                ) : (
                  <span className="text-destructive flex items-center gap-1 text-sm">
                    <XCircle className="size-4" /> Not verified
                  </span>
                ))}
              <Button
                onClick={() =>
                  openOnboarding().then(({ data: link }) => {
                    if (link) window.open(link, "_blank");
                  })
                }
                disabled={isFetching}
              >
                <ExternalLink className="size-4" />
                {isFetching
                  ? "Loading..."
                  : isVerified
                    ? "Manage Payout Account"
                    : "Set up Payout Account"}
              </Button>
            </div>
          </div>
        </>
      )}
    </div>
  );
}
