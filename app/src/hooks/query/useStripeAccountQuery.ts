import { useQuery } from "@tanstack/react-query";
import stripeAccountApi from "@/api/stripeAccountApi";

export function useStripeVerifiedQuery(enabled: boolean) {
  return useQuery({
    queryKey: ["stripe", "verified"],
    queryFn: stripeAccountApi.isVerified,
    enabled,
  });
}

export function usePaymentMethodQuery() {
  return useQuery({
    queryKey: ["stripe", "payment-method"],
    queryFn: stripeAccountApi.getPaymentMethod,
  });
}

export function useStripeOnboardingQuery() {
  return useQuery({
    queryKey: ["stripe", "onboarding-link"],
    queryFn: stripeAccountApi.getOnboardingLink,
    enabled: false,
    throwOnError: false,
  });
}
