import { Button } from "@/components/ui/button";
import { useStripeAccount } from "@/hooks/useStripeAccount";

export function StripeOnboardingBanner() {
  const { isVerified, isLoading, isLoadingLink, beginOnboarding } = useStripeAccount();

  if (isLoading || isVerified) return null;

  return (
    <div className="rounded-xl border border-border bg-card p-4 flex items-center justify-between gap-4">
      <div className="space-y-0.5">
        <p className="font-medium">Complete your payment setup</p>
        <p className="text-sm text-muted-foreground">
          You need to verify your Stripe account before you can send or receive payments.
        </p>
      </div>
      <Button size="sm" disabled={isLoadingLink} onClick={beginOnboarding}>
        {isLoadingLink ? "Loading..." : "Set up payments"}
      </Button>
    </div>
  );
}
