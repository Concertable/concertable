import { useQuery } from "@tanstack/react-query";
import { useAuth } from "react-oidc-context";
import stripeAccountApi from "../api/stripeAccountApi";

export function usePayoutAccountStatusQuery(enabled: boolean) {
  return useQuery({
    queryKey: ["stripe", "account-status"],
    queryFn: stripeAccountApi.getAccountStatus,
    enabled,
    staleTime: 0,
    gcTime: 0,
  });
}

export function usePaymentMethodQuery() {
  const { isAuthenticated } = useAuth();
  return useQuery({
    queryKey: ["stripe", "payment-method"],
    queryFn: stripeAccountApi.getPaymentMethod,
    enabled: isAuthenticated,
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

export function useSetupIntentQuery(enabled: boolean) {
  return useQuery({
    queryKey: ["stripe", "setup-intent"],
    queryFn: stripeAccountApi.createSetupIntent,
    enabled,
    staleTime: Infinity,
    gcTime: 0,
  });
}
