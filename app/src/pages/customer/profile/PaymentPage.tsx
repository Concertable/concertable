import { useState } from "react";
import { ExternalLink, CheckCircle, XCircle } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { useAuthStore } from "@/store/useAuthStore";
import {
  useStripeVerifiedQuery,
  useStripeOnboardingQuery,
} from "@/hooks/query/useStripeAccountQuery";
import { AddPaymentMethodModal } from "@/components/AddPaymentMethodModal";

export default function PaymentPage() {
  const user = useAuthStore((s) => s.user);
  const isManager =
    user?.role === "VenueManager" || user?.role === "ArtistManager";

  const { data: isVerified } = useStripeVerifiedQuery(isManager);
  const { refetch: openOnboarding, isFetching } = useStripeOnboardingQuery();

  const paymentMethodDescription = isManager
    ? "Save a card to pay venue hire fees and other bookings without entering your details each time."
    : "Save a card to pay for tickets and concerts without entering your details each time.";

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
        <p className="text-muted-foreground text-sm">
          {paymentMethodDescription}
        </p>
        <div className="pt-2">
          <Button>Add Payment Method</Button>
        </div>
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
              <Button onClick={() => openOnboarding()} disabled={isFetching}>
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
