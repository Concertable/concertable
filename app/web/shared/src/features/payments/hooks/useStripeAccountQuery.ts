import { useQuery } from "@tanstack/react-query";
import { useAuth } from "react-oidc-context";
import stripeAccountApi from "../api/stripeAccountApi";

export function usePaymentMethodQuery() {
  const { isAuthenticated } = useAuth();
  return useQuery({
    queryKey: ["stripe", "payment-method"],
    queryFn: stripeAccountApi.getPaymentMethod,
    enabled: isAuthenticated,
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
