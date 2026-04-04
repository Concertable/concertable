import { useState } from "react";
import {
  useStripeVerifiedQuery,
  useStripeOnboardingLinkQuery,
} from "@/hooks/query/useStripeAccountQuery";

export function useStripeAccount() {
  const { data: isVerified, isLoading } = useStripeVerifiedQuery();
  const [fetchLink, setFetchLink] = useState(false);
  const { data: onboardingLink, isLoading: isLoadingLink } =
    useStripeOnboardingLinkQuery(fetchLink);

  if (onboardingLink) {
    window.location.href = onboardingLink;
  }

  return {
    isVerified,
    isLoading,
    isLoadingLink,
    beginOnboarding: () => setFetchLink(true),
  };
}
