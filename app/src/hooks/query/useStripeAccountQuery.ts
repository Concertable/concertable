import { useQuery } from "@tanstack/react-query";
import * as stripeAccountApi from "@/api/stripeAccountApi";

export function useStripeVerifiedQuery() {
  return useQuery({
    queryKey: ["stripe", "verified"],
    queryFn: stripeAccountApi.isVerified,
  });
}

export function useStripeOnboardingLinkQuery(enabled: boolean) {
  return useQuery({
    queryKey: ["stripe", "onboarding-link"],
    queryFn: stripeAccountApi.getOnboardingLink,
    enabled,
  });
}
