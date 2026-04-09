import {
  useStripeVerifiedQuery,
  useStripeOnboardingQuery,
} from "@/hooks/query/useStripeAccountQuery";

export function useStripeAccount() {
  const { data: isVerified, isLoading } = useStripeVerifiedQuery();
  const { refetch, isFetching: isLoadingLink } = useStripeOnboardingQuery();

  return {
    isVerified,
    isLoading,
    isLoadingLink,
    beginOnboarding: refetch,
  };
}
