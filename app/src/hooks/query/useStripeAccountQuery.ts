import { useQuery } from "@tanstack/react-query";
import { getOnboardingLink, isVerified } from "@/api/stripeAccountApi";
import { toast } from "sonner";

export function useStripeVerifiedQuery(enabled: boolean) {
  return useQuery({
    queryKey: ["stripe", "verified"],
    queryFn: isVerified,
    enabled,
  });
}

export function useStripeOnboardingQuery() {
  return useQuery({
    queryKey: ["stripe", "onboarding-link"],
    queryFn: getOnboardingLink,
    enabled: false,
    staleTime: Infinity,
    throwOnError: false,
    select: (link) => {
      window.location.href = link;
      return link;
    },
  });
}
